#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\Converters\InstructionToImageConverter.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using CK.Plugins.AutoClick.Model;
using System.IO;

namespace CK.Plugins.AutoClick.Converters
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
