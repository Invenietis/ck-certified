using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ScreenDivider.Events;

namespace ScreenDivider.ViewModels
{
    public class GridViewModel
    {
        readonly WindowViewModel _window;
        DispatcherTimer _timer = new DispatcherTimer();
        Stack<PanelViewModel> _panels = new Stack<PanelViewModel>();

        int _loop = 0;

        /// <summary>
        /// This property getting the current panel that is highlighted
        /// </summary>
        public PanelViewModel Current
        {
            get
            {
                return _panels.Count != 0 ? _panels.Peek() : null;
            }
        }

        /// <summary>
        /// This property expose a ReadOnlyList of the internal panels stacks
        /// </summary>
        public ICollection<PanelViewModel> Panels { get { return _panels.ToList().AsReadOnly(); } }

        /// <summary>
        /// This property getting the max deep
        /// </summary>
        public int MaxDeep { get { return 7; } }

        public GridViewModel( WindowViewModel window )
        {
            _window = window;
            var m = new PanelViewModel( this );
            _panels.Push( m );

            _timer.Tick += delegate( object s, EventArgs args )
            {
                if( Current != null )
                    Current.Switch();
                else
                    _timer.Stop();
            };
            _timer.Interval = new TimeSpan( 0, 0, 0, 0, 2000 );
        }

        internal void PauseWindowOwner()
        {
            _window.Pause();
        }

        /// <summary>
        /// This method drives all panel switching
        /// </summary>
        public void Switch()
        {
            Debug.Assert( Current != null );
            _timer.Start();
        }

        /// <summary>
        /// Manually restart switching
        /// </summary>
        public void RestartSwitch()
        {
            _loop++;
        }

        /// <summary>
        /// This method drives all panel entering
        /// </summary>
        public void Enter()
        {
            Debug.Assert( Current != null );
            Debug.Assert( _panels.Count > 0 );

            if( _timer.IsEnabled )
            {
                if( (_panels.Count - 1) < MaxDeep )
                {
                    _timer.Stop();
                    var newPanel = Current.Enter();
                    _panels.Push( newPanel );
                    _timer.Start();
                }
                else
                {
                    _timer.Stop();
                    MessageBox.Show( "You have reached the max deep !", "Warning...", MessageBoxButton.OK, MessageBoxImage.Exclamation );
                }
            }
            else
            {
                PauseWindowOwner();
                _timer.Start();
            }
        }

        #region Events

        public event EventHandler<ExitGridEventArgs> ExitNode;

        public event EventHandler<GoToWindowEventArgs> LastNodeBeforeGoToWindow;

        public void ExitCommand()
        {
            _timer.Stop();
            if( _panels.Count <= 1 )
            {
                if( (_loop > 2) ) LastNodeBeforeGoToWindow( this, new GoToWindowEventArgs() );
                if( !_window.IsPause && (_loop >= 2 || _window.IsOnlyOne) )
                {
                    _loop = 0;
                    PauseWindowOwner();
                }
                if( ExitNode != null && !_window.IsPause ) ExitNode( this, new ExitGridEventArgs() );
            }
            else
            {
                var panelToRemove = _panels.Pop();
                if( panelToRemove != null ) panelToRemove.Exit();
                _timer.Start();
            }
        }

        #endregion
    }
}
