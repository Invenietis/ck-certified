#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\UI\Converters\RequirementToResourceConverter.cs) is part of CiviKey. 
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
using System.Globalization;
using CK.Plugin;
using CK.StandardPlugins.ObjectExplorer.UI.Resources;

namespace CK.StandardPlugins.ObjectExplorer
{    
    public class RequirementToResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RunningRequirement requirement = (RunningRequirement)value;
            switch( requirement )
            {
                case RunningRequirement.MustExist:
                    return R.MustExist;
                case RunningRequirement.MustExistAndRun:
                    return R.MustExistAndRun;
                case RunningRequirement.MustExistTryStart:
                    return R.MustExistTryStart;
                case RunningRequirement.Optional:
                    return R.Optional;
                case RunningRequirement.OptionalTryStart:
                    return R.OptionalTryStart;
                default:
                    return "Unknow Requirement";                    
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool param = bool.Parse(parameter.ToString());
            return !((bool)value ^ param);
        }
    }
}
