using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CK.Plugins.ObjectExplorer
{
    public class OrConverter : IMultiValueConverter
    {

        public object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            foreach( bool value in values )
            {
                if( value )
                    return true;
            }

            return false;
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
        {
            return null;
        }
    }
}
