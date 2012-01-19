using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;
using Host.Services;
using SimpleSkin.Helper;
using SimpleSkin.ViewModels;
using System.Diagnostics;

namespace SimpleSkin
{
    [Plugin(SimpleSkin.PluginIdString,
        PublicName=PluginPublicName,
        Version=SimpleSkin.PluginIdVersion,
        Categories=new string[]{"Visual","Accessibility"})]
    public class SimpleSkin : IPlugin, ISkinService
    {
        const string PluginIdString = "{36C4764A-111C-45e4-83D6-E38FC1DF5979}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "SimpleSkin";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId ( PluginIdString, PluginIdVersion, PluginPublicName );

        bool _viewHidden;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardContext KeyboardContext { get; set; }

        [RequiredService]
        public IContext Context { get; set; }
        
        VMContextSimple _ctxVm;
        SkinWindow _skinWindow;
        MiniView _miniView;
        bool _isStarted;
        bool _forceClose;

        // auto hide
        bool _autohide;
        int _timeout;
        DispatcherTimer _timer;

        [RequiredService]
        public INotificationService Notification { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

       // void OnTimerTick( object sender, EventArgs e )
       // {
       //     this._ctxVm.Keyboard.Layout.W = this._ctxVm.Keyboard.Layout.W - 5;

        //}

        public void Start()
        {
            if( KeyboardContext.Keyboards.Count > 0 )
            {
                Context.ServiceContainer.Add( Config );

                _isStarted = true;
                _ctxVm = new VMContextSimple( Context, KeyboardContext );
                _skinWindow = new SkinWindow( _ctxVm );
                _skinWindow.Show();

                //DispatcherTimer t = new DispatcherTimer( new TimeSpan( 0, 0, 0, 1, 0 ), DispatcherPriority.Normal, OnTimerTick, _skinWindow.Dispatcher );
                //t.Start();

                _skinWindow.Closing += new CancelEventHandler( OnWindowClosing );
                
                //autohide
                UpdateAutoHideConfig();

                Config.ConfigChanged += new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );

                _skinWindow.MouseLeave += new System.Windows.Input.MouseEventHandler( OnMouseLeaveWindow );
                _skinWindow.MouseEnter += new System.Windows.Input.MouseEventHandler( OnMouseEnterWindow );
                _skinWindow.SizeChanged += new SizeChangedEventHandler( OnWindowResized );
            }
            else
            {
                _isStarted = false;
                Notification.ShowNotification( PluginId.UniqueId, "Aucun clavier n'est disponible", 
                    "Aucun clavier n'est disponible dans le contexte actuel, veuillez choisir un contexte contenant au moins un clavier.", 1000, NotificationTypes.Error );
            }
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {

            if ( e.MultiPluginId.Contains( PluginId ) )
            {
                if (e.Key == "autohide" || e.Key == "autohide-timeout") UpdateAutoHideConfig();
            }

        }

        void UpdateAutoHideConfig()
        {
            var autohideCfg = Config.User["autohide"];
            var timeoutCfg = Config.User["autohide-timeout"];

            if( autohideCfg != null && Boolean.TryParse( autohideCfg.ToString(), out _autohide ) )
            {
                if( !_autohide && _timer != null ) _timer.Stop();
                if( timeoutCfg != null ) Int32.TryParse( timeoutCfg.ToString(), out _timeout );
            }
        }

        void OnMouseLeaveWindow( object sender, System.Windows.Input.MouseEventArgs e )
        {
            int timeout = _timeout == 0 ? 6000 : _timeout;
            if( _autohide )
            {
                if( _timer == null )
                    _timer = new DispatcherTimer( TimeSpan.FromMilliseconds( timeout ), DispatcherPriority.Normal, ( o, args ) => Hide(), _skinWindow.Dispatcher );
                else
                    _timer.Interval = TimeSpan.FromMilliseconds( timeout );

                _timer.Start();
            }
        }

        void OnMouseEnterWindow( object sender, System.Windows.Input.MouseEventArgs e )
        {
            if( _timer != null ) _timer.Stop();
        }

        void OnWindowResized( object sender, SizeChangedEventArgs e )
        {
            if( _timer != null ) _timer.Stop();
        }

        void OnWindowClosing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            if( !_forceClose && !e.Cancel )
            {
                e.Cancel = true;
                if( Notification != null )
                {
                    Notification.ShowNotification( new Guid( PluginIdString ), "Unable to close skin window",
                        "The skin window cannot be closed like this, if you want to close the window stop the plugin.", 5000, NotificationTypes.Warning );
                }
            }
        }

        public void Stop()
        {
            if( _isStarted )
            {
                Context.ServiceContainer.Remove( typeof( IPluginConfigAccessor ) );

                _skinWindow.Closing -= new CancelEventHandler( OnWindowClosing );

                _skinWindow.MouseLeave -= new System.Windows.Input.MouseEventHandler( OnMouseLeaveWindow );
                _skinWindow.MouseEnter -= new System.Windows.Input.MouseEventHandler( OnMouseEnterWindow );
                _skinWindow.SizeChanged -= new SizeChangedEventHandler( OnWindowResized );

                _forceClose = true;
                _skinWindow.Close();

                if( _miniView != null ) 
                { 
                    _miniView.Close(); 
                    _miniView = null;
                    _viewHidden = false;
                }

                _ctxVm.Dispose();
                _isStarted = false;
            }
        }

        public void Teardown()
        {
        }

        public void Hide()
        {
            if( !_viewHidden )
            {
                _viewHidden = true;
                _skinWindow.Hide();
                ShowMiniView();

                if( _timer != null ) _timer.Stop();
            }
        }

        public void RestoreSkin()
        {
            if( _viewHidden )
            {
                _viewHidden = false;
                _miniView.Hide();
                _skinWindow.Show();
            }
        }

        public double MiniViewPositionX
        {
            get
            {
                var postion = Config.Context["MiniViewPositionX"];
                if( postion == null )
                {
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
                    System.Drawing.Point p = ScreenHelper.GetCenterOfParentScreen( rect );

                    return p.X;
                }
                else
                    return (double)postion;
            }
            set { Config.Context["MiniViewPositionX"] = value; }
        }

        public double MiniViewPositionY
        {
            get
            {
                var postion = Config.Context["MiniViewPositionY"];
                if( postion == null )
                    return 0;
                else
                    return (double)postion;
            }
            set { Config.Context["MiniViewPositionY"] = value; }
        }

        void ShowMiniView()
        {
            if( _miniView == null )
            {
                _miniView = new MiniView( RestoreSkin ) { DataContext = this };
                _miniView.Closing += new CancelEventHandler( OnWindowClosing );
            }

            _miniView.Show();
        }
    }
}
