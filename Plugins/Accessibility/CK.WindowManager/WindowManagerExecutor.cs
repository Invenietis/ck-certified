using System.Linq;
using CK.Plugin;
using CK.WindowManager.Model;
using CK.Core;
using System.Windows;
using System;
using CommonServices;
using System.Windows.Threading;
using CK.Windows;
using System.Diagnostics;

namespace CK.WindowManager
{
    [Plugin( "{B91D6A8D-2294-4BAA-AD31-AC1F296D82C4}", PublicName = "CK.WindowManager.Executor", Categories = new string[] { "Accessibility" }, Version = "1.0.0" )]
    public class WindowManagerExecutor : IPlugin
    {
        PreviewBindingInfo _placeholder;
        DefaultActivityLogger _logger;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWindowManager WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWindowBinder WindowBinder { get; set; }

        public WindowManagerExecutor()
        {
            _placeholder = new PreviewBindingInfo();
            _logger = new DefaultActivityLogger();
            //_logger.Tap.Register( new ActivityLoggerConsoleSink() );
        }

        void OnWindowManagerWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );
            IWindowElement triggerHolder = e.Window;
            // Gets all windows attached to the given window
            ISpatialBinding binding = WindowBinder.GetBinding( triggerHolder );

            if( binding != null )
            {
                //temporary
                //ResizingWindow( binding );
                PlacingWindow( binding );
                PlacingButton( binding );
            }
        }

        void OnWindowManagerWindowResized( object sender, WindowElementResizeEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );

            //IWindowElement triggerHolder = e.Window;
            //// Gets all windows attached to the given window
            //ISpatialBinding binding = WindowBinder.GetBinding( triggerHolder );
            //if( binding != null )
            //{
            //    //if( e.DeltaHeight != 0 )
            //    //{
            //    //    if( binding.Left != null ) ResizeVertically( e, binding.Left.SpatialBinding, BindingPosition.Bottom | BindingPosition.Right | BindingPosition.Top );
            //    //    if( binding.Right != null ) ResizeVertically( e, binding.Right.SpatialBinding, BindingPosition.Top | BindingPosition.Bottom | BindingPosition.Left );
            //    //    SpecialMoveBottom( e, binding );
            //    //}
            //    //if( e.DeltaWidth != 0 )
            //    //{
            //    //    if( binding.Top != null ) ResizeHorizontally( e, binding.Top.SpatialBinding, BindingPosition.Bottom | BindingPosition.Right | BindingPosition.Left );
            //    //    if( binding.Bottom != null ) ResizeHorizontally( e, binding.Bottom.SpatialBinding, BindingPosition.Top | BindingPosition.Right | BindingPosition.Left );
            //    //    SpecialMoveRight( e, binding );
            //    //}

            //    if( e.DeltaHeight != 0 || e.DeltaWidth != 0 )
            //    {
            //        //ResizingWindow( binding );
            //        //PlacingWindow( binding );
            //        //PlacingButton( binding );
            //    }
            //}
        }

        void PlacingWindow( ISpatialBinding binding, BindingPosition masterPosition = BindingPosition.None )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );

            IWindowElement reference = binding.Window;
            IWindowElement slave = null;

            if( masterPosition != BindingPosition.Top && binding.Top != null )
            {
                slave = binding.Top.SpatialBinding.Window;
                WindowManager.Move( binding.Top.SpatialBinding.Window, reference.Top - slave.Height, reference.Left );
                PlacingWindow( binding.Top.SpatialBinding, BindingPosition.Bottom );
            }
            if( masterPosition != BindingPosition.Bottom && binding.Bottom != null )
            {
                slave = binding.Bottom.SpatialBinding.Window;
                WindowManager.Move( binding.Bottom.SpatialBinding.Window, reference.Top + reference.Height, reference.Left );
                PlacingWindow( binding.Bottom.SpatialBinding, BindingPosition.Top );
            }
            if( masterPosition != BindingPosition.Left && binding.Left != null )
            {
                slave = binding.Left.SpatialBinding.Window;
                WindowManager.Move( binding.Left.SpatialBinding.Window, reference.Top, reference.Left - slave.Width );
                PlacingWindow( binding.Left.SpatialBinding, BindingPosition.Right );
            }
            if( masterPosition != BindingPosition.Right && binding.Right != null )
            {
                slave = binding.Right.SpatialBinding.Window;
                WindowManager.Move( binding.Right.SpatialBinding.Window, reference.Top, reference.Left + reference.Width );
                PlacingWindow( binding.Right.SpatialBinding, BindingPosition.Left );
            }
        }

        void ResizingWindow( ISpatialBinding binding, BindingPosition masterPosition = BindingPosition.None )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );

            IWindowElement reference = binding.Window;
            IWindowElement slave = null;


            if( masterPosition != BindingPosition.Top && binding.Top != null )
            {
                slave = binding.Top.SpatialBinding.Window;
                if( reference.Height != slave.Height )
                {
                    WindowManager.Resize( binding.Top.SpatialBinding.Window, binding.Window.Width, binding.Top.SpatialBinding.Window.Height );
                    ResizingWindow( binding.Top.SpatialBinding, BindingPosition.Bottom );
                }
            }
            if( masterPosition != BindingPosition.Bottom && binding.Bottom != null )
            {
                slave = binding.Bottom.SpatialBinding.Window;
                if( reference.Height != slave.Height )
                {
                    WindowManager.Resize( binding.Bottom.SpatialBinding.Window, binding.Window.Width, binding.Bottom.SpatialBinding.Window.Height );
                    ResizingWindow( binding.Bottom.SpatialBinding, BindingPosition.Top );
                }
            }
            if( masterPosition != BindingPosition.Left && binding.Left != null )
            {
                slave = binding.Left.SpatialBinding.Window;
                if( reference.Width != slave.Width )
                {
                    WindowManager.Resize( binding.Left.SpatialBinding.Window, binding.Left.SpatialBinding.Window.Width, binding.Window.Height );
                    ResizingWindow( binding.Left.SpatialBinding, BindingPosition.Right );
                }
            }
            if( masterPosition != BindingPosition.Right && binding.Right != null )
            {
                slave = binding.Right.SpatialBinding.Window;
                if( reference.Width != slave.Width )
                {
                    WindowManager.Resize( binding.Right.SpatialBinding.Window, binding.Right.SpatialBinding.Window.Width, binding.Window.Height );
                    ResizingWindow( binding.Right.SpatialBinding, BindingPosition.Left );
                }
            }
        }

        void PlacingButton( ISpatialBinding binding, BindingPosition masterPosition = BindingPosition.None )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );

            if( masterPosition != BindingPosition.Left && binding.Left != null )
            {
                binding.Left.UnbindButton.Move( binding.Window.Top + binding.Window.Height / 2 - binding.Left.UnbindButton.Window.Height / 2, binding.Window.Left - binding.Left.UnbindButton.Window.Width / 2 );
                PlacingButton( binding.Left.SpatialBinding, BindingPosition.Right );
            }

            if( masterPosition != BindingPosition.Right && binding.Right != null )
            {
                binding.Right.UnbindButton.Move( binding.Window.Top + binding.Window.Height / 2 - binding.Right.UnbindButton.Window.Height / 2, binding.Window.Left + binding.Window.Width - binding.Right.UnbindButton.Window.Width / 2 );
                PlacingButton( binding.Right.SpatialBinding, BindingPosition.Left );
            }

            if( masterPosition != BindingPosition.Bottom && binding.Bottom != null )
            {
                binding.Bottom.UnbindButton.Move( binding.Window.Top + binding.Window.Height - binding.Bottom.UnbindButton.Window.Height / 2, binding.Window.Left + binding.Window.Width / 2 - binding.Bottom.UnbindButton.Window.Width / 2 );
                PlacingButton( binding.Bottom.SpatialBinding, BindingPosition.Top );
            }

            if( masterPosition != BindingPosition.Top && binding.Top != null )
            {
                binding.Top.UnbindButton.Move( binding.Window.Top - binding.Top.UnbindButton.Window.Height / 2, binding.Window.Left + binding.Window.Width / 2 - binding.Top.UnbindButton.Window.Width / 2 );
                PlacingButton( binding.Top.SpatialBinding, BindingPosition.Bottom );
            }
        }

        //void ResizeHorizontally( WindowElementResizeEventArgs e, ISpatialBinding spatial, BindingPosition excludePos )
        //{
        //    Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );

        //    if( spatial != null )
        //    {
        //        var windows = spatial.AllDescendants( excludes: excludePos ).Union( new[] { spatial } );
        //        foreach( var window in windows )
        //        {
        //            double newWidth = window.Window.Width + e.DeltaWidth;
        //            //WindowManager.Resize( window.Window, newWidth, window.Window.Height );
        //            WindowManager.Resize( window.Window, new CallGetWithDelayed( () => { return spatial.Bottom.SpatialBinding.Window.Width; }, () => { return window.Window.Height; } ) );
        //            SpecialMoveRight( e, window );
        //        }
        //    }
        //}

        ///// <summary>
        ///// Special case for windows attached to the right during a resizing
        ///// </summary>
        //private void SpecialMoveRight( WindowElementResizeEventArgs e, ISpatialBinding spatial )
        //{
        //    Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );
        //    if( spatial != null )
        //    {
        //        foreach( var windowDesc in spatial.SubTree( BindingPosition.Right ) )
        //        {
        //            //WindowManager.Move( windowDesc, windowDesc.Top, windowDesc.Left + e.DeltaWidth ).Silent();
        //            WindowManager.Move( windowDesc, new CallGetWithDelayed( () => spatial.Window.Top, () => spatial.Window.Left + spatial.Window.Width ) ).Silent();
        //        }
        //    }
        //}

        //void ResizeVertically( WindowElementResizeEventArgs e, ISpatialBinding spatial, BindingPosition excludePos )
        //{
        //    Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );

        //    if( spatial != null )
        //    {
        //        var windows = spatial.AllDescendants( excludes: excludePos ).Union( new[] { spatial } );
        //        foreach( var window in windows )
        //        {
        //            double newHeight = window.Window.Height + e.DeltaHeight;
        //            //WindowManager.Resize( window.Window, window.Window.Width, newHeight );
        //            WindowManager.Resize( window.Window, new CallGetWithDelayed( () => { return window.Window.Width; }, () => { return window.Window.Height + e.DeltaHeight; } ) );
        //            SpecialMoveBottom( e, spatial );
        //        }
        //    }
        //}

        ///// <summary>
        ///// Special case for windows attached to the bottom during a resizing
        ///// </summary>
        //private void SpecialMoveBottom( WindowElementResizeEventArgs e, ISpatialBinding spatial )
        //{
        //    Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

        //    if( spatial != null )
        //    {
        //        //var window = binding.Bottom.Window;
        //        var windows = spatial.SubTree( BindingPosition.Bottom );
        //        foreach( var window in windows )
        //            //WindowManager.Move( window, window.Top + e.DeltaHeight, window.Left ).Silent();
        //            WindowManager.Move( window, new CallGetWithDelayed( () => spatial.Window.Top + spatial.Window.Height, () => spatial.Window.Left ) ).Silent();
        //    }
        //}

        void OnPreviewBinding( object sender, WindowBindedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( e.BindingType == BindingEventType.Attach )
            {
                if( !_placeholder.IsPreviewOf( e.Binding ) ) _placeholder.Display( e.Binding );
            }
            else _placeholder.Shutdown();
        }

        void OnBeforeBinding( object sender, WindowBindingEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );

            if( e.BindingType == BindingEventType.Attach )
            {
                Rect r = e.Binding.GetWindowArea();
                if( r != Rect.Empty )
                {
                    //Console.WriteLine( "OnBeforeBinding ! Origin : {0}", e.Binding.Origin.Name );
                    var move = WindowManager.Move( e.Binding.Origin, r.Top, r.Left );
                    var resize = WindowManager.Resize( e.Binding.Origin, r.Width, r.Height );
                    move.Broadcast();
                    resize.Broadcast();
                }
            }
        }

        void OnAfterBinding( object sender, WindowBindedEventArgs e )
        {
            _placeholder.Shutdown();
        }

        void OnWindowRestored( object sender, WindowElementEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );

            ISpatialBinding binding = WindowBinder.GetBinding( e.Window );
            if( binding != null )
            {
                // The Window that moves first
                foreach( ISpatialBinding descendant in binding.AllDescendants() )
                {
                    descendant.Window.Restore();
                    if( descendant.Top != null && !descendant.Top.UnbindButton.Window.IsVisible ) descendant.Top.UnbindButton.Window.Show();
                    if( descendant.Bottom != null && !descendant.Bottom.UnbindButton.Window.IsVisible ) descendant.Bottom.UnbindButton.Window.Show();
                    if( descendant.Right != null && !descendant.Right.UnbindButton.Window.IsVisible ) descendant.Right.UnbindButton.Window.Show();
                    if( descendant.Left != null && !descendant.Left.UnbindButton.Window.IsVisible ) descendant.Left.UnbindButton.Window.Show();
                }
            }
        }

        void OnWindowMinimized( object sender, WindowElementEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );

            ISpatialBinding binding = WindowBinder.GetBinding( e.Window );
            if( binding != null )
            {
                foreach( ISpatialBinding descendant in binding.AllDescendants() )
                {
                    descendant.Window.Minimize();
                    if( descendant.Top != null && descendant.Top.UnbindButton.Window.IsVisible ) descendant.Top.UnbindButton.Window.Hide();
                    if( descendant.Bottom != null && descendant.Bottom.UnbindButton.Window.IsVisible ) descendant.Bottom.UnbindButton.Window.Hide();
                    if( descendant.Right != null && descendant.Right.UnbindButton.Window.IsVisible ) descendant.Right.UnbindButton.Window.Hide();
                    if( descendant.Left != null && descendant.Left.UnbindButton.Window.IsVisible ) descendant.Left.UnbindButton.Window.Hide();
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
            WindowManager.WindowMinimized += OnWindowMinimized;
            WindowManager.WindowRestored += OnWindowRestored;

            WindowBinder.PreviewBinding += OnPreviewBinding;
            WindowBinder.BeforeBinding += OnBeforeBinding;
            WindowBinder.AfterBinding += OnAfterBinding;
        }

        public void Stop()
        {

            WindowManager.WindowResized -= OnWindowManagerWindowResized;
            WindowManager.WindowMoved -= OnWindowManagerWindowMoved;
            WindowManager.WindowMinimized -= OnWindowMinimized;
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
