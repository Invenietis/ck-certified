using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CK.StandardPlugins.AutoClick.Views
{
    public class WPFClickTypeWatch : ActionOnMouseEnterButton
    {
        static WPFClickTypeWatch()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( WPFClickTypeWatch ), new FrameworkPropertyMetadata( typeof( WPFClickTypeWatch ) ) );
            ValueProperty = DependencyProperty.Register( "Value", typeof( int ), typeof( WPFClickTypeWatch ) );
            DisabledColorProperty = DependencyProperty.Register( "DisabledColor", typeof( LinearGradientBrush ), typeof( WPFClickTypeWatch ) );
            IsPausedProperty = DependencyProperty.Register( "IsPaused", typeof( bool ), typeof( WPFClickTypeWatch ) );
        }

        public static DependencyProperty ValueProperty;
        public bool Value
        {
            get { return (bool)GetValue( ValueProperty ); }
            set { SetValue( ValueProperty, value ); }
        }

        public static DependencyProperty DisabledColorProperty;
        public LinearGradientBrush DisabledColor
        {
            get { return (LinearGradientBrush)GetValue( DisabledColorProperty ); }
            set { SetValue( DisabledColorProperty, value ); }
        }

        public static DependencyProperty IsPausedProperty;
        public bool IsPaused
        {
            get { return (bool)GetValue( IsPausedProperty ); }
            set { SetValue( IsPausedProperty, value ); }
        }
    }
}
