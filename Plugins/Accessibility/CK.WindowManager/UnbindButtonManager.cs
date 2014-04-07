using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using CK.Plugin;
using CK.WindowManager.Model;
using CK.Windows;
using CommonServices;

namespace CK.WindowManager
{
    [Plugin( "{BEA2BC3A-B7A1-4AF5-A86E-A039B7197BA8}", Categories = new string[] { "Accessibility" }, PublicName = "CK.WindowManager.UnbindButtonManager", Version = "1.0.0" )]
    public class UnbindButtonManager : IPlugin, IUnbindButtonManager
    {
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITopMostService> TopMostService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IService<IPointerDeviceDriver> PointerDeviceDriver { get; set; }

        Dictionary<IWindowElement, UnbindButtonContainer> _unbindButtonContainers;

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            _unbindButtonContainers = new Dictionary<IWindowElement, UnbindButtonContainer>();
            return true;
        }

        public void Start()
        {
            _timer = new DispatcherTimer( DispatcherPriority.Background );
            _timer.Interval = new TimeSpan( 0, 0, 0, 0, 10 );
            _timer.Tick += _timer_Tick;

            WindowManager.Service.WindowResized += OnWindowManagerWindowResized;
            WindowManager.Service.WindowMoved += OnWindowManagerWindowMoved;
            WindowManager.Service.WindowMinimized += OnWindowMinimized;
            WindowManager.Service.WindowRestored += OnWindowRestored;

            WindowBinder.Service.AfterBinding += OnAfterBinding;

            PointerDeviceDriver.Service.PointerButtonUp += OnPointerButtonUp;
        }

        public void Stop()
        {
            _timer.Tick -= _timer_Tick;

            WindowManager.Service.WindowResized -= OnWindowManagerWindowResized;
            WindowManager.Service.WindowMoved -= OnWindowManagerWindowMoved;
            WindowManager.Service.WindowMinimized -= OnWindowMinimized;
            WindowManager.Service.WindowRestored -= OnWindowRestored;

            WindowBinder.Service.AfterBinding -= OnAfterBinding;

            PointerDeviceDriver.Service.PointerButtonUp -= OnPointerButtonUp;

            _unbindButtonContainers.Clear();
        }

        public void Teardown()
        {
        }

        #endregion

        #region Create Methods

        void CreateButton( IWindowElement master, IWindowElement slave, BindingPosition position )
        {
            FillContainers( master, slave, position );
        }

        void FillContainers( IWindowElement master, IWindowElement slave, BindingPosition position )
        {
            UnbindButtonContainer masterContainer;
            UnbindButtonContainer slaveContainer;

            if( !_unbindButtonContainers.TryGetValue( slave, out slaveContainer ) )
            {
                slaveContainer = new UnbindButtonContainer( slave );
                _unbindButtonContainers.Add( slave, slaveContainer );
            }
            if( !_unbindButtonContainers.TryGetValue( master, out masterContainer ) )
            {
                masterContainer = new UnbindButtonContainer( master );
                _unbindButtonContainers.Add( master, masterContainer );
            }

            Debug.Assert( masterContainer.GetUnbindFromPosition( position ) == null );
            Debug.Assert( slaveContainer.GetUnbindFromPosition( GetOppositePosition( position ) ) == null );

            IWindowElement button = InitializeButton( master, slave, position );

            masterContainer.SetButton( button, position );
            slaveContainer.SetButton( button, GetOppositePosition( position ) );

            button.Show();
            EnableUnbindButtons();
        }

        WindowElement InitializeButton( IWindowElement window, IWindowElement other, BindingPosition position )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            WindowElement button = null;

            button = new WindowElement( new UnbindButtonView()
            {
                DataContext = CreateVM( window, other, position )
            }, "UnbindButton" );

            DoPlaceButtons( button, window, position );

            TopMostService.Service.RegisterTopMostElement( "30", button.Window );

            return button;
        }

        VMUnbindButton CreateVM( IWindowElement window, IWindowElement other, BindingPosition position )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( position == BindingPosition.None )
                return null;

            Action move = null;

            if( position == BindingPosition.Top )
            {
                move = () =>
                {
                    WindowManager.Service.Move( window, window.Top + 10, window.Left ).Broadcast();
                    WindowManager.Service.Move( other, other.Top - 10, other.Left ).Broadcast();
                };
            }
            else if( position == BindingPosition.Bottom )
            {
                move = () =>
                {
                    WindowManager.Service.Move( window, window.Top - 10, window.Left ).Broadcast();
                    WindowManager.Service.Move( other, other.Top + 10, other.Left ).Broadcast();
                };
            }
            else if( position == BindingPosition.Right )
            {
                move = () =>
                {
                    WindowManager.Service.Move( window, window.Top, window.Left - 10 ).Broadcast();
                    WindowManager.Service.Move( other, other.Top, other.Left + 10 ).Broadcast();
                };
            }
            else if( position == BindingPosition.Left )
            {
                move = () =>
                {
                    WindowManager.Service.Move( window, window.Top, window.Left + 10 ).Broadcast();
                    WindowManager.Service.Move( other, other.Top, other.Left - 10 ).Broadcast();
                };
            }

            return new VMUnbindButton( () =>
            {
                WindowBinder.Service.Unbind( window, other, false );
                move();
                EnableUnbindButtons();
            } );
        }

        void DoPlaceButtons( IWindowElement button, IWindowElement window, BindingPosition position )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            double top = window.Top;
            double height = window.Height;
            double width = window.Width;
            double left = window.Left;

            if( position == BindingPosition.Top )
            {
                button.Window.Top = top - button.Height / 2;
                button.Window.Left = left + width / 2 - button.Width / 2;
            }
            else if( position == BindingPosition.Bottom )
            {
                button.Window.Top = top + height - button.Height / 2;
                button.Window.Left = left + width / 2 - button.Width / 2;
            }
            else if( position == BindingPosition.Right )
            {
                button.Window.Top = top + height / 2 - button.Height / 2;
                button.Window.Left = left + width - button.Width / 2;
            }
            else if( position == BindingPosition.Left )
            {
                button.Window.Top = top + height / 2 - button.Width / 2;
                button.Window.Left = left - button.Height / 2;
            }
        }

        #endregion Create Methods

        #region Remove Methods

        void RemoveButton( IWindowElement master, IWindowElement slave )
        {
            if( _unbindButtonContainers.ContainsKey( slave ) && _unbindButtonContainers.ContainsKey( master ) )
            {
                BindingPosition position = GetPositionOfCommonButton( master, slave );
                if( position != BindingPosition.None )
                {
                    _unbindButtonContainers[master][position].Close();
                    _unbindButtonContainers[master][position] = null;
                    _unbindButtonContainers[slave][GetOppositePosition( position )] = null;
                }

                CleanUnbindButtonContainers( master );
                CleanUnbindButtonContainers( slave );
            }
        }

        public BindingPosition GetPositionOfCommonButton( IWindowElement master, IWindowElement slave )
        {
            UnbindButtonContainer m = _unbindButtonContainers[master];
            UnbindButtonContainer s = _unbindButtonContainers[slave];

            if( m.TopButton != null && s.BottomButton != null && m.TopButton == s.BottomButton ) return BindingPosition.Top;
            if( m.RightButton != null && s.LeftButton != null && m.RightButton == s.LeftButton ) return BindingPosition.Right;
            if( m.LeftButton != null && s.RightButton != null && m.LeftButton == s.RightButton ) return BindingPosition.Left;
            if( m.BottomButton != null && s.TopButton != null && m.BottomButton == s.TopButton ) return BindingPosition.Bottom;
            return BindingPosition.None;
        }

        void CleanUnbindButtonContainers( IWindowElement window )
        {
            UnbindButtonContainer container;
            if( _unbindButtonContainers.TryGetValue( window, out container ) )
            {
                if( container.TopButton == null
                    && container.BottomButton == null
                    && container.LeftButton == null
                    && container.RightButton == null ) 
                    _unbindButtonContainers.Remove( window );
            }
        }

        #endregion Remove Methods

        #region OnXXX

        private void OnAfterBinding( object sender, WindowBindedEventArgs e )
        {
            IWindowElement master = e.Binding.Target;
            IWindowElement slave = e.Binding.Origin;
            BindingPosition position = e.Binding.Position;
            if( e.BindingType == BindingEventType.Attach )
            {
                CreateButton( master, slave, position );
            }
            else if( e.BindingType == BindingEventType.Detach )
            {
                RemoveButton( master, slave );
            }
        }

        private void OnWindowRestored( object sender, WindowElementEventArgs e )
        {
            UnbindButtonContainer container;
            if( _unbindButtonContainers.TryGetValue( e.Window, out container ) ) container.ShowButtons();
        }

        private void OnWindowMinimized( object sender, WindowElementEventArgs e )
        {
            UnbindButtonContainer container;
            if( _unbindButtonContainers.TryGetValue( e.Window, out container ) ) container.HideButtons();
        }

        bool _windowMoved = false;
        bool _buttonIsVisible = true;
        private void OnWindowManagerWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            _windowMoved = true;
            if( _buttonIsVisible )
            {
                foreach( var c in _unbindButtonContainers.Values ) c.HideButtons();
            }
        }

        private void OnWindowManagerWindowResized( object sender, WindowElementResizeEventArgs e )
        {
            _windowMoved = true;
            if( _buttonIsVisible )
            {
                foreach( var c in _unbindButtonContainers.Values ) c.HideButtons();
            }
        }

        DispatcherTimer _timer;
        private void OnPointerButtonUp( object sender, PointerDeviceEventArgs e )
        {
            if( _windowMoved && !_timer.IsEnabled )
            {
                _timer.Start();
            }
        }

        void _timer_Tick( object sender, EventArgs e )
        {
            _timer.Stop();
            _windowMoved = false;
            EnableUnbindButtons();
        }

        void EnableUnbindButtons()
        {
            foreach( var c in _unbindButtonContainers.Values )
            {
                c.PlacingButton();
                c.ShowButtons();
            }
        }

        #endregion OnXXX

        #region Helper methods

        internal static BindingPosition GetOppositePosition( BindingPosition position )
        {
            if( position == BindingPosition.Bottom ) return BindingPosition.Top;
            if( position == BindingPosition.Top ) return BindingPosition.Bottom;
            if( position == BindingPosition.Left ) return BindingPosition.Right;
            if( position == BindingPosition.Right ) return BindingPosition.Left;
            return BindingPosition.None;
        }

        #endregion Helper methods

        class UnbindButtonContainer
        {

            public UnbindButtonContainer( IWindowElement window )
            {
                Window = window;
            }

            public IWindowElement Window { get; private set; }
            public IWindowElement TopButton { get; private set; }
            public IWindowElement BottomButton { get; private set; }
            public IWindowElement LeftButton { get; private set; }
            public IWindowElement RightButton { get; private set; }

            public IWindowElement this[BindingPosition p]
            {
                get
                {
                    return GetUnbindFromPosition( p );
                }
                set
                {
                    SetButton( value, p );
                }
            }

            public void PlacingButton()
            {
                double top = Window.Top;
                double height = Window.Height;
                double width = Window.Width;
                double left = Window.Left;

                if( TopButton != null ) TopButton.Move( top - TopButton.Height / 2, left + width / 2 - TopButton.Width / 2 );

                if( BottomButton != null ) BottomButton.Move( top + height - BottomButton.Height / 2, left + width / 2 - BottomButton.Width / 2 );

                if( RightButton != null ) RightButton.Move( top + height / 2 - RightButton.Height / 2, left + width - RightButton.Width / 2 );

                if( LeftButton != null ) LeftButton.Move( top + height / 2 - LeftButton.Width / 2, left - LeftButton.Height / 2 );
            }

            public void HideButtons()
            {
                if( TopButton != null && TopButton.Window.IsVisible ) TopButton.Hide();
                if( BottomButton != null && BottomButton.Window.IsVisible ) BottomButton.Hide();
                if( LeftButton != null && LeftButton.Window.IsVisible ) LeftButton.Hide();
                if( RightButton != null && RightButton.Window.IsVisible ) RightButton.Hide();
            }

            public void ShowButtons()
            {
                if( TopButton != null && TopButton.Window.IsLoaded && !TopButton.Window.IsVisible ) TopButton.Show();
                if( BottomButton != null && BottomButton.Window.IsLoaded && !BottomButton.Window.IsVisible ) BottomButton.Show();
                if( LeftButton != null && LeftButton.Window.IsLoaded && !LeftButton.Window.IsVisible ) LeftButton.Show();
                if( RightButton != null && RightButton.Window.IsLoaded && !RightButton.Window.IsVisible ) RightButton.Show();
            }

            public IWindowElement GetUnbindFromPosition( BindingPosition position )
            {
                if( position == BindingPosition.Top ) return TopButton;
                else if( position == BindingPosition.Bottom ) return BottomButton;
                else if( position == BindingPosition.Left ) return LeftButton;
                else if( position == BindingPosition.Right ) return RightButton;
                return null;
            }

            public void SetButton( IWindowElement button, BindingPosition position )
            {
                Debug.Assert( position != BindingPosition.None );
                if( position == BindingPosition.Top ) TopButton = button;
                else if( position == BindingPosition.Bottom ) BottomButton = button;
                else if( position == BindingPosition.Left ) LeftButton = button;
                else if( position == BindingPosition.Right ) RightButton = button;
            }
        }
    }
}
