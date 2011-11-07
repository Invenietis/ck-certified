using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using CK.StandardPlugins.AutoClick.Model;
using System.IO;
using System.Windows.Media.Imaging;

namespace CK.StandardPlugins.AutoClick.Converters
{
    public class PathToImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return new BitmapImage( new Uri( "/AutoClick;component/" + value.ToString(), UriKind.Relative ) );
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return null;
        }

        #endregion
    }
}
