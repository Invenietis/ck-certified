﻿using System;
using System.Windows.Data;

namespace KeyboardEditor.ViewModels
{
    [ValueConversion( typeof( object ), typeof( bool ) )]
    public class HasOrderToBooleanConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return value is IHasOrder;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}
