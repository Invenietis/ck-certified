using System;
using System.Windows.Data;

namespace KeyboardEditor.ViewModels
{
    [ValueConversion( typeof( object ), typeof( Type ) )]
    public class TypeConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return value == null ? null : value.GetType();
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return Binding.DoNothing;
        }
    }
}
