using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace CK.Plugins.AutoClick.Converters
{
    public class ValueToPointConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            double current = (Double.Parse(value.ToString()) / (double)100) * (double)360;

            current = current - 90;

            current = current * 0.017453292519943295;

            double x = 10 + 10 * Math.Cos( current );
            double y = 10 + 10 * Math.Sin( current );

            return new Point( x, y );
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
