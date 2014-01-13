        #region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Windows.Core\Converter\StringIsNullOrWhiteSpaceToBoolean.cs) is part of CiviKey. 
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
using System.Windows;
using System.Windows.Data;

namespace CK.WPF.Controls
{
    [ValueConversion( typeof( bool ), typeof( Visibility ) )]
    public class StringIsNullOrWhiteSpaceToBoolean : IValueConverter
    {
        /// <summary>
        /// Returns true if the string is null or white space
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            string stringValue = value.ToString();
            return String.IsNullOrWhiteSpace(stringValue);
        }


        /// <summary>
        /// Does not support two-way binding 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return Binding.DoNothing;
        }
    }
}
