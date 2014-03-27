using System.Linq;
using CK.Plugin;
using CK.WindowManager.Model;
using CK.Core;
using System.Windows;
using System;
using CommonServices;
using System.Windows.Threading;
using CK.Windows;

namespace CK.WindowManager
{
    [Plugin( "{B91D6A8D-2294-4BAA-AD31-AC1F296D82C4}", PublicName = "CK.WindowManager.Executor", Categories = new string[] { "Accessibility" }, Version = "1.0.0" )]
    public class WindowManagerExecutor : IPlugin
    {
        PreviewBindingInfo _placeholder;
        DefaultActivityLogger _logger;
        //Action _resized; //warning cyclelife : it's use for OnPointerButtonUp

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
            // Gets all windows attached to the given window
            ISpatialBinding binding = WindowBinder.GetBinding( triggerHolder );

            if( binding != null && binding.AllDescendants().Count() != 0 )
            {
                PlacingWindow( binding, binding.Window );
                PlacingButton( binding, binding );
            }
        }

        void OnWindowManagerWindowResized( object sender, WindowElementResizeEventArgs e )
        {
            IWindowElement triggerHolder = e.Window;
            // Gets all windows attached to the given window
            ISpatialBinding binding = WindowBinder.GetBinding( triggerHolder );
            if( binding != null )
            {
                //To simplify the multithread management we avoid hiding and showing the buttons. 
                //(their visibility used to be bound to event like the mousebuttonup/mousebuttondown, which is a problem multi-thread wise)   
                //if ( ( e.DeltaHeight != 0 || e.DeltaWidth != 0 ) && binding.Left != null )
                //{
                //    binding.Left.UnbindButton.Window.Dispatcher.BeginInvoke( (Action)( () => HidingButton( binding ) ) );
                //}

                if( e.DeltaHeight != 0 )
                {
                    if( binding.Left != null ) ResizeVertically( e, binding.Left.SpatialBinding, BindingPosition.Bottom | BindingPosition.Right | BindingPosition.Top );
                    if( binding.Right != null ) ResizeVertically( e, binding.Right.SpatialBinding, BindingPosition.Top | BindingPosition.Bottom | BindingPosition.Left );
                    SpecialMoveBottom( e, binding );
                }
                if( e.DeltaWidth != 0 )
                {
                    if( binding.Top != null ) ResizeHorizontally( e, binding.Top.SpatialBinding, BindingPosition.Bottom | BindingPosition.Right | BindingPosition.Left );
                    if( binding.Bottom != null ) ResizeHorizontally( e, binding.Bottom.SpatialBinding, BindingPosition.Top | BindingPosition.Right | BindingPosition.Left );
                    SpecialMoveRight( e, binding );
                }

                if( e.DeltaHeight != 0 || e.DeltaWidth != 0 )
                {
                    PlacingButton( binding, binding );
                }
            }
        }

        void PlacingWindow( ISpatialBinding binding, IWindowElement master )
        {
            IWindowElement window = null;
            if( binding.Top != null && binding.Top.SpatialBinding.Window != master )
            {
                window = binding.Top.SpatialBinding.Window;
                WindowManager.Move( window, binding.Window.Top - window.Height, binding.Window.Left );
                PlacingWindow( binding.Top.SpatialBinding, binding.Window );
            }
            if( binding.Bottom != null && binding.Bottom.SpatialBinding.Window != master )
            {
                window = binding.Bottom.SpatialBinding.Window;
                WindowManager.Move( window, binding.Window.Top + binding.Window.Height, binding.Window.Left );
                PlacingWindow( binding.Bottom.SpatialBinding, binding.Window );
            }
            if( binding.Left != null && binding.Left.SpatialBinding.Window != master )
            {
                window = binding.Left.SpatialBinding.Window;
                WindowManager.Move( window, binding.Window.Top, binding.Window.Left - window.Width );
                PlacingWindow( binding.Left.SpatialBinding, binding.Window );
            }
            if( binding.Right != null && binding.Right.SpatialBinding.Window != master )
            {
                window = binding.Right.SpatialBinding.Window;
                WindowManager.Move( window, binding.Window.Top, binding.Window.Left + binding.Window.Width );
                PlacingWindow( binding.Right.SpatialBinding, binding.Window );
            }

        }

        void PlacingButton( ISpatialBinding binding, ISpatialBinding master )
        {
            double top = 0;
            double height = 0;
            double width = 0;
            double left = 0;

            top = binding.Window.Top;
            height = binding.Window.Height;
            width = binding.Window.Width;
            left = binding.Window.Left;

            if( binding.Left != null && binding.Left.SpatialBinding != master )
            {
                binding.Left.UnbindButton.Window.Dispatcher.BeginInvoke( (Action)(() =>
                {
                    binding.Left.UnbindButton.Move( top + height / 2 - binding.Left.UnbindButton.Window.Height / 2, left - binding.Left.UnbindButton.Window.Width / 2 );
                    //binding.Left.UnbindButton.Window.Show();
                }) );
                PlacingButton( binding.Left.SpatialBinding, binding );
            }

            if( binding.Right != null && binding.Right.SpatialBinding != master )
            {
                binding.Right.UnbindButton.Window.Dispatcher.BeginInvoke( (Action)(() =>
                {
                    binding.Right.UnbindButton.Move( top + height / 2 - binding.Right.UnbindButton.Window.Height / 2, left + width - binding.Right.UnbindButton.Window.Width / 2 );
                    //binding.Right.UnbindButton.Window.Show();
                }) );
                PlacingButton( binding.Right.SpatialBinding, binding );
            }

            if( binding.Bottom != null && binding.Bottom.SpatialBinding != master )
            {
                binding.Bottom.UnbindButton.Window.Dispatcher.BeginInvoke( (Action)(() =>
                {
                    binding.Bottom.UnbindButton.Move( top + height - binding.Bottom.UnbindButton.Window.Height / 2, left + width / 2 - binding.Bottom.UnbindButton.Window.Width / 2 );
                    //binding.Bottom.UnbindButton.Window.Show();
                }) );
                PlacingButton( binding.Bottom.SpatialBinding, binding );
            }

            if( binding.Top != null && binding.Top.SpatialBinding != master )
            {
                binding.Top.UnbindButton.Window.Dispatcher.BeginInvoke( (Action)(() =>
                {
                    binding.Top.UnbindButton.Move( top - binding.Top.UnbindButton.Window.Height / 2, left + width / 2 - binding.Top.UnbindButton.Window.Width / 2 );
                    //binding.Top.UnbindButton.Window.Show();
                }) );
                PlacingButton( binding.Top.SpatialBinding, binding );
            }
        }

        void ResizeHorizontally( WindowElementResizeEventArgs e, ISpatialBinding spatial, BindingPosition excludePos )
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

        void ResizeVertically( WindowElementResizeEventArgs e, ISpatialBinding spatial, BindingPosition excludePos )
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
                DispatchWhenRequired( e.Binding.Origin.Window.Dispatcher, (Action)(() =>
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
                }), false );
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
                foreach( ISpatialBinding descendant in binding.AllDescendants() )
                {
                    descendant.Window.Restore();
                    DispatchWhenRequired( NoFocusManager.Default.NoFocusDispatcher, (Action)(() =>
                    {
                        if( descendant.Top != null && !descendant.Top.UnbindButton.Window.IsVisible ) descendant.Top.UnbindButton.Window.Show();
                        if( descendant.Bottom != null && !descendant.Bottom.UnbindButton.Window.IsVisible ) descendant.Bottom.UnbindButton.Window.Show();
                        if( descendant.Right != null && !descendant.Right.UnbindButton.Window.IsVisible ) descendant.Right.UnbindButton.Window.Show();
                        if( descendant.Left != null && !descendant.Left.UnbindButton.Window.IsVisible ) descendant.Left.UnbindButton.Window.Show();
                    }), false );
                }
            }
        }

        void OnWindowMinimized( object sender, WindowElementEventArgs e )
        {
            ISpatialBinding binding = WindowBinder.GetBinding( e.Window );
            if( binding != null )
            {
                foreach( ISpatialBinding descendant in binding.AllDescendants() )
                {
                    descendant.Window.Minimize();
                    DispatchWhenRequired( NoFocusManager.Default.NoFocusDispatcher, (Action)(() =>
                    {
                        if( descendant.Top != null && descendant.Top.UnbindButton.Window.IsVisible ) descendant.Top.UnbindButton.Window.Hide();
                        if( descendant.Bottom != null && descendant.Bottom.UnbindButton.Window.IsVisible ) descendant.Bottom.UnbindButton.Window.Hide();
                        if( descendant.Right != null && descendant.Right.UnbindButton.Window.IsVisible ) descendant.Right.UnbindButton.Window.Hide();
                        if( descendant.Left != null && descendant.Left.UnbindButton.Window.IsVisible ) descendant.Left.UnbindButton.Window.Hide();
                    }), false );
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

        private T DispatchWhenRequired<T>( Dispatcher dispatcher, Func<T> f )
        {
            if( dispatcher.CheckAccess() ) return f();

            return (T)dispatcher.Invoke( f );
        }

        private void DispatchWhenRequired( Dispatcher dispatcher, Action d, bool synchronous = true )
        {
            if( dispatcher.CheckAccess() ) d();
            else
            {
                if( synchronous )
                    dispatcher.Invoke( d );
                else
                    dispatcher.BeginInvoke( d );
            }
        }
    }
}
