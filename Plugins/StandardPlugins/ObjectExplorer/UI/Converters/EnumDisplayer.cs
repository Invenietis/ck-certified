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

namespace CK.StandardPlugins.ObjectExplorer
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
