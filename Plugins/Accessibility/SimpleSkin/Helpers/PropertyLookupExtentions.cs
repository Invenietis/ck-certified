#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\Helpers\PropertyLookupExtentions.cs) is part of CiviKey. 
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

using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Interop;
using System;
using System.Collections;
using CK.Keyboard.Model;
using System.Collections.Generic;
using CK.Plugin.Config;

namespace SimpleSkin
{
    public class ValueWrapper<T>
    {
        public T ThisValue { get; internal set; }

        public T Value { get; internal set; }

        //public T DefaultValue { get; internal set; }

        //public bool IsInherited { get { return !ThisValue.Equals( Value ) && !ThisValue.Equals( DefaultValue ); } }
    }

    public static class PropertyLookupExtentions
    {
        #region Property lookup path

        public static IEnumerable<object> GetPropertyLookupPath( this IKeyboardElement obj )
        {
            if( obj is ILayout ) return GetPropertyLookupPath( (ILayout)obj );
            else if( obj is ILayoutZone ) return GetPropertyLookupPath( (ILayoutZone)obj );
            else if( obj is ILayoutKey ) return GetPropertyLookupPath( (ILayoutKey)obj );
            else if( obj is ILayoutKeyMode ) return GetPropertyLookupPath( (ILayoutKeyMode)obj );
            else if( obj is IKeyboard ) return GetPropertyLookupPath( (IKeyboard)obj );
            else if( obj is IZone ) return GetPropertyLookupPath( (IZone)obj );
            else if( obj is IKey ) return GetPropertyLookupPath( (IKey)obj );
            else if( obj is IKeyMode ) return GetPropertyLookupPath( (IKeyMode)obj );

            throw new ArgumentException( "There is no property lookup path available for this type yet", "obj" );
        }

        static IEnumerable<object> GetPropertyLookupPath( ILayoutKeyMode obj )
        {
            return new object[] { obj, obj.LayoutKey, obj.LayoutKey.LayoutZone, obj.Layout };
        }

        static IEnumerable<object> GetPropertyLookupPath( ILayoutKey obj )
        {
            return new object[] { obj, obj.LayoutZone, obj.Layout };
        }

        static IEnumerable<object> GetPropertyLookupPath( ILayoutZone obj )
        {
            return new object[] { obj, obj.Layout };
        }

        static IEnumerable<object> GetPropertyLookupPath( ILayout obj )
        {
            return new object[] { obj };
        }

        static IEnumerable<object> GetPropertyLookupPath( IKeyMode obj )
        {
            return new object[] { obj, obj.Key, obj.Key.Zone, obj.Keyboard };
        }

        static IEnumerable<object> GetPropertyLookupPath( IKey obj )
        {
            return new object[] { obj, obj.Zone, obj.Keyboard };
        }

        static IEnumerable<object> GetPropertyLookupPath( IZone obj )
        {
            return new object[] { obj, obj.Keyboard };
        }

        static IEnumerable<object> GetPropertyLookupPath( IKeyboard obj )
        {
            return new object[] { obj };
        }

        #endregion

        public static T GetPropertyValue<T>( this IKeyboardElement obj, IPluginConfigAccessor config, string property, T defaultValue = default(T) )
        {
            var path = obj.GetPropertyLookupPath();
            foreach( var holder in path )
            {
                if( config[holder].Contains( property ) )
                    return (T)config[holder][property];
            }
            return defaultValue;
        }

        public static ValueWrapper<T> GetWrappedPropertyValue<T>( this IKeyboardElement obj, IPluginConfigAccessor config, string property, T defaultValue = default(T) )
        {
            var path = obj.GetPropertyLookupPath();
            
            var thisvalue = config[obj][property];
            ValueWrapper<T> wrapper = new ValueWrapper<T>() { ThisValue = thisvalue == null ? default( T ) : (T)thisvalue };
            
            foreach( var holder in path )
            {
                if( config[holder].Contains( property ) )
                {
                    wrapper.Value = (T)config[holder][property];
                    return wrapper;
                }
            }

            wrapper.Value = defaultValue;
            return wrapper;
        }
    }
}
