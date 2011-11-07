using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using CK.StandardPlugins.AutoClick.Model;
using System.IO;

namespace CK.StandardPlugins.AutoClick.Converters
{
    public class InstructionToImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            ClickInstruction inst = (ClickInstruction)Enum.Parse( typeof(ClickInstruction), value.ToString() );
            string path;
            switch( inst )
            {
                case ClickInstruction.RightButtonDown:
                    path =  "../Res/Images/RightDown.png";
                    break;
                case ClickInstruction.RightButtonUp:
                    path =  "../Res/Images/RightUp.png";
                    break;
                case ClickInstruction.LeftButtonDown:
                    path =  "../Res/Images/LeftDown.png";
                    break;
                case ClickInstruction.LeftButtonUp:
                    path =  "../Res/Images/LeftUp.png";
                    break;
                case ClickInstruction.WheelDown:
                    path =  "../Res/Images/WheelDown.png";
                    break;
                case ClickInstruction.WheelUp:
                    path =  "../Res/Images/WheelUp.png";
                    break;
                default:
                    path =  "";
                    break;
            }

            return path;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return null;
        }

        #endregion
    }
}
