using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScrollerVisualizer
{
    public class ItemControlConverter : IMultiValueConverter
    {
        #region Implementation of IMultiValueConverter

        public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            if( values != null && values.Length == 2 &&
                values[0] is int && values[1] is int )
            {
                return Equals( (int)values[0], (int)values[1] + 1 );
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}