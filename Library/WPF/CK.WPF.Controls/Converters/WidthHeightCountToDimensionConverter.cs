using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace CK.WPF.Controls
{
    public class WidthHeightCountToDimensionConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        /// <summary>
        /// Converts window width, height and count to either a split width or a split height.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert( object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if( values == null ) return null;

            Debug.Assert( values.Length >= 4 );
            Debug.Assert( values[0] is double ); // Width
            Debug.Assert( values[1] is double ); // Height
            Debug.Assert( values[2] is Thickness ); // Margin
            Debug.Assert( values[3] is int ); // Count

            /*
             * If W > H
             * H = Auto
             * W = 1/*
             * Else
             * H = 1/*
             * W = Auto
             * */

            double containerWidth = (double)values[0];
            double containerHeight = (double)values[1];
            Thickness margin = (Thickness)values[2];
            int itemCount = (int)values[3];

            bool outputWidth = parameter == null; // If null: Output is Width, if not null: Output is Height
            bool isHorizontal = containerWidth > containerHeight;

            Double output = Double.NaN;

            if( isHorizontal )
            {
                if( outputWidth )
                {
                    output = (containerWidth / itemCount) - margin.Left - margin.Right;
                }
            }
            else
            {
                if( !outputWidth )
                {
                    output = (containerHeight / itemCount) - margin.Top - margin.Bottom;
                }
                else
                {
                    output = containerWidth - margin.Left - margin.Right;
                }
            }

            return output;
        }

        public object[] ConvertBack( object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
