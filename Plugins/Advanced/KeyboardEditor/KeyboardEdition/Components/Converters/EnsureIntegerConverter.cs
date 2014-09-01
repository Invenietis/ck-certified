using System;
using System.Windows.Data;

namespace KeyboardEditor.ViewModels
{
    [ValueConversion( typeof( string ), typeof( int ) )]
    public class EnsureIntegerConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            int finalVal = 0;
            Int32.TryParse( value.ToString(), out finalVal );
            return finalVal;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            int finalVal = 0;
            Int32.TryParse( value.ToString(), out finalVal );
            return finalVal;
        }
    }
}
