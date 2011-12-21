using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;

namespace CK.Plugins.ObjectExplorer
{
    /// <summary>
    /// Converter that takes as ConverterParameter a string format
    /// and takes as value the variable (binding) to insert into the string format
    /// Used to allow localization with binding
    /// </summary>
    class SimpleStringFormatConverter : IValueConverter
    {
        /// <summary>
        /// "value" should contain the value of the binding (the variable)
        /// "parameter" should contain the string format
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {               
            return String.Format((string)parameter, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
