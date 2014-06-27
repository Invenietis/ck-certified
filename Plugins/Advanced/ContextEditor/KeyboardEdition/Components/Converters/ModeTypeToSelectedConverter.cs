#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\Components\Converters\ModeTypeToSelectedConverter.cs) is part of CiviKey. 
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

namespace KeyboardEditor.ViewModels
{
    public class ModeTypeToSelectedConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            ModeTypes type = ModeTypes.Mode;
            ModeTypes param = ModeTypes.Mode;

            if( !Enum.TryParse<ModeTypes>( value.ToString(), out type ) ) throw new ArgumentException( "Passing an object that is not of ModeTypes type into a ModeTypeToSelectedConverter converter value" );
            if( !Enum.TryParse<ModeTypes>( parameter.ToString(), out param ) ) throw new ArgumentException( "Passing an object that is not of ModeTypes type into a ModeTypeToSelectedConverter's converter parameter" );

            return type == param;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            ModeTypes param = ModeTypes.Mode;
            bool boolValue;

            if(!bool.TryParse(value.ToString(), out boolValue)) throw new ArgumentException( "Passing an object that is not of bool type into a ModeTypeToSelectedConverter's converter-back value" );
            if( !Enum.TryParse<ModeTypes>( parameter.ToString(), out param ) ) throw new ArgumentException( "Passing an object that is not of ModeTypes type into a ModeTypeToSelectedConverter's converter parameter" );

            if( param == ModeTypes.Mode )
            {
                if( boolValue ) return ModeTypes.Mode;
                else return ModeTypes.Layout;
            }
            else if( param == ModeTypes.Layout )
            {
                if( boolValue ) return ModeTypes.Layout;
                else return ModeTypes.Mode;
            }

            return Binding.DoNothing;
        }
    }
}