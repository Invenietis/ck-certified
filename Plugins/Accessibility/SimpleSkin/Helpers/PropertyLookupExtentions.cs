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
