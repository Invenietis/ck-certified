﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ScreenDivider.Converters
{
    [ValueConversion( typeof( bool ), typeof( Visibility ) )]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool IsReversed { get; set; }
        public bool UseHidden { get; set; }
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            var val = System.Convert.ToBoolean( value, CultureInfo.InvariantCulture );
            if( this.IsReversed )
            {
                val = !val;
            }
            if( !val )
            {
                return Visibility.Visible;
            }
            return this.UseHidden ? Visibility.Hidden : Visibility.Collapsed;
        }
        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}
