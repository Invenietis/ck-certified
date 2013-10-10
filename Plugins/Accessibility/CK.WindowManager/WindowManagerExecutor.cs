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
            if( binding != null )
            {
                // Horizontal move
                foreach( IWindowElement window in binding.AllDescendants() )
                {
                    WindowManager.Move( window, window.Top + e.DeltaTop, window.Left + e.DeltaLeft );
                }
            }
        }

        void OnWindowManagerWindowResized( object sender, WindowElementResizeEventArgs e )
        {
            // The Window that moves first
            IWindowElement triggerHolder = e.Window;
            // Gets all windows attach to the given window
            ISpatialBinding binding = WindowBinder.GetBinding( triggerHolder );
            if( binding != null )
            {
                if( e.DeltaHeight != 0 )
                {
                    DoResizeVerticaly( e, binding.Left );
                    DoResizeVerticaly( e, binding.Right );
                    if( binding.Bottom != null )
                    {
                        var windows = binding.Bottom.AllDescendants( excludes: BindingPosition.Top ).Union( new[] { binding.Bottom.Window } );
                        foreach( var window in windows )
                            WindowManager.Move( window, window.Top + e.DeltaHeight, window.Left );
                    }
                }
                if( e.DeltaWidth != 0 )
                {
                    DoResizeHorizontaly( e, binding.Top );
                    DoResizeHorizontaly( e, binding.Bottom );
                }
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
            ISpatialBinding binding = WindowBinder.GetBinding( e.Window );
            if( binding != null )
            {
                // The Window that moves first
                foreach( IWindowElement window in binding.AllDescendants() )
                    window.Restore();
            }
        }

        void WindowManager_WindowHidden( object sender, WindowElementEventArgs e )
        {
            ISpatialBinding binding = WindowBinder.GetBinding( e.Window );
            if( binding != null )
            {
                foreach( IWindowElement window in binding.AllDescendants() )
                    window.Hide();
            }
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
                   
                    var windowToMove = WindowBinder.GetBinding( e.Binding.Slave ).AllDescendants().Except( new[] { e.Binding.Master } ).Union( new[] { e.Binding.Slave } );
                    foreach( var w in windowToMove )
                    {
                        double topSlave = e.Binding.Master.Top - w.Height;
                        double leftSlave = e.Binding.Master.Left;

                        WindowManager.Move( w, topSlave, leftSlave );
                        WindowManager.Resize( w, e.Binding.Master.Width, w.Height );
                    }
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
