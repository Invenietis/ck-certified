using System;
using System.Collections.Generic;
using System.Windows.Threading;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WindowManager.Model;
using CommonServices.Accessibility;
using HighlightModel;
using SimpleSkin.ViewModels;
using CK.Windows;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;
using Host.Services;
using System.Windows.Media;
using CK.Windows.Helpers;
using SimpleSkin.Res;
using System.Linq;
using CommonServices;

namespace SimpleSkin
{
    [Plugin( KeyboardDisplayer.PluginIdString, PublicName = PluginPublicName, Version = KeyboardDisplayer.PluginIdVersion,
       Categories = new string[] { "Visual", "Accessibility" } )]
    public class KeyboardDisplayer : IPlugin
    {
        const string PluginPublicName = "KeyboardDisplayer";
        const string PluginIdString = "{D173E013-2491-4491-BF3E-CA2F8552B5EB}";
        const string PluginIdVersion = "1.0.0";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        const string PredictionKeyboardName = "Prediction";

        [ConfigurationAccessor( "{00000000-0000-0000-0000-000000000000}" )]
        public IPluginConfigAccessor SharedConfig { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ISharedData SharedData { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        [ConfigurationAccessor( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" )]
        public IPluginConfigAccessor Config { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ITopMostService> TopMostService { get; set; }

        [RequiredService]
        public INotificationService Notification { get; set; }

        class SkinInfo
        {
            public SkinInfo( SkinWindow window, VMContextActiveKeyboard vm, Dispatcher d, WindowManagerSubscriber sub )
            {
                Skin = window;
                ViewModel = vm;
                Dispatcher = d;
                Subscriber = sub;
                NameKeyboard = vm.KeyboardVM.Keyboard.Name;
            }

            public readonly WindowManagerSubscriber Subscriber;

            public readonly SkinWindow Skin;

            public readonly VMContextActiveKeyboard ViewModel;

            public readonly Dispatcher Dispatcher;

            /// <summary>
            /// must be updated manual
            /// </summary>
            public string NameKeyboard;

            public bool IsClosing;
        }

        public NoFocusManager NoFocusManager { get { return NoFocusManager.Default; } }

        IDictionary<string, SkinInfo> _skins;

        bool _viewHidden;
        MiniViewVM _miniViewVm;
        MiniView _miniView;

        public bool IsViewHidden { get { return _viewHidden; } }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            _skins = new Dictionary<string, SkinInfo>();
            return true;
        }

        public void Start()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( KeyboardContext.Service.Keyboards.Actives.Count == 0 )
            {
                ShowNoActiveKeyboardNotification();
            }
            else
            {
                foreach( var activeKeyboard in KeyboardContext.Service.Keyboards.Actives )
                {
                    InitializeActiveWindow( activeKeyboard );
                }
            }
            RegisterEvents();

            //temporary
            UnregisterPrediction();
            RegisterPrediction();
        }

        public void Stop()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            //temporary
            UnregisterPrediction();

            UnregisterEvents();

            UninitializeMiniview();

            foreach( var skin in _skins.Values )
            {
                UninitializeActiveWindows( skin );
            }

            _skins.Clear();
        }

        public void Teardown()
        {
        }

        #endregion

        #region WindowManager and WindowBinder Members

        void Subscribe( SkinInfo skinInfo )
        {
            skinInfo.Subscriber.Subscribe( skinInfo.NameKeyboard, skinInfo.Skin );
        }

        void Unsubscribe( SkinInfo skinInfo )
        {
            skinInfo.Subscriber.Unsubscribe();
        }

        #endregion WindowManager and WindowBidner Members

        #region TopMostService Methods

        void RegisterTopMostService( SkinInfo skinInfo )
        {
            if( TopMostService.Status == InternalRunningStatus.Started )
            {
                TopMostService.Service.RegisterTopMostElement( "10", skinInfo.Skin );
            }
        }

        void UnregisterTopMostService( SkinInfo skinInfo )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( TopMostService.Status == InternalRunningStatus.Started )
            {
                //skinInfo.Dispatcher.BeginInvoke( new Action( () =>
                //{
                TopMostService.Service.UnregisterTopMostElement( skinInfo.Skin );
                //} ) );
            }
        }

        void OnTopMostServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                ForEachSkin( s => RegisterTopMostService( s ) );
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                ForEachSkin( s => UnregisterTopMostService( s ) );
            }
        }

        #endregion TopMostService Methods

        #region Windows Initialization

        void InitializeActiveWindow( IKeyboard keyboard )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            var vm = new VMContextActiveKeyboard( NoFocusManager, keyboard.Name, Context, KeyboardContext.Service.Keyboards.Context, Config, SharedData );
            var subscriber = new WindowManagerSubscriber( WindowManager, WindowBinder );

            var skin = NoFocusManager.CreateNoFocusWindow<SkinWindow>( nfm => new SkinWindow( nfm )
            {
                DataContext = vm
            } );

            SkinInfo skinInfo = new SkinInfo( skin, vm, NoFocusManager.NoFocusDispatcher, subscriber );
            _skins.Add( keyboard.Name, skinInfo );

            InitializeWindowLayout( skinInfo );
            skinInfo.Dispatcher.Invoke( (Action)(() =>
            {
                skinInfo.Skin.Show();
                skinInfo.Skin.ShowInTaskbar = false;
            }) );
            SetWindowPlacement( skinInfo );

            Subscribe( skinInfo );
            RegisterHighlighter( skinInfo );
            RegisterTopMostService( skinInfo );

            RegisterSkinEvents( skinInfo );
        }

        // when we call this function, we must remove the skin from the dictionary 
        void UninitializeActiveWindows( SkinInfo skin )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );


            //Setting the config is done after closing the window because modifying a value in the Config
            //Triggers a Caliburn Micro OnNotifyPropertyChanged, which calls an Invoke on the main UI Thread.
            //generating random locks.
            //Once the LayoutManager is ready, we won't need this anymore.
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();

            UnregisterSkinEvents( skin );
            Unsubscribe( skin );
            UnregisterFromHighlighter( skin );
            UnregisterTopMostService( skin );

            placement = CKWindowTools.GetPlacement( skin.Skin.Hwnd );

            skin.ViewModel.Dispose();

            if( _skins.Count == 1 && _miniView != null && _miniView.Visibility != Visibility.Hidden )
            {
                if( Highlighter.Status.IsStartingOrStarted )
                    Highlighter.Service.UnregisterTree( _miniViewVm.Name, _miniViewVm );

                _miniView.Hide();
                _viewHidden = false;
            }

            //temporary 03/03/2014
            if( !skin.IsClosing )
            {
                skin.IsClosing = true;
                skin.Skin.Dispatcher.Invoke( (Action)(() =>
                {
                    skin.Skin.Close();
                }) );
            }

            Config.User.Set( PlacementString( skin ), placement );
        }

        private void UninitializeMiniview()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( _miniView != null )
            {
                if( Highlighter.Status.IsStartingOrStarted )
                    Highlighter.Service.UnregisterTree( _miniViewVm.Name, _miniViewVm );

                _miniView.Close();
                _viewHidden = false;
            }

        }

        string PlacementString( SkinInfo skinInfo )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( skinInfo.ViewModel.KeyboardVM != null && skinInfo.ViewModel.KeyboardVM.Keyboard != null )
                return skinInfo.ViewModel.KeyboardVM.Keyboard.Name + ".WindowPlacement";

            return String.Empty;
        }

        /// <summary>
        /// Must be called from the skin thread
        /// </summary>
        private void InitializeWindowLayout( SkinInfo skinInfo )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            int defaultWidth = skinInfo.ViewModel.KeyboardVM.W;
            int defaultHeight = skinInfo.ViewModel.KeyboardVM.H;

            if( !Config.User.Contains( PlacementString( skinInfo ) ) )
            {
                var viewPortSize = Config[skinInfo.ViewModel.KeyboardVM.Layout]["ViewPortSize"];

                skinInfo.Skin.Dispatcher.Invoke( (Action)(() =>
                {
                    if( viewPortSize != null )
                    {
                        Size size = (Size)viewPortSize;
                        SetDefaultWindowPosition( skinInfo, (int)size.Width, (int)size.Height );
                    }
                    else
                        SetDefaultWindowPosition( skinInfo, defaultWidth, defaultHeight ); //first launch : places the skin in the default position
                }) );
            }
            else
            {
                skinInfo.Skin.Dispatcher.Invoke( (Action)(() =>
                {
                    skinInfo.Skin.Width = skinInfo.Skin.Height = 0; //After the first launch : hiding the window to get its last placement from the user conf.
                }) );
            }
        }

        /// <summary>
        /// Must be called from the skin thread
        /// </summary>
        private void SetDefaultWindowPosition( SkinInfo skinInfo, int defaultWidth, int defaultHeight )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == skinInfo.Dispatcher, "This method should only be called by the window's dispatcher." );

            skinInfo.Skin.Top = 0;
            skinInfo.Skin.Left = 0;
            skinInfo.Skin.Width = defaultWidth;
            skinInfo.Skin.Height = defaultHeight;
        }

        private void SetWindowPlacement( SkinInfo skinInfo )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            var defaultPlacement = new WINDOWPLACEMENT();
            defaultPlacement = CKWindowTools.GetPlacement( skinInfo.Skin.Hwnd );
            WINDOWPLACEMENT actualPlacement = Config.User.GetOrSet( PlacementString( skinInfo ), defaultPlacement );

            skinInfo.Dispatcher.Invoke( (Action)(() => CKWindowTools.SetPlacement( skinInfo.Skin.Hwnd, actualPlacement )), null );
        }

        #endregion Windows Initialization

        #region Hightlight Methods

        void OnHighlighterServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( e.Current == InternalRunningStatus.Started )
            {
                if( _viewHidden == true )
                {
                    if( Highlighter.Status == InternalRunningStatus.Started )
                    {
                        Highlighter.Service.RegisterTree( _miniViewVm.Name, R.MiniViewName, _miniViewVm );
                    }
                }
                else
                {
                    ForEachSkin( s => RegisterHighlighter( s ) );
                    DirtyTemporaryPredictionInjectionInCurrentKeyboard();
                }
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                if( _viewHidden == true )
                {
                    if( Highlighter.Status == InternalRunningStatus.Started )
                    {
                        Highlighter.Service.UnregisterTree( _miniViewVm.Name, _miniViewVm );
                    }
                }
                else
                {
                    ForEachSkin( s => UnregisterFromHighlighter( s ) );
                    UnregisterPrediction();
                }
            }
        }

        private void UnregisterFromHighlighter( SkinInfo skinInfo )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( Highlighter.Status.IsStartingOrStarted )
            {
                Highlighter.Service.UnregisterTree( skinInfo.NameKeyboard, skinInfo.ViewModel.KeyboardVM );
            }
        }

        private void RegisterHighlighter( SkinInfo skinInfo )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                Highlighter.Service.RegisterTree( skinInfo.NameKeyboard, skinInfo.NameKeyboard,
                    new ExtensibleHighlightableElementProxy( skinInfo.NameKeyboard, skinInfo.ViewModel.KeyboardVM, true ) );
            }
        }

        #endregion

        #region Prediction temporary region

        string _includedKeyboardName = string.Empty;
        //if the target keyboard isn't active and isn't registered, the prediction is registered as a module (a root element)
        private void RegisterPrediction()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                if( _skins.ContainsKey( PredictionKeyboardName ) )
                {
                    if( KeyboardContext.Status == InternalRunningStatus.Started )
                    {
                        //if the current isn't registered
                        if( Highlighter.Service.RegisterInRegisteredElementAt( KeyboardContext.Service.CurrentKeyboard.Name, KeyboardContext.Service.CurrentKeyboard.Name, ChildPosition.Pre, _skins[PredictionKeyboardName].ViewModel.KeyboardVM ) )
                        {
                            Object o = Config[KeyboardContext.Service.CurrentKeyboard.CurrentLayout]["HighlightBackground"];
                            if( o != null ) Config[_skins[PredictionKeyboardName].ViewModel.KeyboardVM.Keyboard.CurrentLayout]["HighlightBackground"] = o;
                            _includedKeyboardName = KeyboardContext.Service.CurrentKeyboard.Name;
                            return;
                        }
                    }
                    Config[_skins[PredictionKeyboardName].ViewModel.KeyboardVM.Keyboard.CurrentLayout]["HighlightBackground"] = ColorConverter.ConvertFromString( "#FFBDCFF4" );
                    VMKeyboardSimple elem = _skins[PredictionKeyboardName].ViewModel.KeyboardVM;
                    elem.IsHighlightableTreeRoot = false;

                    Highlighter.Service.RegisterTree( PredictionKeyboardName, PredictionKeyboardName, elem );
                }
            }
        }

        private void UnregisterPrediction()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                if( _skins.ContainsKey( PredictionKeyboardName ) )
                {
                    if( KeyboardContext.Status == InternalRunningStatus.Started )
                    {
                        if( !string.IsNullOrEmpty( _includedKeyboardName ) && Highlighter.Service.UnregisterInRegisteredElement( _includedKeyboardName, _includedKeyboardName, ChildPosition.Pre, _skins[PredictionKeyboardName].ViewModel.KeyboardVM ) )
                        {
                            _includedKeyboardName = string.Empty;
                            return;
                        }
                    }

                    Highlighter.Service.UnregisterTree( PredictionKeyboardName, _skins[PredictionKeyboardName].ViewModel.KeyboardVM );
                    Highlighter.Service.UnregisterInRegisteredElement( KeyboardContext.Service.CurrentKeyboard.Name, KeyboardContext.Service.CurrentKeyboard.Name, ChildPosition.Pre, _skins[PredictionKeyboardName].ViewModel.KeyboardVM );
                    _includedKeyboardName = string.Empty;
                }
            }
        }

        private void DirtyTemporaryPredictionInjectionInCurrentKeyboard()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            UnregisterPrediction();
            RegisterPrediction();
        }

        #endregion

        #region Register Unregister Events

        private void RegisterEvents()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            KeyboardContext.Service.Keyboards.KeyboardActivated += OnKeyboardActivated;
            KeyboardContext.Service.Keyboards.KeyboardDeactivated += OnKeyboardDeactivated;
            KeyboardContext.Service.Keyboards.KeyboardDestroyed += OnKeyboardDestroyed;
            KeyboardContext.Service.Keyboards.KeyboardCreated += OnKeyboardCreated;
            KeyboardContext.Service.Keyboards.KeyboardRenamed += OnKeyboardRenamed;

            TopMostService.ServiceStatusChanged += OnTopMostServiceStatusChanged;
            Highlighter.ServiceStatusChanged += OnHighlighterServiceStatusChanged;
        }

        private void RegisterSkinEvents( SkinInfo skinInfo )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            //temporary 03/03/2014
            skinInfo.Skin.StateChanged += OnStateChanged;
            skinInfo.Skin.Closing += new CancelEventHandler( OnWindowClosing );
        }

        private void UnregisterEvents()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            KeyboardContext.Service.Keyboards.KeyboardActivated -= OnKeyboardActivated;
            KeyboardContext.Service.Keyboards.KeyboardDeactivated -= OnKeyboardDeactivated;
            KeyboardContext.Service.Keyboards.KeyboardDestroyed -= OnKeyboardDestroyed;
            KeyboardContext.Service.Keyboards.KeyboardCreated -= OnKeyboardCreated;
            KeyboardContext.Service.Keyboards.KeyboardRenamed -= OnKeyboardRenamed;

            TopMostService.ServiceStatusChanged -= OnTopMostServiceStatusChanged;
            Highlighter.ServiceStatusChanged -= OnHighlighterServiceStatusChanged;

            ForEachSkin( UnregisterSkinEvents );
        }

        private void UnregisterSkinEvents( SkinInfo skinInfo )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            //temporary 03/03/2014
            skinInfo.Skin.StateChanged -= OnStateChanged;

            skinInfo.Skin.Closing -= new CancelEventHandler( OnWindowClosing );
        }

        #endregion Register Unregister Events

        #region OnXXXX

        void OnKeyboardDeactivated( object sender, KeyboardEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            Debug.Assert( _skins.ContainsKey( e.Keyboard.Name ) );

            UninitializeActiveWindows( _skins[e.Keyboard.Name] );
            UnregisterPrediction();
            _skins.Remove( _skins[e.Keyboard.Name].ViewModel.KeyboardVM.Keyboard.Name );
            RegisterPrediction();
        }

        void OnKeyboardActivated( object sender, KeyboardEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( _viewHidden && WindowManager.Status.IsStartingOrStarted ) WindowManager.Service.RestoreAllWindows();
            InitializeActiveWindow( e.Keyboard );
            DirtyTemporaryPredictionInjectionInCurrentKeyboard();
        }

        void OnKeyboardRenamed( object sender, KeyboardRenamedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            SkinInfo skin;
            if( _skins.TryGetValue( e.PreviousName, out skin ) )
            {
                _skins.Remove( e.PreviousName );
                UnregisterFromHighlighter( skin );
                Unsubscribe( skin );

                skin.NameKeyboard = e.Keyboard.Name;
                _skins.Add( skin.NameKeyboard, skin );
                RegisterHighlighter( skin );
                Subscribe( skin );

            }
        }

        void OnKeyboardDestroyed( object sender, KeyboardEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( e.Keyboard.IsActive )
            {
                OnKeyboardDeactivated( sender, e );
            }
        }

        void OnKeyboardCreated( object sender, KeyboardEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( e.Keyboard.IsActive && !_skins.ContainsKey( e.Keyboard.Name ) )
            {
                OnKeyboardActivated( sender, e );
            }
        }

        #region temporary 03/03/2014

        void OnStateChanged( object sender, EventArgs e )
        {
            SkinWindow window = sender as SkinWindow;
            if( window != null )
            {
                WindowState state = window.WindowState;

                NoFocusManager.ExternalDispatcher.BeginInvoke( (Action)(() =>
                {
                    if( state == WindowState.Minimized )
                    {
                        if( !_viewHidden )
                        {
                            _viewHidden = true;

                            ShowMiniView();

                            if( Highlighter.Status == InternalRunningStatus.Started )
                            {
                                Highlighter.Service.RegisterTree( _miniViewVm.Name, R.MiniViewName, _miniViewVm );
                                ForEachSkin( UnregisterFromHighlighter );
                            }
                        }

                        if( WindowManager.Status.IsStartingOrStarted ) WindowManager.Service.MinimizeAllWindows();
                    }
                    else if( state == WindowState.Normal )
                    {
                        RestoreSkin();
                    }

                    //temporary
                    UnregisterPrediction();
                }) );
            }
        }

        void ShowMiniView()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( _miniView == null )
            {
                _miniViewVm = new MiniViewVM( this );

                _miniView = new MiniView( RestoreSkin ) { DataContext = _miniViewVm };
                _miniView.Show();

                if( !ScreenHelper.IsInScreen( new System.Drawing.Point( _miniViewVm.X + (int)_miniView.ActualWidth / 2, _miniViewVm.Y + (int)_miniView.ActualHeight / 2 ) ) ||
                !ScreenHelper.IsInScreen( new System.Drawing.Point( _miniViewVm.X + (int)_miniView.ActualWidth, _miniViewVm.Y + (int)_miniView.ActualHeight ) ) )
                {
                    _miniView.Left = 0;
                    _miniView.Top = 0;
                }
            }
            else
            {
                _miniView.Show();
            }
        }

        /// <summary>
        /// Hides the keyboard's MiniView and shows the keyboard
        /// </summary>
        public void RestoreSkin()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( _miniView != null && _miniView.Visibility != Visibility.Hidden )
            {
                if( Highlighter.Status.IsStartingOrStarted )
                    Highlighter.Service.UnregisterTree( _miniViewVm.Name, _miniViewVm );
                _miniView.Hide();
            }

            //Dispatched afterwards
            if( WindowManager.Status.IsStartingOrStarted ) WindowManager.Service.RestoreAllWindows();

            ForEachSkin( RegisterHighlighter );
            DirtyTemporaryPredictionInjectionInCurrentKeyboard();
            _viewHidden = false;
        }

        #endregion temporary

        void OnWindowClosing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            SkinWindow sw = sender as SkinWindow;
            Debug.Assert( sw != null );

            if( !_skins.Values.Single( s => s.Skin == sw ).IsClosing )
                e.Cancel = true;
        }

        #endregion

        #region Helper

        /// <summary>
        /// Calls the action for each skin.
        /// As skins can belong to different dispatchers, you may specify whether it should make sure the call is made through each skin Dispatcher.
        /// </summary>
        /// <param name="action">the action to call on each skin</param>
        /// <param name="dispatch">whether this aciton should be called on a skin's own dispatcher</param>
        /// <param name="synchronous">Only taken into account if dispatch == true</param>
        private void ForEachSkin( Action<SkinInfo> action )
        {
            foreach( var skin in _skins.Values )
            {
                action( skin );
            }
        }

        private void ShowNoActiveKeyboardNotification()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            //TODO : re-enable
            if( Notification != null )
            {
                Notification.ShowNotification( PluginId.UniqueId, "Aucun clavier n'est actif",
                    "Aucun clavier n'est actif, veuillez activer un clavier.", 1000, NotificationTypes.Warning );
            }
        }

        #endregion Helper
    }
}
