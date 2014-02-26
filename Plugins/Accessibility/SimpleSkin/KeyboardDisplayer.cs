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

namespace SimpleSkin
{
    [Plugin( KeyboardDisplayer.PluginIdString, PublicName = PluginPublicName, Version = KeyboardDisplayer.PluginIdVersion,
       Categories = new string[] { "Visual", "Accessibility" } )]
    public class KeyboardDisplayer : IPlugin
    {
        const string PluginPublicName = "MultiSkin";
        const string PluginIdString = "{D173E013-2491-4491-BF3E-CA2F8552B5EB}";
        const string PluginIdVersion = "0.9.0";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IHighlighterService> HighlighterService { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        [ConfigurationAccessor( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" )]
        public IPluginConfigAccessor Config { get; set; }
        
        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowBinder> WindowBinder { get; set; }

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

        CKNoFocusWindowManager _noFocusWindowManager;
        IDictionary<string,SkinInfo> _skins;
        IDictionary<string,RegisteredElementInfo> _registeredElementInfo;

        bool _forceClose;

        public bool Setup( IPluginSetupInfo info )
        {
            _skins = new Dictionary<string, SkinInfo>();
            _registeredElementInfo = new Dictionary<string, RegisteredElementInfo>();
            return true;
        }

        public void Start()
        {
            if( KeyboardContext.Status == InternalRunningStatus.Started )
            {
                _noFocusWindowManager = new CKNoFocusWindowManager();
                if( KeyboardContext.Service.Keyboards.Actives.Count > 0 )
                {
                    foreach( var activeKeyboard in KeyboardContext.Service.Keyboards.Actives )
                    {
                        var subscriber = new WindowManagerSubscriber( WindowManager, WindowBinder );
                        var vm = new VMContextActiveKeyboard( activeKeyboard.Name, Context, KeyboardContext.Service.Keyboards.Context, Config, _noFocusWindowManager.NoFocusWindowThreadDispatcher );

                        var skin = _noFocusWindowManager.CreateNoFocusWindow<SkinWindow>( () => new SkinWindow
                        {
                            DataContext = vm
                        } );

                        SkinInfo skinInfo = new SkinInfo( skin, vm, _noFocusWindowManager.NoFocusWindowThreadDispatcher, subscriber );
                        _skins.Add( activeKeyboard.Name, skinInfo );

                        SubscribeToWindowManager( skinInfo );

                        //Set placement and show window
                        InitializeWindowPlacementAndShow( skinInfo );
                    }
                }
                else
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
                
                RegisterEvents();
                InitializeHighligther();
                RegisterPrediction();
            }
        }

        //Set Placement and Show Window
        private void InitializeWindowPlacementAndShow( SkinInfo skinInfo )
        {
            var defaultPlacement = new WINDOWPLACEMENT();

            skinInfo.Dispatcher.Invoke( (System.Action)(() =>
            {
                InitializeWindowLayout( skinInfo );
                skinInfo.Skin.Show();
                defaultPlacement = CKWindowTools.GetPlacement( skinInfo.Skin.Hwnd );
            }), null );

            WINDOWPLACEMENT actualPlacement = Config.User.GetOrSet( PlacementString( skinInfo ), defaultPlacement );
            skinInfo.Dispatcher.Invoke( (Action)(() => CKWindowTools.SetPlacement( skinInfo.Skin.Hwnd, actualPlacement )), null );
        }

        //TODOF i think, we can factorize this
        public void Stop()
        {
            UnregisterPrediction();
            UnregisterEvents();
            if( HighlighterService.Status == InternalRunningStatus.Started )
            {
                UninitializeHighlighter();
                ForEachSkin( s =>
                    {
                        RegisteredElementInfo element;
                        if( _registeredElementInfo.TryGetValue( s.ViewModel.KeyboardVM.Keyboard.Name, out element ) ) UnregisterInRegisteredElement( element );
                    } );
            }

            _forceClose = true;

            foreach( var skin in _skins.Values )
            {

                skin.Subscriber.Unsubscribe();
                skin.ViewModel.Dispose();

                //Setting the config is done after closing the window because modifying a value in the Config
                ///Triggers a Caliburn Micro OnNotifyPropertyChanged, which calls an Invoke on the main UI Thread.
                //generating random locks.
                //Once the LayoutManager is ready, we won't need this anymore.
                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                skin.Dispatcher.Invoke( (Action)(() =>
                {
                    placement = CKWindowTools.GetPlacement( skin.Skin.Hwnd );
                    skin.Skin.Close();
                }) );

                Config.User.Set( PlacementString( skin ), placement );
            }

            Debug.Assert( _registeredElementInfo.Count == 0 );

            _skins.Clear();
        }

        public void Teardown()
        {
            if( _noFocusWindowManager != null )
            {
                //TODO : remove when the NoFocusWindowManager is exported to a service.
                //Then register the Shutdown call to the ApplicationExiting event.
                _noFocusWindowManager.Shutdown();
            }
        }

        //partial void OnSuccessfulStart()
        //{
        //    _subscriber = new WindowManagerSubscriber( WindowManager, WindowBinder );
        //    _skinDispatcher.BeginInvoke( new Action( () =>
        //    {
        //        _subscriber.WindowRegistered = ( e ) =>
        //        {
        //            e.Window.Hidden += OnWindowHidden;
        //        };
        //        _subscriber.WindowUnregistered = ( e ) =>
        //        {
        //            e.Window.Hidden -= OnWindowHidden;
        //        };
        //        _subscriber.Subscribe( "Skin", _skinWindow );
        //        _skinWindow.HidingAsked += OnWindowHidden;

        //    } ) );
        //}

        //partial void OnSuccessfulStop()
        //{
        //    _skinDispatcher.BeginInvoke( new Action( () =>
        //    {
        //        _skinWindow.HidingAsked -= OnWindowHidden;
        //    } ) );
        //    _subscriber.Unsubscribe();
        //}

        #endregion

        void SubscribeToWindowManager( SkinInfo skinInfo )
        {
            skinInfo.Dispatcher.BeginInvoke( new Action( () =>
            {
                skinInfo.Subscriber.Subscribe( skinInfo.ViewModel.KeyboardVM.Keyboard.Name, skinInfo.Skin );
            } ) );
        }

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
        /// <param name="defaultWidth"></param>
        /// <param name="defaultHeight"></param>
        /// <param name="skinInfo"></param>
        private void SetDefaultWindowPosition( SkinInfo skinInfo, int defaultWidth, int defaultHeight )
        {
            skinInfo.Skin.Top = 0;
            skinInfo.Skin.Left = 0;
            skinInfo.Skin.Width = defaultWidth;
            skinInfo.Skin.Height = defaultHeight;
        }

        private void ForEachSkin( Action<SkinInfo> action )
        {
            foreach( var skin in _skins.Values )
            {
                action( skin );
            }
        }

        #region Hightlight Methods

        void OnHighlighterServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                ForEachSkin( s =>
                {
                    if( !_registeredElementInfo.ContainsKey( s.ViewModel.KeyboardVM.Keyboard.Name ) ) RegisterHighlighter( s );
                } );
                foreach( var element in _registeredElementInfo.Values ) HighlighterService.Service.RegisterInRegisteredElementAt( element.TargetModuleName, element.ExtensibleElementName, element.Position, element.SkinInfo.ViewModel.KeyboardVM );
                UnregisterPrediction();
                RegisterPrediction();
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                ForEachSkin( s =>
                {
                    if( !_registeredElementInfo.ContainsKey( s.ViewModel.KeyboardVM.Keyboard.Name ) ) UnregisterHighlighter( s );
                } );
                UnregisterPrediction();
            }
        }

        private void InitializeHighligther()
        {
            HighlighterService.ServiceStatusChanged += OnHighlighterServiceStatusChanged;
            if( HighlighterService.Status == InternalRunningStatus.Started )
            {
                ForEachSkin( RegisterHighlighter );
            }

        }

        private void UninitializeHighlighter()
        {
            if( HighlighterService.Status == InternalRunningStatus.Started )
            {
                ForEachSkin( UnregisterHighlighter );
            }
            HighlighterService.ServiceStatusChanged -= OnHighlighterServiceStatusChanged;
        }

        private void UnregisterHighlighter( SkinInfo skinInfo )
        {
            if( HighlighterService.Status > InternalRunningStatus.Stopped )
            {
                HighlighterService.Service.UnregisterTree( skinInfo.ViewModel.KeyboardVM.Keyboard.Name, skinInfo.ViewModel.KeyboardVM );
            }
        }

        private void RegisterHighlighter( SkinInfo skinInfo )
        {
            if( HighlighterService.Status == InternalRunningStatus.Started )
            {
                HighlighterService.Service.RegisterTree( skinInfo.ViewModel.KeyboardVM.Keyboard.Name,
                    new ExtensibleHighlightableElementProxy( skinInfo.ViewModel.KeyboardVM.Keyboard.Name, skinInfo.ViewModel.KeyboardVM ) );
            }
        }

        string _includedKeyboardName = string.Empty;

        //if the target keyboard isn't active and isn't registered, the prediction is registered in root
        private void RegisterPrediction()
        {
            if( HighlighterService.Status == InternalRunningStatus.Started )
            {
                if( _skins.ContainsKey( "Prediction" ) )
                {
                    if( KeyboardContext.Status == InternalRunningStatus.Started )
                    {
                        //if the current isn't regitered
                        if( HighlighterService.Service.RegisterInRegisteredElementAt( KeyboardContext.Service.CurrentKeyboard.Name, KeyboardContext.Service.CurrentKeyboard.Name, ChildPosition.Pre, _skins["Prediction"].ViewModel.KeyboardVM ) )
                        {
                            _includedKeyboardName = KeyboardContext.Service.CurrentKeyboard.Name;
                            return;
                        }
                    }
                    HighlighterService.Service.RegisterTree( "Prediction", _skins["Prediction"].ViewModel.KeyboardVM );
                }
            }
        }

        private void UnregisterPrediction()
        {
            if( HighlighterService.Status == InternalRunningStatus.Started )
            {
                if( _skins.ContainsKey( "Prediction" ) )
                {
                    if( KeyboardContext.Status == InternalRunningStatus.Started )
                    {
                        if( !string.IsNullOrEmpty( _includedKeyboardName ) && HighlighterService.Service.UnregisterInRegisteredElement( _includedKeyboardName, _includedKeyboardName, ChildPosition.Pre, _skins["Prediction"].ViewModel.KeyboardVM ) )
                        {
                            _includedKeyboardName = string.Empty;
                            return;
                        }
                    }
                    HighlighterService.Service.UnregisterTree( "Prediction", _skins["Prediction"].ViewModel.KeyboardVM );
                    _includedKeyboardName = string.Empty;
                }
            }
        }

        void RegisterInRegisteredElement( RegisteredElementInfo element )
        {
            if( HighlighterService.Status == InternalRunningStatus.Started )
            {
                HighlighterService.Service.RegisterInRegisteredElementAt( element.TargetModuleName, element.ExtensibleElementName, element.Position, element.SkinInfo.ViewModel.KeyboardVM );
            }
            _registeredElementInfo.Add( element.SkinInfo.ViewModel.KeyboardVM.Keyboard.Name, element );
        }

        void UnregisterInRegisteredElement( RegisteredElementInfo element )
        {
            if( HighlighterService.Status == InternalRunningStatus.Started )
            {
                HighlighterService.Service.UnregisterInRegisteredElement( element.TargetModuleName, element.ExtensibleElementName, element.Position, element.SkinInfo.ViewModel.KeyboardVM );
            }
            _registeredElementInfo.Remove( element.SkinInfo.ViewModel.KeyboardVM.Keyboard.Name );
        }

        #endregion

        #region OnXXXX

        private void RegisterEvents()
        {
            KeyboardContext.Service.Keyboards.KeyboardActivated += Keyboards_KeyboardActivated;
            KeyboardContext.Service.Keyboards.KeyboardDeactivated += Keyboards_KeyboardDeactivated;
            KeyboardContext.Service.Keyboards.KeyboardDestroyed += Keyboards_KeyboardDestroyed;
            KeyboardContext.Service.Keyboards.KeyboardCreated += Keyboards_KeyboardCreated;
            KeyboardContext.Service.Keyboards.KeyboardRenamed += Keyboards_KeyboardRenamed;

            WindowBinder.ServiceStatusChanged += WindowBinder_ServiceStatusChanged;

            if( WindowBinder.Status == InternalRunningStatus.Started )
            {
                WindowBinder.Service.AfterBinding += Service_AfterBinding;
            }

            //Config.ConfigChanged += new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );
            ForEachSkin( RegisterSkinEvents );

            if( _skins.Count > 0 )
            {
                //KeyboardContext.Service.CurrentKeyboardChanging += new EventHandler<CurrentKeyboardChangingEventArgs>( OnCurrentKeyboardChanging );
                //KeyboardContext.Service.CurrentKeyboardChanged += new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
            }
        }
        
        private void RegisterSkinEvents( SkinInfo skinInfo )
        {
            //skinInfo.Skin.Closing += new CancelEventHandler( OnWindowClosing );
            //skinInfo.Skin.MouseLeave += new System.Windows.Input.MouseEventHandler( OnMouseLeaveWindow );
            //skinInfo.Skin.MouseEnter += new System.Windows.Input.MouseEventHandler( OnMouseEnterWindow );
            //skinInfo.Skin.SizeChanged += new SizeChangedEventHandler( OnWindowResized );
        }

        private void UnregisterEvents()
        {
            KeyboardContext.Service.Keyboards.KeyboardActivated -= Keyboards_KeyboardActivated;
            KeyboardContext.Service.Keyboards.KeyboardDeactivated -= Keyboards_KeyboardDeactivated;
            KeyboardContext.Service.Keyboards.KeyboardDestroyed -= Keyboards_KeyboardDestroyed;
            KeyboardContext.Service.Keyboards.KeyboardCreated -= Keyboards_KeyboardCreated;
            KeyboardContext.Service.Keyboards.KeyboardRenamed -= Keyboards_KeyboardRenamed;

            if( WindowBinder.Status == InternalRunningStatus.Started )
            {
                WindowBinder.Service.AfterBinding -= Service_AfterBinding;
            }

            //Config.ConfigChanged -= new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );
            //KeyboardContext.Service.CurrentKeyboardChanging -= new EventHandler<CurrentKeyboardChangingEventArgs>( OnCurrentKeyboardChanging );
            //KeyboardContext.Service.CurrentKeyboardChanged -= new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
            ForEachSkin( UnregisterSkinEvents ); 
        }

        private void UnregisterSkinEvents( SkinInfo skinInfo )
        {
            //skinInfo.Skin.Closing -= new CancelEventHandler( OnWindowClosing );
            //skinInfo.Skin.MouseLeave -= new System.Windows.Input.MouseEventHandler( OnMouseLeaveWindow );
            //skinInfo.Skin.MouseEnter -= new System.Windows.Input.MouseEventHandler( OnMouseEnterWindow );
            //skinInfo.Skin.SizeChanged -= new SizeChangedEventHandler( OnWindowResized );
        }

        void Service_AfterBinding( object sender, WindowBindedEventArgs e )
        {
           
        }

        void WindowBinder_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                WindowBinder.Service.AfterBinding += Service_AfterBinding;
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                WindowBinder.Service.AfterBinding -= Service_AfterBinding;
            }
        }

        void Keyboards_KeyboardDeactivated( object sender, KeyboardEventArgs e )
        {
            Debug.Assert( _skins.ContainsKey( e.Keyboard.Name ) );

            //factorize
            SkinInfo skin = _skins[e.Keyboard.Name];
            UninitializeActiveWindows( skin );
            UnregisterPrediction();
            RegisterPrediction();
        }

        void Keyboards_KeyboardActivated( object sender, KeyboardEventArgs e )
        {
            InitializeActiveWindows( e.Keyboard );
            UnregisterPrediction();
            RegisterPrediction();
        }

        void Keyboards_KeyboardRenamed( object sender, KeyboardRenamedEventArgs e )
        {
            SkinInfo skin;
            if( _skins.TryGetValue( e.PreviousName, out skin ) )
            {
                _skins.Remove( e.PreviousName );
                _skins.Add( e.Keyboard.Name, skin );

                //Update name with Unregister and Register
                if( HighlighterService.Status == InternalRunningStatus.Started )
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

        void Keyboards_KeyboardDestroyed( object sender, KeyboardEventArgs e )
        {
            if( e.Keyboard.IsActive )
            {
                Debug.Assert( _skins.ContainsKey( e.Keyboard.Name ) );
                SkinInfo skin = _skins[e.Keyboard.Name];
                UninitializeActiveWindows( skin );
            }
        }

        void Keyboards_KeyboardCreated( object sender, KeyboardEventArgs e )
        {
            if( e.Keyboard.IsActive && !_skins.ContainsKey( e.Keyboard.Name ) )
            {
                InitializeActiveWindows( e.Keyboard );
            }
        }

        void InitializeActiveWindows( IKeyboard keyboard )
        {
            var subscriber = new WindowManagerSubscriber( WindowManager, WindowBinder );
            var vm = new VMContextActiveKeyboard( keyboard.Name, Context, KeyboardContext.Service.Keyboards.Context, Config, _noFocusWindowManager.NoFocusWindowThreadDispatcher );

            var skin = _noFocusWindowManager.CreateNoFocusWindow<SkinWindow>( () => new SkinWindow
            {
                DataContext = vm
            } );

            SkinInfo skinInfo = new SkinInfo( skin, vm, _noFocusWindowManager.NoFocusWindowThreadDispatcher, subscriber );
            _skins.Add( keyboard.Name, skinInfo );

            SubscribeToWindowManager( skinInfo );

            //Set placement and show window
            InitializeWindowPlacementAndShow( skinInfo );
            RegisterSkinEvents( skinInfo );

            if( HighlighterService.Status == InternalRunningStatus.Started ) RegisterHighlighter( skinInfo );
        }

        void UninitializeActiveWindows( SkinInfo skin )
        {
            skin.Subscriber.Unsubscribe();
            skin.ViewModel.Dispose();

            //Setting the config is done after closing the window because modifying a value in the Config
            ///Triggers a Caliburn Micro OnNotifyPropertyChanged, which calls an Invoke on the main UI Thread.
            //generating random locks.
            //Once the LayoutManager is ready, we won't need this anymore.
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            skin.Dispatcher.Invoke( (Action)(() =>
            {
                placement = CKWindowTools.GetPlacement( skin.Skin.Hwnd );
                skin.Skin.Close();
            }) );

            Config.User.Set( PlacementString( skin ), placement );

            if( HighlighterService.Status == InternalRunningStatus.Started )
            {
                UnregisterHighlighter( skin );
                RegisteredElementInfo element;
                if( _registeredElementInfo.TryGetValue( skin.ViewModel.KeyboardVM.Keyboard.Name, out element ) ) UnregisterInRegisteredElement( element );
                else
                {
                    element = _registeredElementInfo.Values.Where( r => r.TargetModuleName == skin.ViewModel.KeyboardVM.Keyboard.Name ).FirstOrDefault();
                    if( element != null ) UnregisterInRegisteredElement( element );
                }
            }

            UnregisterSkinEvents( skin );

            _skins.Remove( skin.ViewModel.KeyboardVM.Keyboard.Name );
        }

        //void OnCurrentKeyboardChanging( object sender, CurrentKeyboardChangingEventArgs e )
        //{
        //    Debug.Assert( _skins.ContainsKey( e.Current.Name ) );
        //    SkinInfo skin = _skins[e.Current.Name];

        //    e.Current.IsActive = false;
        //    e.Next.IsActive = true;

        //    if( e.Next == null )
        //    {
        //        _skinDispatcher.BeginInvoke( (Action)(() =>
        //        {
        //            if( _miniView != null && _miniView.IsVisible )
        //            {
        //                Debug.Assert( !_viewHidden, "The miniview is visible yet _viewHidden is false" );
        //                _miniView.Hide();
        //            }

        //            if( _skinWindow != null && _skinWindow.IsVisible )
        //            {
        //                _skinWindow.Hide();
        //            }
        //        }), null );
        //    }
        //    else
        //    {
        //        //if the previous keyboard was null
        //        if( e.Current == null )
        //        {
        //            _skinDispatcher.BeginInvoke( (Action)(() =>
        //            {
        //                //if the view was not hidden before setting the keyboard to null
        //                if( _skinWindow != null && !_viewHidden )
        //                {
        //                    Debug.Assert( !_skinWindow.IsVisible, "Changing the current keyboard from null to an existing keyboard, but the skin view was already visible" );
        //                    _skinWindow.Show();
        //                }
        //                else if( _miniView != null )
        //                {
        //                    Debug.Assert( !_miniView.IsVisible, "Changing the current keyboard from null to an existing keyboard, but the miniview was already visible" );
        //                    _miniView.Show();
        //                }
        //            }), null );
        //        }
        //    }
        //}

        //void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        //{
        //    InitializeHighligther();

        //    if( Highlighter.Status == InternalRunningStatus.Started )
        //    {
        //        Highlighter.Service.RegisterTree( "Keyboard", _ctxVm.KeyboardVM );
        //    }

        //    if( e.Current != null && _skinWindow != null )
        //    {
        //        if( Config.User[PlacementString] != null )
        //        {
        //            WINDOWPLACEMENT placement = (WINDOWPLACEMENT)Config.User[PlacementString];
        //            if( _viewHidden ) placement.showCmd = 0;
        //            else placement.showCmd = 8; //Show without taking focus

        //            _skinDispatcher.BeginInvoke( (Action)(() => CKWindowTools.SetPlacement( _skinWindow.Hwnd, placement )), null );
        //        }
        //        else
        //        {
        //            int w;
        //            int h;
        //            var viewPortSize = Config[_ctxVm.KeyboardContext.CurrentKeyboard.CurrentLayout]["ViewPortSize"];
        //            if( viewPortSize != null )
        //            {
        //                Size size = (Size)viewPortSize;
        //                w = (int)size.Width;
        //                h = (int)size.Height;
        //            }
        //            else
        //            {
        //                w = _ctxVm.KeyboardVM.W;
        //                h = _ctxVm.KeyboardVM.H;
        //            }

        //            _skinDispatcher.BeginInvoke( (Action)(() => SetDefaultWindowPosition( w, h )), null );
        //        }
        //    }
        //}

        //void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        //{
        //    if( e.MultiPluginId.Any( ( c ) => c.UniqueId.Equals( this.PluginGuid ) ) && !String.IsNullOrEmpty( e.Key ) )
        //    {
        //        if( e.Key == "autohide" || e.Key == "autohide-timeout" ) UpdateAutoHideConfig();
        //    }
        //}

        //void OnMouseLeaveWindow( object sender, System.Windows.Input.MouseEventArgs e )
        //{
        //    int timeout = _timeout == 0 ? 6000 : _timeout;
        //    if( _autohide )
        //    {
        //        if( _timer == null )
        //            _timer = new DispatcherTimer( TimeSpan.FromMilliseconds( timeout ), DispatcherPriority.Normal, ( o, args ) => HideSkin(), _skinWindow.Dispatcher );
        //        else
        //            _timer.Interval = TimeSpan.FromMilliseconds( timeout );

        //        _timer.Start();
        //    }
        //}

        //void OnMouseEnterWindow( object sender, System.Windows.Input.MouseEventArgs e )
        //{
        //    if( _timer != null ) _timer.Stop();
        //}

        //void OnWindowResized( object sender, SizeChangedEventArgs e )
        //{
        //    if( _timer != null ) _timer.Stop();
        //}

        //void OnWindowClosing( object sender, System.ComponentModel.CancelEventArgs e )
        //{
        //    if( !_forceClose && !e.Cancel )
        //    {
        //        e.Cancel = true;
        //        if( Notification != null )
        //        {
        //            Notification.ShowNotification( new Guid( PluginIdString ), "Unable to close skin window",
        //                "The skin window cannot be closed like this, if you want to close the window deactive the keyboard.", 5000, NotificationTypes.Warning );
        //        }
        //    }
        //}

        //void OnWindowHidden( object sender, EventArgs e )
        //{
        //    HideSkin();
        //}

        ///// <summary>
        ///// Hides the skin and shows the keyboard's MiniView
        ///// </summary>
        //public void HideSkin()
        //{
        //    if( !_viewHidden )
        //    {
        //        _viewHidden = true;

        //        _skinDispatcher.BeginInvoke( (Action)(() =>
        //        {
        //            ShowMiniView();
        //            if( Highlighter.Status == InternalRunningStatus.Started )
        //            {
        //                Highlighter.Service.RegisterTree( "MinimizeKeyboard", _miniViewVm );
        //                Highlighter.Service.UnregisterTree( "Keyboard", _ctxVm.KeyboardVM );
        //            }
        //            if( _timer != null ) _timer.Stop();
        //        }), null );
        //    }
        //}

        #endregion
    }
}
