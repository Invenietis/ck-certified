using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;

namespace CK.WPF.Controls
{
    public class WidthHeightToOrientationConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        /// <summary>
        /// Converts a width and height value to a Vertical orientation if H > W or to a Horizontal orientation if W >= H.
        /// A non-null parameter reverses this behavior.
        /// </summary>
        /// <param name="values">Object values. Expected: [Width, Height]</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="culture">Unused.</param>
        /// <returns>Orientation result.</returns>
        public object Convert( object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            bool reverse = parameter != null;
            Orientation orientation;

            Debug.Assert( values.Length >= 2, "Array contains at least two items (Width and Height)" );
            Debug.Assert( values[0] is double, "Item 0 (Width) is a double" );
            Debug.Assert( values[1] is double, "Item 1 (Height) is a double" );

            double width = (double)values[0];
            double height = (double)values[1];

            if( height > width )
            {
                orientation = reverse ? Orientation.Horizontal : Orientation.Vertical;
            }
            else
            {
                orientation = reverse ? Orientation.Vertical : Orientation.Horizontal;
            }

            return orientation;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack( object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
