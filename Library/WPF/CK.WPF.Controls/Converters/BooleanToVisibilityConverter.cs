using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace CK.WPF.Controls
{ 
    [ValueConversion( typeof( bool ), typeof( Visibility ) )]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        struct Parameter
        {
            public bool Invert;
            public Visibility NotVisible;
        }

        /// <summary>
        /// Converts a boolean value to a Visibility value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">
        /// ConverterParameter is of type Visibility
        /// </param><param name="culture"></param>
        /// <returns></returns>
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            Parameter p = ParseParameter( parameter );
            bool isVisible = (bool)value;
            if( p.Invert ) isVisible = !isVisible; 
            return isVisible ? Visibility.Visible : p.NotVisible;
        }


        /// <summary>
        /// Supports 2-way databinding of the BooleanToVisibilityConverter, converting Visibility to a boolean.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            Parameter p = ParseParameter( parameter );
            bool isVisible = ((Visibility)value == Visibility.Visible);
            if( p.Invert ) isVisible = !isVisible; 
            return isVisible;
        }

        /// <summary>   
        /// Parses 'Invert', 'Hidden' or 'Invert,Hidden' into <see cref="Parameter"/>.
        /// </summary>   
        /// <param name="parameter">Parameter string.</param>   
        /// <returns>Parsed structure.</returns>   
        static Parameter ParseParameter( object parameter )
        {
            if( parameter is Parameter ) return (Parameter)parameter;
            switch( (string)parameter )
            {
                case null:
                case "": return new Parameter() { Invert = false, NotVisible = Visibility.Collapsed };
                case "Invert": return new Parameter() { Invert = true, NotVisible = Visibility.Collapsed };
                case "Hidden": return new Parameter() { Invert = false, NotVisible = Visibility.Hidden };
                case "Invert,Hidden": return new Parameter() { Invert = true, NotVisible = Visibility.Hidden };
            }
            throw new FormatException( "Invalid mode specified as the ConverterParameter. It can be empty (defaults to Collapsed), 'Invert', 'Hidden' or 'Invert,Hidden'." );
        }
    }
}
