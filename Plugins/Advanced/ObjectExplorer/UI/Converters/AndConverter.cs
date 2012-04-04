using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CK.Plugins.ObjectExplorer
{
    public class AndMultiValueConverter : IMultiValueConverter
    {

        public object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            foreach( bool value in values )
            {
                if( !value )
                    return false;
            }

            return true;
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
        {
            return null;
        }
    }
}
