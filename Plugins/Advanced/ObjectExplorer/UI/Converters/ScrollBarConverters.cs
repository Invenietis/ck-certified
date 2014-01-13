#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\UI\Converters\ScrollBarConverters.cs) is part of CiviKey. 
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
using System.Windows.Data;

namespace CK.Plugins.ObjectExplorer
{
   public class ScrollbarOnFarLeftConverter : IValueConverter
   {
      public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value == null) return false;
         return ((double)value > 0);
      }

      public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         throw new System.NotImplementedException();
      }
   }

   public class ScrollbarOnFarRightConverter : IMultiValueConverter
   {
      public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (values == null) return false;
         if (values[0] == null || values[1] == null || values[2] == null) return false;
         if (values[0].Equals(double.NaN) || values[1].Equals(double.NaN) || values[2].Equals(double.NaN)) return false;

         double dblHorizontalOffset = 0;
         double dblViewportWidth = 0;
         double dblExtentWidth = 0;

         double.TryParse(values[0].ToString(), out dblHorizontalOffset);
         double.TryParse(values[1].ToString(), out dblViewportWidth);
         double.TryParse(values[2].ToString(), out dblExtentWidth);

         bool fOnFarRight = Math.Round((dblHorizontalOffset + dblViewportWidth), 2) < Math.Round(dblExtentWidth, 2);
         return fOnFarRight;
      }

      public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
      {
         throw new System.NotImplementedException();
      }
   }
}
