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
        DefaultActivityLogger _logger;

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowManager WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowBinder WindowBinder { get; set; }

        public WindowManagerExecutor()
        {
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


        Rect GetWindowPosition( IBinding binding )
        {
            if( binding.Position == BindingPosition.Top )
            {
                double top = binding.Master.Top - binding.Slave.Height;
                double left = binding.Master.Left;
                double width = binding.Master.Width;
                double height = binding.Slave.Height;
                return new Rect( left, top, width, height );
            }
            if( binding.Position == BindingPosition.Bottom )
            {
                double top = binding.Master.Top + binding.Master.Height;
                double left = binding.Master.Left;
                double width = binding.Master.Width;
                double height = binding.Slave.Height;
                return new Rect( left, top, width, height );
            }
            if( binding.Position == BindingPosition.Left )
            {
                double top = binding.Master.Top;
                double left = binding.Master.Left - binding.Slave.Width;
                double width = binding.Slave.Width;
                double height = binding.Master.Height;
                return new Rect( left, top, width, height );
            }
            if( binding.Position == BindingPosition.Right )
            {
                double top = binding.Master.Top;
                double left = binding.Master.Left + binding.Master.Width;
                double width = binding.Slave.Width;
                double height = binding.Master.Height;
                return new Rect( left, top, width, height );
            }
            return new Rect();
        }

        //TODO : by windows
        CKWindow _previewingBinding = null;
        BindingPosition _previewingBindingPosition = BindingPosition.None;

        void WindowBinder_PreviewBinding( object sender, WindowBindedEventArgs e )
        {
            if( e.BindingType == BindingEventType.Attach )
            {
                if( _previewingBindingPosition != e.Binding.Position )
                {
                    _previewingBindingPosition = e.Binding.Position;
                    ShowPlaceholder( e.Binding );
                }
            }
            else
            {
                if( _previewingBinding != null )
                    _previewingBinding.Dispatcher.BeginInvoke( new Action( () =>
                    {
                        _previewingBinding.Hide();
                        _previewingBinding = null;
                        _previewingBindingPosition = BindingPosition.None;
                    } ) );
            }
        }

        void ShowPlaceholder( IBinding binding )
        {
            CKWindow w = _previewingBinding ?? (_previewingBinding = new CKWindow());

            Rect r = GetWindowPosition( binding );
            w.Show();
            w.Left = r.Left;
            w.Top = r.Top;
            w.Width = r.Width;
            w.Height = r.Height;
            w.Opacity = .5;
            w.Background = new System.Windows.Media.SolidColorBrush( System.Windows.Media.Color.FromRgb( 152, 120, 152 ) );
            w.ResizeMode = ResizeMode.NoResize;
            w.WindowStyle = WindowStyle.None;
        }

        void WindowBinder_BeforeBinding( object sender, WindowBindingEventArgs e )
        {
            if( e.BindingType == BindingEventType.Attach )
            {
                Rect r = GetWindowPosition( e.Binding );

                WindowManager.Move( e.Binding.Slave, r.Top, r.Left ).Broadcast();
                WindowManager.Resize( e.Binding.Slave, r.Width, r.Height ).Broadcast();
            }
        }

        void WindowBinder_AfterBinding( object sender, WindowBindedEventArgs e )
        {
            if( _previewingBinding != null )
                _previewingBinding.Dispatcher.BeginInvoke( new Action( () =>
                {
                    _previewingBinding.Hide();
                    _previewingBinding = null;
                } ) );
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

            WindowBinder.PreviewBinding += WindowBinder_PreviewBinding;
            WindowBinder.BeforeBinding += WindowBinder_BeforeBinding;
            WindowBinder.AfterBinding += WindowBinder_AfterBinding;
        }

        public void Stop()
        {
            WindowManager.WindowResized -= OnWindowManagerWindowResized;
            WindowManager.WindowMoved -= OnWindowManagerWindowMoved;
            WindowManager.WindowHidden -= WindowManager_WindowHidden;
            WindowManager.WindowRestored -= WindowManager_WindowRestored;

            WindowBinder.PreviewBinding -= WindowBinder_PreviewBinding;
            WindowBinder.BeforeBinding -= WindowBinder_BeforeBinding;
            WindowBinder.AfterBinding -= WindowBinder_AfterBinding;
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
