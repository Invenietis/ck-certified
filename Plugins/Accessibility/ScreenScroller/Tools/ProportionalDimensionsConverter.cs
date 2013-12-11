using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ScreenScroller
{
    public class ProportionalDimensionsConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if( value == null ) throw new ArgumentNullException( "The value set for the ProportionalDimensiosnConverter is null (parameter = " + parameter.ToString() + ")" );

            double elementSize = Double.Parse( value.ToString() );
            int proportionRatio = Int32.Parse( parameter.ToString() );

            return elementSize / proportionRatio;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}
