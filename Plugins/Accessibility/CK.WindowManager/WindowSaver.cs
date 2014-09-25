using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using BasicCommandHandlers;
using CK.Context;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WindowManager.Model;

namespace CK.WindowManager
{
    [Plugin( WindowSaver.PluginIdString, PublicName = PluginPublicName, Version = WindowSaver.PluginIdVersion,
       Categories = new string[] { "Accessibility" } )]
    public class WindowSaver : IPlugin
    {
        const string PluginPublicName = "WindowSaver";
        const string PluginIdString = "{55A95F2F-2D67-4AE1-B5CF-4880337F739F}";
        const string PluginIdVersion = "1.0.0";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWindowManager WindowManager { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        bool _isSave;
        RegionHelper _regionHelper;

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _regionHelper = new RegionHelper();
            foreach( var s in Screen.AllScreens ) _regionHelper.Add( s.Bounds );

            InitAllPlacements();

            RegisterEvents();
        }

        public void Stop()
        {
            UnregisterEvents();
            SavePlacements();
        }

        public void Teardown()
        {
        }

        #endregion

        private void RegisterEvents()
        {
            foreach( var we in WindowManager.WindowElements )
            {
                we.Window.Closing += OnClosing;
            }
            WindowManager.Registered += OnWindowManagerRegistered;
            WindowManager.Unregistered += OnWindowManagerUnregistered;

            Context.ApplicationExiting += OnApplicationExiting;
        }

        private void UnregisterEvents()
        {
            foreach( var we in WindowManager.WindowElements )
            {
                we.Window.Closing -= OnClosing;
            }
            WindowManager.Registered -= OnWindowManagerRegistered;
            WindowManager.Unregistered -= OnWindowManagerUnregistered;

            Context.ApplicationExiting -= OnApplicationExiting;
        }

        void OnApplicationExiting( object sender, ApplicationExitingEventArgs e )
        {
            SavePlacements();
        }

        private void SavePlacements()
        {
            if( !_isSave )
            {
                foreach( var we in WindowManager.WindowElements )
                {
                    we.Window.Closing -= OnClosing;
                    Config.User.Set( we.Name, new WindowPlacement( we.Name, we.Top, we.Left, we.Width, we.Height ) );
                }
                _isSave = true;
            }
        }

        void OnWindowManagerRegistered( object sender, WindowElementEventArgs e )
        {
            if( e.Window.Window.IsVisible )
            {
                InitWindowPlacement( e.Window );
            }
            else
            {
                e.Window.Window.IsVisibleChanged += OnIsVisibleChanged;
            }

            e.Window.Window.Closing += OnClosing;
        }

        void OnIsVisibleChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            if( (bool)e.NewValue )
            {
                Window window = sender as Window;
                IWindowElement we = WindowManager.WindowElements.First( element => element.Window == window );
                InitWindowPlacement( we );
                window.IsEnabledChanged -= OnIsVisibleChanged;
            }
        }

        void OnWindowManagerUnregistered( object sender, WindowElementEventArgs e )
        {
            e.Window.Window.Closing -= OnClosing;
            Config.User.Set( e.Window.Name, new WindowPlacement( e.Window.Name, e.Window.Top, e.Window.Left, e.Window.Width, e.Window.Height ) );
        }

        void OnClosing( object sender, CancelEventArgs e )
        {
            Window window = sender as Window;
            IWindowElement we = WindowManager.WindowElements.First( element => element.Window == window );
            Config.User.Set( we.Name, new WindowPlacement( we.Name, we.Top, we.Left, we.Width, we.Height ) );
        }

        private void InitAllPlacements()
        {
            foreach( var we in WindowManager.WindowElements )
            {
                InitWindowPlacement( we );
            }
        }

        private void InitWindowPlacement( IWindowElement we )
        {
            WindowPlacement wp = Config.User.GetOrSet<WindowPlacement>( we.Name, (WindowPlacement)null );
            if( wp != null )
            {

                if( OnePointIsContainsInScreen( wp ) )
                {
                    we.Move( wp.Top, wp.Left );
                }
                else
                {
                    we.Move( 0, 0 );
                }
                we.Resize( wp.Width, wp.Height );
            }
        }

        private bool OnePointIsContainsInScreen( WindowPlacement wp )
        {
            return _regionHelper.Contains( new System.Drawing.Point( wp.Left, wp.Top ) )
                || _regionHelper.Contains( new System.Drawing.Point( wp.Left + wp.Width, wp.Top ) )
                || _regionHelper.Contains( new System.Drawing.Point( wp.Left, wp.Top + wp.Height ) )
                || _regionHelper.Contains( new System.Drawing.Point( wp.Left + wp.Width, wp.Top + wp.Height ) );
        }
    }
}
