using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections;
using System.Windows;
using CK.Plugin;

namespace CK.Plugins.ObjectExplorer
{
    public class ICollectionToVisibilityConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            ICollection col = value as ICollection;
            if( col != null && col.Count > 0 )
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return null;
        }
    }
}
