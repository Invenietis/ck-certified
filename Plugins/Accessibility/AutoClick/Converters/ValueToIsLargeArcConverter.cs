using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CK.Plugins.AutoClick.Converters
{
    public class ValueToIsLargeArcConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {

            return ((Double.Parse( value.ToString() ) * 2) >= 100);
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
