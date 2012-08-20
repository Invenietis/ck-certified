using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CK.WPF.Controls
{
    [ValueConversion( typeof( string ), typeof( bool ) )]
    public class StringIsNullOrWhiteSpaceToBoolean : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if(value.GetType() != typeof(string)) return false;
            return String.IsNullOrWhiteSpace( value.ToString() );
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {            
            return null;
        }
    }
}
