using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using CK.Plugin;
using CK.WindowManager.Model;

namespace CK.WindowManager
{
    [Plugin( "{B91D6A8D-2294-4BAA-AD31-AC1F296D82C4}", PublicName = "CK.WindowManager.Executor", Categories = new string[] { "Accessibility" }, Version = "1.0.0" )]
    public class WindowManagerExecutor : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowManager WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowBinder WindowBinder { get; set; }

        void OnWindowManagerWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            // The Window that moves first
            IWindowElement triggerHolder = e.Window;

            // Gets all windows attach to the given window
            ISpatialBinding binding = WindowBinder.GetBinding( triggerHolder );

            // Horizontal move
            foreach( IWindowElement window in binding.AllDescendants() )
            {
                WindowManager.Move( window, window.Top + e.DeltaTop, window.Left + e.DeltaLeft );
            }
        }

        void OnWindowManagerWindowResized( object sender, WindowElementResizeEventArgs e )
        {
            // The Window that moves first
            IWindowElement triggerHolder = e.Window;

            // Gets all windows attach to the given window
            ISpatialBinding binding = WindowBinder.GetBinding( triggerHolder );

            if( e.DeltaHeight != 0 )
            {
                DoResizeVerticaly( e, binding.Left );
                DoResizeVerticaly( e, binding.Right );
            }
            if( e.DeltaWidth != 0 )
            {
                DoResizeHorizontaly( e, binding.Top );
                DoResizeHorizontaly( e, binding.Bottom );
            }
        }

        private void DoResizeHorizontaly( WindowElementResizeEventArgs e, ISpatialBinding spatial )
        {
            if( spatial != null )
            {
                var window = spatial.Window;
                double newWidth = window.Width + e.DeltaWidth;
                WindowManager.Resize( window, newWidth, window.Height );
            }
        }

        private void DoResizeVerticaly( WindowElementResizeEventArgs e, ISpatialBinding spatial )
        {
            if( spatial != null )
            {
                var window = spatial.Window;
                double newHeight = window.Height + e.DeltaHeight;
                WindowManager.Resize( window, window.Width, newHeight );
            }
        }

        void WindowManager_WindowRestored( object sender, WindowElementEventArgs e )
        {
            // The Window that moves first
            foreach( IWindowElement window in WindowBinder.GetBinding( e.Window ).AllDescendants() )
                window.Restore();
        }

        void WindowManager_WindowHidden( object sender, WindowElementEventArgs e )
        {
            foreach( IWindowElement window in WindowBinder.GetBinding( e.Window ).AllDescendants() )
                window.Hide();
        }

        void WindowBinder_BeforeBinding( object sender, WindowBindingEventArgs e )
        {

        }

        void WindowBinder_AfterBinding( object sender, WindowBindedEventArgs e )
        {
            if( e.BindingType == BindingEventType.Attach )
            {
                if( e.Binding.Position == BindingPosition.Top )
                {
                    double topSlave = e.Binding.Master.Top - e.Binding.Slave.Height;
                    double leftSlave = e.Binding.Master.Left;

                    WindowManager.Move( e.Binding.Slave, topSlave, leftSlave );
                    WindowManager.Resize( e.Binding.Slave, e.Binding.Master.Width, e.Binding.Slave.Height );
                }
            }
        }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            WindowManager.WindowResized += OnWindowManagerWindowResized;
            WindowManager.WindowMoved += OnWindowManagerWindowMoved;
            WindowManager.WindowHidden += WindowManager_WindowHidden;
            WindowManager.WindowRestored += WindowManager_WindowRestored;

            WindowBinder.BeforeBinding += WindowBinder_BeforeBinding;
            WindowBinder.AfterBinding += WindowBinder_AfterBinding;
        }

        public void Stop()
        {
            WindowManager.WindowResized -= OnWindowManagerWindowResized;
            WindowManager.WindowMoved -= OnWindowManagerWindowMoved;
            WindowManager.WindowHidden -= WindowManager_WindowHidden;
            WindowManager.WindowRestored -= WindowManager_WindowRestored;

            WindowBinder.BeforeBinding -= WindowBinder_BeforeBinding;
            WindowBinder.AfterBinding -= WindowBinder_AfterBinding;
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
