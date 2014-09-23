using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WindowManager.Model;
using CK.Windows;
using CK.WordPredictor.Model;
using CK.WordPredictor.UI.ViewModels;
using CK.WordPredictor.UI.Views;
using CommonServices;
using CommonServices.Accessibility;
using HighlightModel;

namespace CK.WordPredictor.UI
{
    [Plugin( AutonomousWordPredictorPlugin.PluginIdString, PublicName = PluginPublicName, Version = AutonomousWordPredictorPlugin.PluginIdVersion, Categories = new string[] { "Visual", "Accessibility" } )]
    public class AutonomousWordPredictorPlugin : IPlugin
    {
        #region Plugin description

        const string PluginPublicName = "Autonomous Word Predictor Plugin";
        const string PluginIdString = "{C78A5CC8-449F-4A73-88B4-A8CDC3D88534}";
        const string PluginIdVersion = "1.0.0";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        #endregion

        const string PredictionName = "Prediction"; 

        #region fields

        AutonomousWordPredictorViewModel _predictorViewModel;
        AutonomousWordPredictorView _predictorView;
        IKeyboard _currentKeyboard;
        WindowManagerSubscriber _subscriber;

        #endregion fields

        #region Dependencies

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IWordPredictorService> WordPredictorService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IWordPredictorFeature> Feature { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ICommandManagerService> CommandManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [ConfigurationAccessor( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" )]
        public IPluginConfigAccessor LayoutKeyboardConfig { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ISharedData SharedData { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ITopMostService> TopMostService { get; set; }

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        #endregion

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _currentKeyboard = KeyboardContext.Service.CurrentKeyboard ?? KeyboardContext.Service.Keyboards.First();

            InitializeWindow();
            
            _predictorViewModel.OnCurrentKeyboardChanged( _currentKeyboard.CurrentLayout );

            KeyboardContext.Service.CurrentKeyboardChanged += OnCurrentKeyboardChanged;

            InitializeHighlighter();
            InitializeTopMostService();
        }

        public void Stop()
        {
            UninitializeTopMostService();
            UninitializeHighlighter();
            KeyboardContext.Service.CurrentKeyboardChanged += OnCurrentKeyboardChanged;
            UninitializeWindow();
        }

        public void Teardown()
        {
        }

        #endregion

        private void InitializeWindow()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            _subscriber = new WindowManagerSubscriber( WindowManager, WindowBinder );
            _predictorViewModel = new AutonomousWordPredictorViewModel( _currentKeyboard.CurrentLayout, LayoutKeyboardConfig, Config, SharedData, WordPredictorService.Service, CommandManager.Service );
            _predictorView = NoFocusManager.Default.CreateNoFocusWindow<AutonomousWordPredictorView>( nfm => new AutonomousWordPredictorView( nfm )
            {
                DataContext = _predictorViewModel
            } );

            _subscriber.Subscribe( PredictionName, _predictorView );

            _predictorView.Dispatcher.Invoke( (Action)(() =>
            {
                _predictorView.Show();
            }) );
        }

        private void UninitializeWindow()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            _subscriber.Unsubscribe();
            _predictorViewModel.Dispose();
            _predictorView.Dispatcher.Invoke( (Action)(() =>
            {
                _predictorView.Close();
            }) );
        }

        #region Highlighter Members

        private void InitializeHighlighter()
        {
            Highlighter.ServiceStatusChanged += Highlighter_ServiceStatusChanged;

            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                RegisterHighlighter();
            }
        }

        private void UninitializeHighlighter()
        {
            Highlighter.ServiceStatusChanged -= Highlighter_ServiceStatusChanged;
            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                Highlighter.Service.ElementRegisteredOrUnregistered -= OnElementRegisteredOrUnregistered;
                if( Highlighter.Service.RegisteredElements.ContainsKey( _currentKeyboard.Name ) )
                {
                    Highlighter.Service.RegisterInRegisteredElementAt( _currentKeyboard.Name, _currentKeyboard.Name, ChildPosition.Post, _predictorViewModel );
                }
            }
        }

        private void Highlighter_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                RegisterHighlighter();
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                Highlighter.Service.ElementRegisteredOrUnregistered -= OnElementRegisteredOrUnregistered;
            }
        }

        private void RegisterHighlighter()
        {
            Highlighter.Service.ElementRegisteredOrUnregistered += OnElementRegisteredOrUnregistered;

            if( Highlighter.Service.RegisteredElements.ContainsKey( _currentKeyboard.Name ) )
            {
                Highlighter.Service.RegisterInRegisteredElementAt( _currentKeyboard.Name, _currentKeyboard.Name, ChildPosition.Pre, _predictorViewModel );
            }
        }

        /// <summary>
        /// This method detect if the curetn keyboard is register after this.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnElementRegisteredOrUnregistered( object sender, HighlightElementRegisterEventArgs e )
        {
            if( e.HasRegistered && e.InternalName == _currentKeyboard.Name )
            {
                Highlighter.Service.RegisterInRegisteredElementAt( _currentKeyboard.Name, _currentKeyboard.Name, ChildPosition.Pre, _predictorViewModel );
            }
        }

        #endregion Highlighter Members

        #region TopMost Members

        private void InitializeTopMostService()
        {
            TopMostService.ServiceStatusChanged += TopMostService_ServiceStatusChanged;
            if( TopMostService.Status == InternalRunningStatus.Started)
            {
                TopMostService.Service.RegisterTopMostElement( "10", _predictorView );
            }
        }

        private void UninitializeTopMostService()
        {
            if( TopMostService.Status == InternalRunningStatus.Started )
            {
                TopMostService.Service.UnregisterTopMostElement( _predictorView );
            }
            TopMostService.ServiceStatusChanged -= TopMostService_ServiceStatusChanged;
        }

        void TopMostService_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                TopMostService.Service.RegisterTopMostElement( "10", _predictorView );
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                TopMostService.Service.UnregisterTopMostElement( _predictorView );
            }
        }

        #endregion TopMost Members

        private void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            if( e.Current != null )
            {
                _currentKeyboard = KeyboardContext.Service.CurrentKeyboard;
                _predictorViewModel.OnCurrentKeyboardChanged( _currentKeyboard.CurrentLayout );
            }
        }
    }
}
