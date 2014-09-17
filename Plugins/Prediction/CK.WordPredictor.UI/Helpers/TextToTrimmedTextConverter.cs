using System;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Media;
using CK.WPF.StandardViews;

namespace CK.WordPredictor.UI.Helpers
{
    public class TextToTrimmedTextConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if( values == null ) return null;

            Debug.Assert( values.Length >= 2 );

            Debug.Assert( values[0] is string ); // Text
            Debug.Assert( values[1] is StdKeyView ); // StdKeyView

            FormattedText formatted;

            string text = (string)values[0];
            StdKeyView view = (StdKeyView)values[1];

            return text; // TODO
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
