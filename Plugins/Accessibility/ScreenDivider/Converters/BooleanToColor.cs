using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ScreenDivider.Converters
{
    [ValueConversion( typeof( bool ), typeof( SolidColorBrush ) )]
    public class BooleanToColor : IValueConverter
    {
        public bool IsReversed { get; set; }
        public bool UseHidden { get; set; }
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            string color = "#FF9900";
            var val = System.Convert.ToBoolean( value, CultureInfo.InvariantCulture );
            if( this.IsReversed )
            {
                val = !val;
            }
            if( val )
            {
                return BrushConverterString( color );
            }
            return new SolidColorBrush( Color.FromRgb( 204, 204, 204 ) );
        }

        SolidColorBrush BrushConverterString( string color )
        {
            var b = new BrushConverter();
            return (SolidColorBrush)b.ConvertFrom( color );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}
