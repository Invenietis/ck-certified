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
using System.Linq;
using CK.Windows;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;
using Host.Services;
using System.Windows.Media;
using CK.Windows.Helpers;

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
            }

            public readonly WindowManagerSubscriber Subscriber;

            public readonly SkinWindow Skin;

            public readonly VMContextActiveKeyboard ViewModel;

            public readonly Dispatcher Dispatcher;

            public string NameKeyboard { get { return ViewModel.KeyboardVM.Keyboard.Name; } }

            public bool IsClosing;
        }

        class RegisteredElementInfo
        {
            public RegisteredElementInfo( string targetModuleName, string extensibleElementName, ChildPosition position, SkinInfo skin )
            {
                TargetModuleName = targetModuleName;
                ExtensibleElementName = extensibleElementName;
                Position = position;
                SkinInfo = skin;
            }

            public readonly string TargetModuleName;
            public readonly string ExtensibleElementName;
            public readonly ChildPosition Position;
            public readonly SkinInfo SkinInfo;
        }

        #region IPlugin Members

        IDictionary<string, SkinInfo> _skins;
        IDictionary<string, RegisteredElementInfo> _registeredElementInfo;

        bool _viewHidden;
        MiniViewVM _miniViewVm;
        MiniView _miniView;
        public bool IsViewHidden { get { return _viewHidden; } }

        public bool Setup( IPluginSetupInfo info )
        {
            _skins = new Dictionary<string, SkinInfo>();
            _registeredElementInfo = new Dictionary<string, RegisteredElementInfo>();
            return true;
        }

        public void Start()
        {
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

        void InitializeActiveWindow( IKeyboard keyboard )
        {
            var vm = new VMContextActiveKeyboard( keyboard.Name, Context, KeyboardContext.Service.Keyboards.Context, Config, NoFocusManager.Default.NoFocusDispatcher );
            NoFocusManager.Default.NoFocusDispatcher.Invoke( (Action)(() =>
            {
                var subscriber = new WindowManagerSubscriber( WindowManager, WindowBinder );

                bool sitb = keyboard.Name != "Prediction";
                var skin = NoFocusManager.Default.CreateNoFocusWindow<SkinWindow>( nfm => new SkinWindow( nfm )
                {
                    ShowInTaskbar = sitb,
                    DataContext = vm
                } );

                SkinInfo skinInfo = new SkinInfo( skin, vm, NoFocusManager.Default.NoFocusDispatcher, subscriber );
                _skins.Add( keyboard.Name, skinInfo );

                InitializeWindowLayout( skinInfo );

                skinInfo.Skin.Show();

                SetWindowPlacement( skinInfo );

                Subscribe( skinInfo );
                RegisterHighlighter( skinInfo );
                RegisterTopMostService( skinInfo );

                RegisterSkinEvents( skinInfo );

            }), null );
        }

        void UninitializeActiveWindows( SkinInfo skin )
        {
            skin.ViewModel.Dispose();
            //Setting the config is done after closing the window because modifying a value in the Config
            //Triggers a Caliburn Micro OnNotifyPropertyChanged, which calls an Invoke on the main UI Thread.
            //generating random locks.
            //Once the LayoutManager is ready, we won't need this anymore.
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            skin.Dispatcher.Invoke( (Action)(() =>
            {
                UnregisterSkinEvents( skin );
                Unsubscribe( skin );
                UnregisterHighlighter( skin );
                RegisterTopMostService( skin );
                //temporary 03/03/2014
                if( _skins.Count == 1 && _miniView != null && _miniView.Visibility != Visibility.Hidden )
                {
                    if( Highlighter.Status.IsStartingOrStarted )
                        Highlighter.Service.UnregisterTree( _miniViewVm.Name, _miniViewVm );
                    _miniView.Hide();
                    _viewHidden = false;
                }

                placement = CKWindowTools.GetPlacement( skin.Skin.Hwnd );

                if( !skin.IsClosing )
                {
                    skin.IsClosing = true;
                    skin.Skin.Close();
                }

            }) );

            Config.User.Set( PlacementString( skin ), placement );

            _skins.Remove( skin.ViewModel.KeyboardVM.Keyboard.Name );
        }

        //TODOF i think, we can factorize this
        public void Stop()
        {
            //temporary
            UnregisterPrediction();

            UnregisterEvents();

            UninitializeMiniview();

            foreach( var skin in _skins.Values )
            {
                UninitializeActiveWindows( skin );
            }

            Debug.Assert( _registeredElementInfo.Count == 0 );

            _skins.Clear();
        }

        private void UninitializeMiniview()
        {
            _miniView.Dispatcher.Invoke( (Action)(() =>
                {
                    if( _miniView != null )
                    {
                        Highlighter.Service.UnregisterTree( _miniViewVm.Name, _miniViewVm );
                        _miniView.Close();
                        _viewHidden = false;
                    }
                }) );
        }

        public void Teardown()
        {
        }

        #endregion

        #region WindowManager and WindowBinder Members

        void Subscribe( SkinInfo skinInfo )
        {
            skinInfo.Subscriber.Subscribe( skinInfo.ViewModel.KeyboardVM.Keyboard.Name, skinInfo.Skin );
        }

        void Unsubscribe( SkinInfo skinInfo )
        {
            skinInfo.Subscriber.Unsubscribe();
        }

        #endregion WindowManager and WindowBidner Members

        #region TopMostService

        void RegisterTopMostService( SkinInfo skinInfo )
        {
            if( TopMostService.Status == InternalRunningStatus.Started )
            {
                TopMostService.Service.RegisterTopMostElement( "10", skinInfo.Skin );
            }
        }

        void UnregisterTopMostService( SkinInfo skinInfo )
        {
            if( TopMostService.Status == InternalRunningStatus.Started )
            {
                skinInfo.Dispatcher.BeginInvoke( new Action( () =>
                {
                    TopMostService.Service.UnregisterTopMostElement( skinInfo.Skin );
                } ) );
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

        #endregion TopMostService

        string PlacementString( SkinInfo skinInfo )
        {
            if( skinInfo.ViewModel.KeyboardVM != null && skinInfo.ViewModel.KeyboardVM.Keyboard != null )
                return skinInfo.ViewModel.KeyboardVM.Keyboard.Name + ".WindowPlacement";
            return "";
        }

        /// <summary>
        /// Must be called from the skin thread
        /// </summary>
        private void InitializeWindowLayout( SkinInfo skinInfo )
        {
            int defaultWidth = skinInfo.ViewModel.KeyboardVM.W;
            int defaultHeight = skinInfo.ViewModel.KeyboardVM.H;

            if( !Config.User.Contains( PlacementString( skinInfo ) ) )
            {
                var viewPortSize = Config[skinInfo.ViewModel.KeyboardVM.Layout]["ViewPortSize"];
                if( viewPortSize != null )
                {
                    Size size = (Size)viewPortSize;
                    SetDefaultWindowPosition( skinInfo, (int)size.Width, (int)size.Height );
                }
                else
                    SetDefaultWindowPosition( skinInfo, defaultWidth, defaultHeight ); //first launch : places the skin in the default position
            }
            else skinInfo.Skin.Width = skinInfo.Skin.Height = 0; //After the first launch : hiding the window to get its last placement from the user conf.
        }

        /// <summary>
        /// Must be called from the skin thread
        /// </summary>
        private void SetDefaultWindowPosition( SkinInfo skinInfo, int defaultWidth, int defaultHeight )
        {
            skinInfo.Skin.Top = 0;
            skinInfo.Skin.Left = 0;
            skinInfo.Skin.Width = defaultWidth;
            skinInfo.Skin.Height = defaultHeight;
        }

        private void SetWindowPlacement( SkinInfo skinInfo )
        {
            var defaultPlacement = new WINDOWPLACEMENT();
            defaultPlacement = CKWindowTools.GetPlacement( skinInfo.Skin.Hwnd );

            WINDOWPLACEMENT actualPlacement = Config.User.GetOrSet( PlacementString( skinInfo ), defaultPlacement );
            skinInfo.Dispatcher.Invoke( (Action)( () => CKWindowTools.SetPlacement( skinInfo.Skin.Hwnd, actualPlacement ) ), null );
        }

        #region Hightlight Methods

        void OnHighlighterServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                if( _viewHidden == true )
                {
                    _miniView.Dispatcher.BeginInvoke( (Action)(() =>
                    {
                        if( Highlighter.Status == InternalRunningStatus.Started )
                        {
                            Highlighter.Service.RegisterTree( _miniViewVm.Name, _miniViewVm );
                        }
                    }) );
                }
                else
                {
                    ForEachSkin( s =>
                    {
                        if( !_registeredElementInfo.ContainsKey( s.ViewModel.KeyboardVM.Keyboard.Name ) ) RegisterHighlighter( s );
                    } );
                    foreach( var element in _registeredElementInfo.Values ) Highlighter.Service.RegisterInRegisteredElementAt( element.TargetModuleName, element.ExtensibleElementName, element.Position, element.SkinInfo.ViewModel.KeyboardVM );
                    UnregisterPrediction();
                    RegisterPrediction();
                }
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                if( _viewHidden == true )
                {
                    _miniView.Dispatcher.BeginInvoke( (Action)(() =>
                    {
                        if( Highlighter.Status == InternalRunningStatus.Started )
                        {
                            Highlighter.Service.UnregisterTree( _miniViewVm.Name, _miniViewVm );
                        }
                    }) );
                }
                else
                {
                    ForEachSkin( s =>
                    {
                        if( !_registeredElementInfo.ContainsKey( s.ViewModel.KeyboardVM.Keyboard.Name ) ) UnregisterHighlighter( s );
                    } );
                    UnregisterPrediction();
                }
            }
        }

        private void UnregisterHighlighter( SkinInfo skinInfo )
        {
            if( Highlighter.Status.IsStartingOrStarted )
            {
                Highlighter.Service.UnregisterTree( skinInfo.ViewModel.KeyboardVM.Keyboard.Name, skinInfo.ViewModel.KeyboardVM );
            }
        }

        private void RegisterHighlighter( SkinInfo skinInfo )
        {
            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                Highlighter.Service.RegisterTree( skinInfo.ViewModel.KeyboardVM.Keyboard.Name,
                    new ExtensibleHighlightableElementProxy( skinInfo.ViewModel.KeyboardVM.Keyboard.Name, skinInfo.ViewModel.KeyboardVM ) );
            }
        }

        void RegisterInRegisteredElement( RegisteredElementInfo element )
        {
            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                Highlighter.Service.RegisterInRegisteredElementAt( element.TargetModuleName, element.ExtensibleElementName, element.Position, element.SkinInfo.ViewModel.KeyboardVM );
            }
            _registeredElementInfo.Add( element.SkinInfo.ViewModel.KeyboardVM.Keyboard.Name, element );
        }

        void UnregisterInRegisteredElement( RegisteredElementInfo element )
        {
            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                Highlighter.Service.UnregisterInRegisteredElement( element.TargetModuleName, element.ExtensibleElementName, element.Position, element.SkinInfo.ViewModel.KeyboardVM );
            }
            _registeredElementInfo.Remove( element.SkinInfo.ViewModel.KeyboardVM.Keyboard.Name );
        }

        #region Prediction temporary region

        string _includedKeyboardName = string.Empty;
        //if the target keyboard isn't active and isn't registered, the prediction is registered as a module (a root element)
        private void RegisterPrediction()
        {
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
                    Highlighter.Service.RegisterTree( PredictionKeyboardName, _skins[PredictionKeyboardName].ViewModel.KeyboardVM );
                }
            }
        }

        private void UnregisterPrediction()
        {
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
                    _includedKeyboardName = string.Empty;
                }
            }
        }

        #endregion

        #endregion

        #region OnXXXX

        private void RegisterEvents()
        {
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
            //temporary 03/03/2014
            skinInfo.Skin.StateChanged += OnStateChanged;

            skinInfo.Skin.Closing += new CancelEventHandler( OnWindowClosing );
        }

        private void UnregisterEvents()
        {
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
            //temporary 03/03/2014
            skinInfo.Skin.StateChanged -= OnStateChanged;

            skinInfo.Skin.Closing -= new CancelEventHandler( OnWindowClosing );
        }

        void OnKeyboardDeactivated( object sender, KeyboardEventArgs e )
        {
            Debug.Assert( _skins.ContainsKey( e.Keyboard.Name ) );

            SkinInfo skin = _skins[e.Keyboard.Name];
            UninitializeActiveWindows( skin );
            //temporary
            UnregisterPrediction();
            RegisterPrediction();
        }

        void OnKeyboardActivated( object sender, KeyboardEventArgs e )
        {
            InitializeActiveWindow( e.Keyboard );
            UnregisterPrediction();
            RegisterPrediction();
        }

        void OnKeyboardRenamed( object sender, KeyboardRenamedEventArgs e )
        {
            SkinInfo skin;
            if( _skins.TryGetValue( e.PreviousName, out skin ) )
            {
                _skins.Remove( e.PreviousName );
                _skins.Add( e.Keyboard.Name, skin );

                //Update name with Unregister and Register
                if( Highlighter.Status == InternalRunningStatus.Started )
                {
                    RegisteredElementInfo element;
                    if( _registeredElementInfo.TryGetValue( e.PreviousName, out element ) )
                    {
                        UnregisterInRegisteredElement( element );
                        RegisterInRegisteredElement( element );
                    }
                }
            }
        }

        void OnKeyboardDestroyed( object sender, KeyboardEventArgs e )
        {
            if( e.Keyboard.IsActive )
            {
                Debug.Assert( _skins.ContainsKey( e.Keyboard.Name ) );
                SkinInfo skin = _skins[e.Keyboard.Name];
                UninitializeActiveWindows( skin );
            }
        }

        void OnKeyboardCreated( object sender, KeyboardEventArgs e )
        {
            if( e.Keyboard.IsActive && !_skins.ContainsKey( e.Keyboard.Name ) )
            {
                InitializeActiveWindow( e.Keyboard );
            }
        }

        #region temporary 03/03/2014

        void OnStateChanged( object sender, EventArgs e )
        {
            SkinWindow window = sender as SkinWindow;
            if( window != null && window.WindowState == WindowState.Minimized )
            {
                ForEachSkin( HideSkin );

                if( WindowManager.Status.IsStartingOrStarted ) WindowManager.Service.MinimizeAllWindows();

                //temporary
                UnregisterPrediction();
            }
            if( window != null && window.WindowState == WindowState.Normal )
            {
                RestoreSkin();
                //temporary
                UnregisterPrediction();
            }
        }

        /// <summary>
        /// Hides the skin and shows the keyboard's MiniView
        /// </summary>
        void HideSkin( SkinInfo skinInfo )
        {
            if( !_viewHidden )
            {
                _viewHidden = true;

                skinInfo.Dispatcher.BeginInvoke( (Action)(() =>
                {
                    ShowMiniView( skinInfo );
                    if( Highlighter.Status == InternalRunningStatus.Started )
                    {
                        Highlighter.Service.RegisterTree( _miniViewVm.Name, _miniViewVm );
                        UnregisterHighlighter( skinInfo );
                    }
                }), null );
            }
        }

        void ShowMiniView( SkinInfo skinInfo )
        {
            if( _miniView == null )
            {
                _miniViewVm = new MiniViewVM( this );

                _miniView = new MiniView( RestoreSkin ) { DataContext = _miniViewVm };
                _miniView.Closing += OnWindowClosing;
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
            _miniView.Dispatcher.BeginInvoke( (Action)(() =>
                {
                    if( _miniView.Visibility != Visibility.Hidden )
                    {
                        if( Highlighter.Status.IsStartingOrStarted )
                            Highlighter.Service.UnregisterTree( _miniViewVm.Name, _miniViewVm );
                        _miniView.Hide();
                    }
                }) );

            if( WindowManager.Status.IsStartingOrStarted ) WindowManager.Service.RestoreAllWindows();

            ForEachSkin( RegisterHighlighter );

            //temporary
            UnregisterPrediction();
            RegisterPrediction();
            _viewHidden = false;
        }

        #endregion temporary

        void OnWindowClosing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            SkinWindow sw = sender as SkinWindow;

            Debug.Assert( sw != null );
            Debug.Assert( _skins.Values.FirstOrDefault( si => si.Skin == sw ) != null );

            SkinInfo skinInfo = _skins.Values.First( si => si.Skin == sw );
            if( !skinInfo.IsClosing )
            {
                skinInfo.IsClosing = true;
                skinInfo.ViewModel.KeyboardVM.Keyboard.IsActive = false;
            }
            //temporary
            UnregisterPrediction();
            RegisterPrediction();
        }

        #endregion

        #region Helper

        private void ForEachSkin( Action<SkinInfo> action )
        {
            foreach( var skin in _skins.Values )
            {
                action( skin );
            }
        }

        private void ShowNoActiveKeyboardNotification()
        {
            Application.Current.Dispatcher.BeginInvoke( (Action)(() =>
            {
                if( Notification != null )
                {
                    Notification.ShowNotification( PluginId.UniqueId, "Aucun clavier n'est actif",
                        "Aucun clavier n'est actif, veuillez activer un clavier.", 1000, NotificationTypes.Warning );
                }
            }), null );
        }

        #endregion Helper
    }
}
