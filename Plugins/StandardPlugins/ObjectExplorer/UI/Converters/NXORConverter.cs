using System;
using System.Windows.Data;
using System.Globalization;
using System.Diagnostics;

namespace CK.StandardPlugins.ObjectExplorer
{
    [ValueConversion( typeof( bool? ), typeof( bool ) )]
    public class NXORConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            bool param = bool.Parse( parameter.ToString() );

            if( value == null )
            {
                return false;
            }
            else
            {
                return !((bool)value ^ param);
            }
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            bool param = bool.Parse( parameter.ToString() );
            return !((bool)value ^ param);
        }
    }
}
