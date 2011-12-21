using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Markup;
using System.Security;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using CK.Core;

namespace CK.WPF.Controls
{

    [MarkupExtensionReturnType( typeof( Type ) ), TypeConverter( typeof( TypeExtensionConverter ) )]
    public class TypeExtension : MarkupExtension
    {
        MarkupTypeReference[] _types;

        public TypeExtension()
        {
            _types = new MarkupTypeReference[6];
        }

        public TypeExtension( string typeName )
        {
            _types = new MarkupTypeReference[6];
            _types[0].SetTypeName( "typeName", typeName );
        }

        public TypeExtension( string typeName, Type[] types )
        {
            _types = new MarkupTypeReference[6];
            _types[0].SetTypeName( "typeName", typeName );

            int iUs = 0;
            for( int i = 0; i < types.Length && iUs < _types.Length; ++i )
                if( types[i] != null )
                    _types[iUs++].SetType( "type", types[i] );
        }

        internal MarkupTypeReference[] Types { get { return _types; } }

        public override object ProvideValue( IServiceProvider p )
        {
            return MarkupTypeReference.Resolve( Types, p );
        }

        [ConstructorArgument( "typeName" ), DefaultValue( (string)null )]
        public string TypeName
        {
            get { return _types[0].TypeName; }
            set { _types[0].SetTypeName( "value", value ); }
        }

        #region TypeArgumenXXX/TypeArgumentNameXXX properties definitions.

        public Type TypeArgument0
        {
            get { return _types[1].Type; }
            set { _types[1].SetType( "value", value ); }
        }

        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public string TypeArgumentName0
        {
            get { return _types[1].TypeName; }
            set { _types[1].SetTypeName( "value", value ); }
        }

        public Type TypeArgument1
        {
            get { return _types[2].Type; }
            set { _types[2].SetType( "value", value ); }
        }

        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public string TypeArgumentName1
        {
            get { return _types[2].TypeName; }
            set { _types[2].SetTypeName( "value", value ); }
        }

        public Type TypeArgument2
        {
            get { return _types[3].Type; }
            set { _types[3].SetType( "value", value ); }
        }

        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public string TypeArgumentName2
        {
            get { return _types[3].TypeName; }
            set { _types[3].SetTypeName( "value", value ); }
        }

        public Type TypeArgument3
        {
            get { return _types[4].Type; }
            set { _types[4].SetType( "value", value ); }
        }

        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public string TypeArgumentName3
        {
            get { return _types[4].TypeName; }
            set { _types[4].SetTypeName( "value", value ); }
        }

        public Type TypeArgument4
        {
            get { return _types[5].Type; }
            set { _types[5].SetType( "value", value ); }
        }

        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public string TypeArgumentName4
        {
            get { return _types[5].TypeName; }
            set { _types[5].SetTypeName( "value", value ); }
        }
        #endregion
    }

    internal class TypeExtensionConverter : TypeConverter
    {
        public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType )
        {
            return ((destinationType == typeof( InstanceDescriptor )) || base.CanConvertTo( context, destinationType ));
        }

        [SecurityTreatAsSafe, SecurityCritical]
        public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType )
        {
            if( destinationType != typeof( InstanceDescriptor ) )
            {
                return base.ConvertTo( context, culture, value, destinationType );
            }
            TypeExtension extension = value as TypeExtension;
            if( extension == null )
            {
                throw new ArgumentException( String.Format( "Type must be '{0}'.", typeof( TypeExtension ).FullName ) );
            }
            var types = MarkupTypeReference.Apply( extension.Types, 0, m => m.Type );
            return new InstanceDescriptor( typeof( TypeExtension ).GetConstructor( new Type[] { typeof( Type ), typeof( Type[] ) } ), new object[] { extension.TypeName, types } );
        }
    }

}
