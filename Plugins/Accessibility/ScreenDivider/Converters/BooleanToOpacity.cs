using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ScreenDivider.Converters
{
    [ValueConversion( typeof( bool ), typeof( double ) )]
    public class BooleanToOpacity : IValueConverter
    {
        #region IValueConverter Members

        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            var val = System.Convert.ToBoolean( value, CultureInfo.InvariantCulture );
            if( val )
            {
                return 0.10;
            }
            return 0.5;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
