using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows;
using CK.WordPredictor.Model;
using CK.WordPredictor.UI.ViewModels;
using CK.WordPredictor.UI.Views;
using CommonServices;
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

        #region fields

        AutonomousWordPredictorViewModel _predictorViewModel;
        AutonomousWordPredictorView _predictorView;
        IKeyboard _currentKeyboard;

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
        public IPluginConfigAccessor Config { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ISharedData SharedData { get; set; }

        #endregion

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _currentKeyboard = KeyboardContext.Service.CurrentKeyboard ?? KeyboardContext.Service.Keyboards.First();
            _predictorViewModel = new AutonomousWordPredictorViewModel( _currentKeyboard.CurrentLayout, Config, SharedData, WordPredictorService.Service, CommandManager.Service );
            _predictorView = NoFocusManager.Default.CreateNoFocusWindow<AutonomousWordPredictorView>( nfm => new AutonomousWordPredictorView( nfm )
            {
                DataContext = _predictorViewModel
            } );

            _predictorView.Dispatcher.Invoke( (Action)(() =>
                {
                    _predictorView.Show();
                }) );

            _predictorViewModel.OnCurrentKeyboardChanged( _currentKeyboard.CurrentLayout );

            KeyboardContext.Service.CurrentKeyboardChanged += OnCurrentKeyboardChanged;
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
        }

        #endregion

        void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            if( e.Current != null )
            {
                _currentKeyboard = KeyboardContext.Service.CurrentKeyboard;
                _predictorViewModel.OnCurrentKeyboardChanged( _currentKeyboard.CurrentLayout );
            }
        }
    }
}
