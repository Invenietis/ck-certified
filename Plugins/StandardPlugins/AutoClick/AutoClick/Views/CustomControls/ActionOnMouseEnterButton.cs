using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace CK.StandardPlugins.AutoClick.Views
{
    public class ActionOnMouseEnterButton : Button
    {
        static ActionOnMouseEnterButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( ActionOnMouseEnterButton ), new FrameworkPropertyMetadata( typeof( ActionOnMouseEnterButton ) ) );
            IsPausedProperty = DependencyProperty.Register( "IsPaused", typeof( bool ), typeof( ActionOnMouseEnterButton ) ); 
        }
        
        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register( "Selected", typeof( bool ), typeof( ActionOnMouseEnterButton ) );
        public bool Selected
        {
            get { return (bool)GetValue( SelectedProperty ); }
            set { SetValue( SelectedProperty, value ); }
        }

        static DependencyProperty IsPausedProperty;
        public bool IsPaused
        {
            get { return (bool)GetValue( IsPausedProperty ); }
            set { SetValue( IsPausedProperty, value ); }
        }

        public ICommand MouseEnterCommand
        {
            get { return (ICommand)GetValue( MouseEnterCommandProperty ); }
            set 
            {
                SetValue( MouseEnterCommandProperty, value ); 
            }
        }
        public static readonly DependencyProperty MouseEnterCommandProperty =
        DependencyProperty.Register( "MouseEnterCommand", typeof( ICommand ), typeof( ActionOnMouseEnterButton ) );

        //TODO : Replace by a Behavior.
        protected override void OnMouseEnter( MouseEventArgs e )
        {
            FireCommand( MouseEnterCommand );
        }
        void FireCommand( ICommand command )
        {
            if( command != null )
            {
                if( command.CanExecute( true ) )
                    command.Execute( this );
            }
        }     
    }
}
