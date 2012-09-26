#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\UI\Converters\EnumDisplayer.cs) is part of CiviKey. 
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
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Markup;
using System.Reflection;

namespace CK.Plugins.ObjectExplorer
{
    [ContentProperty( "DisplayEntries" )]
    public class EnumDisplayer : IValueConverter
    {
        Type _type;
        IDictionary _displayValues;
        IDictionary _reverseValues;
        List<EnumDisplayEntry> _displayEntries;

        public Type Type
        {
            get { return _type; }
            set
            {
                if( !value.IsEnum )
                    throw new ArgumentException( "parameter is not an Enumermated type", "value" );
                _type = value;
            }
        }

        public IEnumerable<string> DisplayNames
        {
            get
            {
                ComputeDisplayValues();
                return _displayValues.Values as IEnumerable<string>;
            }
        }

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            ComputeDisplayValues();
            return _displayValues[value];
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            ComputeDisplayValues();
            return _reverseValues[value];
        }

        public List<EnumDisplayEntry> DisplayEntries
        {
            get
            {
                if( _displayEntries == null )
                    _displayEntries = new List<EnumDisplayEntry>();
                return _displayEntries;
            }
        }

        void ComputeDisplayValues()
        {
            if( _displayValues == null )
            {
                Type displayValuesType = typeof( Dictionary<,> )
                                            .GetGenericTypeDefinition().MakeGenericType( typeof( string ), _type );
                this._reverseValues = (IDictionary)Activator.CreateInstance( displayValuesType );

                this._displayValues =
                   (IDictionary)Activator.CreateInstance( typeof( Dictionary<,> )
                            .GetGenericTypeDefinition()
                            .MakeGenericType( _type, typeof( string ) ) );

                var fields = _type.GetFields( BindingFlags.Public | BindingFlags.Static );
                foreach( var field in fields )
                {
                    object enumValue = field.GetValue( null );
                    string displayString = GetDisplayStringValue( enumValue );

                    if( displayString != null )
                    {
                        _displayValues.Add( enumValue, displayString );
                        _reverseValues.Add( displayString, enumValue );
                    }
                }
            }
        }

        string GetDisplayStringValue( object enumValue )
        {
            if( _displayEntries != null && _displayEntries.Count > 0 )
            {
                EnumDisplayEntry foundEntry = _displayEntries.Find( delegate( EnumDisplayEntry entry )
                {
                    object e = Enum.Parse( _type, entry.EnumValue );
                    return enumValue.Equals( e );
                } );
                if( foundEntry != null )
                {
                    if( foundEntry.ExcludeFromDisplay ) return null;
                    return foundEntry.DisplayString;

                }
            }
            return Enum.GetName( _type, enumValue );
        }
    }

    public class EnumDisplayEntry
    {
        public string EnumValue { get; set; }
        public string DisplayString { get; set; }
        public bool ExcludeFromDisplay { get; set; }
    }
}
