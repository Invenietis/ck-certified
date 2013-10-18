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
                foreach( IWindowElement window in binding.AllDescendants().Select( x => x.Window ) )
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
                    ResizeVerticaly( e, binding.Left, BindingPosition.Bottom | BindingPosition.Right | BindingPosition.Top );
                    ResizeVerticaly( e, binding.Right, BindingPosition.Top | BindingPosition.Bottom | BindingPosition.Left );
                    SpecialMoveBottom( e, binding );
                }
                if( e.DeltaWidth != 0 )
                {
                    ResizeHorizontaly( e, binding.Top, BindingPosition.Bottom | BindingPosition.Right | BindingPosition.Left );
                    ResizeHorizontaly( e, binding.Bottom, BindingPosition.Top | BindingPosition.Right | BindingPosition.Left );
                    SpecialMoveRight( e, binding );
                }
            }
        }

        void ResizeHorizontaly( WindowElementResizeEventArgs e, ISpatialBinding spatial, BindingPosition excludePos )
        {
            if( spatial != null )
            {
                var windows = spatial.AllDescendants( excludes: excludePos ).Union( new[] { spatial } );
                foreach( var window in windows )
                {
                    double newWidth = window.Window.Width + e.DeltaWidth;
                    WindowManager.Resize( window.Window, newWidth, window.Window.Height );
                    SpecialMoveRight( e, window );
                }
            }
        }

        /// <summary>
        /// Special case for windows attached to the right during a resizing
        /// </summary>
        private void SpecialMoveRight( WindowElementResizeEventArgs e, ISpatialBinding spatial )
        {
            if( spatial != null )
            {
                foreach( var windowDesc in spatial.SubTree( BindingPosition.Right ) )
                    WindowManager.Move( windowDesc, windowDesc.Top, windowDesc.Left + e.DeltaWidth ).Silent();
            }
        }

        void ResizeVerticaly( WindowElementResizeEventArgs e, ISpatialBinding spatial, BindingPosition excludePos )
        {
            if( spatial != null )
            {
                var windows = spatial.AllDescendants( excludes: excludePos ).Union( new[] { spatial } );
                foreach( var window in windows )
                {
                    double newHeight = window.Window.Height + e.DeltaHeight;
                    WindowManager.Resize( window.Window, window.Window.Width, newHeight );
                    SpecialMoveBottom( e, spatial );
                }
            }
        }

        /// <summary>
        /// Special case for windows attached to the bottom during a resizing
        /// </summary>
        private void SpecialMoveBottom( WindowElementResizeEventArgs e, ISpatialBinding spatial )
        {
            if( spatial != null )
            {
                //var window = binding.Bottom.Window;
                var windows = spatial.SubTree( BindingPosition.Bottom );
                foreach( var window in windows )
                    WindowManager.Move( window, window.Top + e.DeltaHeight, window.Left ).Silent();
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
                if( r != Rect.Empty )
                {
                    WindowManager.Move( e.Binding.Slave, r.Top, r.Left ).Broadcast();
                    WindowManager.Resize( e.Binding.Slave, r.Width, r.Height ).Broadcast();
                }
                else e.Canceled = true;
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
                foreach( IWindowElement window in binding.AllDescendants().Select( x => x.Window ) )
                    window.Restore();
            }
        }

        void OnWindowHidden( object sender, WindowElementEventArgs e )
        {
            ISpatialBinding binding = WindowBinder.GetBinding( e.Window );
            if( binding != null )
            {
                foreach( IWindowElement window in binding.AllDescendants().Select( x => x.Window ) )
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
