using System;
using System.Windows.Data;

namespace KeyboardEditor.ViewModels
{
    [ValueConversion( typeof( object ), typeof( bool ) )]
    public class DeletableComponentConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return (value is VMKeyEditable || value is VMZoneEditable);
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return Binding.DoNothing;
        }
    }
}
