using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace CK.WPF.Controls
{
    [ValueConversion( typeof( bool ), typeof( WindowState ) )]
    public class BooleanToIsMinimizedConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if( value != null )
            {
                bool booleanValue;
                if( bool.TryParse( value.ToString(), out booleanValue ) )
                {
                    return booleanValue ? WindowState.Minimized : WindowState.Normal;
                }
            }
            return WindowState.Normal;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if(value != null)
            {
                WindowState ws = WindowState.Normal;
                if( Enum.TryParse<WindowState>( value.ToString(), out ws ) )
                {
                    return ws == WindowState.Minimized;
                }
            }
            return false;
        }
    }
}
