using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CK.Windows;
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
            StdKeyView key = (StdKeyView)values[1];
            TextBlock textblock = key.FindChildren<TextBlock>().FirstOrDefault( tb => tb.Name == "Letter" );
            if( textblock == null ) return text; // TODO
            return GetTrimmedPath( text, textblock, key );
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotImplementedException();
        }

        #endregion

        string GetTrimmedPath( string text, TextBlock textblock, StdKeyView key )
        {
            FormattedText formatted = new FormattedText(
                    text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface( textblock.FontFamily,
                                    textblock.FontStyle,
                                    textblock.FontWeight,
                                    textblock.FontStretch ),
                    textblock.FontSize,
                    textblock.Foreground
                );
            bool widthOK = formatted.Width < key.ActualWidth;

            if( !widthOK )
            {
                string part1;
                string part2;
                SplitStrings( text, out part1, out part2 );


                while( !widthOK )
                {
                    if( part1.Length - 1 < 0 ) break;
                    part1 = part1.Substring( 0, part1.Length - 1 );
                    part2 = part2.Substring( 1, part1.Length );

                    formatted = new FormattedText(
                        String.Format( "{0}…{1}", part1, part2 ),
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface( textblock.FontFamily,
                                        textblock.FontStyle,
                                        textblock.FontWeight,
                                        textblock.FontStretch ),
                        textblock.FontSize,
                        textblock.Foreground
                    );

                    widthOK = formatted.Width < key.ActualWidth && part1.Length != 0;
                }
            }

            return formatted.Text;
        }

        private void SplitStrings( string text, out string part1, out string part2 )
        {
            int length = text.Length / 2;

            part1 = text.Substring( 0, length );
            part2 = text.Substring( text.Length - length, length );
        }
    }
}
