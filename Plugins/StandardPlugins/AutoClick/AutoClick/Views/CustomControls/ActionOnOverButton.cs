using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CK.StandardPlugins.AutoClick.Views
{
    /// <summary>
    ///  Button that will send its MouseEnterCommand command when the mouse stays over it for 0.5 second
    /// </summary>
    public class ActionOnOverButton : Button
    {
        DispatcherTimer _overTimer;

        static ActionOnOverButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( ActionOnOverButton ), new FrameworkPropertyMetadata( typeof( ActionOnOverButton ) ) );
            MouseEnterCommandProperty = DependencyProperty.Register( "MouseEnterCommand", typeof( ICommand ), typeof( ActionOnOverButton ) );
        }           

        static DependencyProperty MouseEnterCommandProperty;
        public ICommand MouseEnterCommand
        {
            get { return (ICommand)GetValue( MouseEnterCommandProperty ); }
            set { SetValue( MouseEnterCommandProperty, value ); }
        }

        protected override void OnMouseEnter( MouseEventArgs e )
        {
            if( _overTimer == null )
            {
                _overTimer = new DispatcherTimer();
                _overTimer.Interval = new TimeSpan( 0, 0, 0, 0, 500 );
                _overTimer.Tick += new EventHandler( OnTick );
            }
            _overTimer.Start();
            FireCommand( MouseEnterCommand );
            base.OnMouseEnter( e );
        }

        protected override void OnMouseLeave( MouseEventArgs e )
        {
            //_overTimer.Tick -= new EventHandler( OnTick );
            _overTimer.Stop();
            //_overTimer = null;
            base.OnMouseLeave( e );
        }

        void FireCommand( ICommand command )
        {
            if( command != null )
            {
                if( command.CanExecute( true ) )
                    command.Execute( this );
            }
        }

        private void OnTick( object sender, EventArgs e )
        {
            FireCommand( MouseEnterCommand );
        }
    }
}
