using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CK.Plugins.ObjectExplorer
{
    public class NotConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            bool result = false;
            if( bool.TryParse( value.ToString(), out result ) )
            {
                return !result;
            }
            throw new Exception( "WPF NotConverter : The NotConverter needs a boolean as parameter." );
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return null;
        }
    }
}
