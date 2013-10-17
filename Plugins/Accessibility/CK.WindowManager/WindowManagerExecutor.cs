using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using CK.Plugin;
using CK.WindowManager.Model;
using CK.Core;
using System.Windows;
using CK.Windows;

namespace CK.WindowManager
{
    [Plugin( "{B91D6A8D-2294-4BAA-AD31-AC1F296D82C4}", PublicName = "CK.WindowManager.Executor", Categories = new string[] { "Accessibility" }, Version = "1.0.0" )]
    public class WindowManagerExecutor : IPlugin
    {
        PreviewBindingInfo _placeholder;
        DefaultActivityLogger _logger;

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowManager WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowBinder WindowBinder { get; set; }

        public WindowManagerExecutor()
        {
            _placeholder = new PreviewBindingInfo();
            _logger = new DefaultActivityLogger();
            //_logger.Tap.Register( new ActivityLoggerConsoleSink() );
        }

        void OnWindowManagerWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            IWindowElement triggerHolder = e.Window;
            // Gets all windows attach to the given window
            ISpatialBinding binding = WindowBinder.GetBinding( triggerHolder );
            if( binding != null )
            {
                foreach( IWindowElement window in binding.AllDescendants() )
                {
                    WindowManager.Move( window, window.Top + e.DeltaTop, window.Left + e.DeltaLeft );
                }
            }
        }

        void OnWindowManagerWindowResized( object sender, WindowElementResizeEventArgs e )
        {
            IWindowElement triggerHolder = e.Window;
            // Gets all windows attach to the given window
            ISpatialBinding binding = WindowBinder.GetBinding( triggerHolder );
            if( binding != null )
            {
                if( e.DeltaHeight != 0 )
                {
                    DoResizeVerticaly( e, binding.Left );
                    DoResizeVerticaly( e, binding.Right );
                    // Special case for windows attached to the bottom during a resizing
                    if( binding.Bottom != null )
                    {
                        //var window = binding.Bottom.Window;
                        var windows = binding.Bottom.AllDescendants( excludes: BindingPosition.Top ).Union( new[] { binding.Bottom.Window } );
                        foreach( var window in windows )
                            WindowManager.Move( window, window.Top + e.DeltaHeight, window.Left ).Silent();
                    }
                }
                if( e.DeltaWidth != 0 )
                {
                    DoResizeHorizontaly( e, binding.Top, BindingPosition.Bottom );
                    DoResizeHorizontaly( e, binding.Bottom, BindingPosition.Top );
                    // Special case for windows attached to the right during a resizing

                    if( binding.Right != null )
                    {
                        var windows = binding.Right.AllDescendants( excludes: BindingPosition.Left ).Union( new[] { binding.Right.Window } );
                        foreach( var window in windows )
                            WindowManager.Move( window, window.Top, window.Left + e.DeltaWidth ).Silent();
                    }
                }
            }
        }

        void DoResizeHorizontaly( WindowElementResizeEventArgs e, ISpatialBinding spatial, BindingPosition excludePos )
        {
            if( spatial != null )
            {
                {
                    // [Master]
                    // [Spatial] 
                    // [Spatial.Descendants]
                    var windows = spatial.AllDescendants( excludes: excludePos ).Union( new[] { spatial.Window } );
                    foreach( var window in windows )
                    {
                        double newWidth = window.Width + e.DeltaWidth;
                        WindowManager.Resize( window, newWidth, window.Height );
                    }
                }
                // Special case to propagate the movement to all attached windows at the right of the target
                //  [Master]
                //  [Spatial] [Spatial.Right]
                //  
                //  [Spatial] [Spatial.Right]
                //  [Master]
                if( spatial.Right != null )
                {
                    var windows = spatial.Right.AllDescendants( excludes: BindingPosition.Left ).Union( new[] { spatial.Right.Window } );
                    foreach( var window in windows )
                        WindowManager.Move( window, window.Top, window.Left + e.DeltaWidth ).Silent();
                }
            }
        }

        void DoResizeVerticaly( WindowElementResizeEventArgs e, ISpatialBinding spatial )
        {
            if( spatial != null )
            {
                {
                    var window = spatial.Window;
                    double newHeight = window.Height + e.DeltaHeight;
                    WindowManager.Resize( window, window.Width, newHeight );
                }
                // Special case to propagate the movement to all attached windows at the bottom of the target
                //  [Master] [Spatial]
                //           [Spatial.Bottom]
                //  
                //         [Spatial] [Master]
                //  [Spatial.Bottom]
                if( spatial.Bottom != null )
                {
                    var windows = spatial.Bottom.AllDescendants( excludes: BindingPosition.Top ).Union( new[] { spatial.Bottom.Window } );
                    foreach( var window in windows )
                        WindowManager.Move( window, window.Top + e.DeltaHeight, window.Left ).Silent();
                }
            }
        }

        void OnPreviewBinding( object sender, WindowBindedEventArgs e )
        {
            if( e.BindingType == BindingEventType.Attach )
            {
                if( !_placeholder.IsPreviewOf( e.Binding ) ) _placeholder.Display( e.Binding );
            }
            else _placeholder.Shutdown();
        }

        void OnBeforeBinding( object sender, WindowBindingEventArgs e )
        {
            if( e.BindingType == BindingEventType.Attach )
            {
                Rect r = e.Binding.GetWindowArea();

                WindowManager.Move( e.Binding.Slave, r.Top, r.Left ).Broadcast();
                WindowManager.Resize( e.Binding.Slave, r.Width, r.Height ).Broadcast();
            }
        }

        void OnAfterBinding( object sender, WindowBindedEventArgs e )
        {
            _placeholder.Shutdown();
        }

        void OnWindowRestored( object sender, WindowElementEventArgs e )
        {
            ISpatialBinding binding = WindowBinder.GetBinding( e.Window );
            if( binding != null )
            {
                // The Window that moves first
                foreach( IWindowElement window in binding.AllDescendants() )
                    window.Restore();
            }
        }

        void OnWindowHidden( object sender, WindowElementEventArgs e )
        {
            ISpatialBinding binding = WindowBinder.GetBinding( e.Window );
            if( binding != null )
            {
                foreach( IWindowElement window in binding.AllDescendants() )
                    window.Hide();
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
            WindowManager.WindowHidden += OnWindowHidden;
            WindowManager.WindowRestored += OnWindowRestored;

            WindowBinder.PreviewBinding += OnPreviewBinding;
            WindowBinder.BeforeBinding += OnBeforeBinding;
            WindowBinder.AfterBinding += OnAfterBinding;
        }

        public void Stop()
        {
            WindowManager.WindowResized -= OnWindowManagerWindowResized;
            WindowManager.WindowMoved -= OnWindowManagerWindowMoved;
            WindowManager.WindowHidden -= OnWindowHidden;
            WindowManager.WindowRestored -= OnWindowRestored;

            WindowBinder.PreviewBinding -= OnPreviewBinding;
            WindowBinder.BeforeBinding -= OnBeforeBinding;
            WindowBinder.AfterBinding -= OnAfterBinding;
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
