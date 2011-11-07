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
    internal struct MarkupTypeReference
    {
        Type _type;
        string _typeName;

        public Type Type { get { return _type; } }

        public string TypeName { get { return _typeName; } }

        public bool IsNull { get { return _type == null && _typeName == null; } }

        public void SetType( string parameterName, Type value )
        {
            if( value == null ) throw new ArgumentNullException( parameterName );
            _type = value;
            _typeName = null;
        }

        public void SetTypeName( string parameterName, string value )
        {
            if( value == null ) throw new ArgumentNullException( parameterName );
            _typeName = value.Replace( '-', '`' );
            _type = null;
        }

        public Type Resolve( IServiceProvider p )
        {
            if( _type == null )
            {
                if( _typeName == null ) throw new InvalidOperationException( "MarkupExtension Type requires Type or TypeName to be defined." );
                IXamlTypeResolver service = p.GetService<IXamlTypeResolver>( true );
                _type = service.Resolve( _typeName );
                if( _type == null ) throw new InvalidOperationException( String.Format( "MarkupExtension invalid TypeName '{0}'.", _typeName ) );
            }
            return _type;
        }

        public static T[] Apply<T>( MarkupTypeReference[] types, int startIndex, Func<MarkupTypeReference, T> f )
        {
            T[] temp = new T[types.Length];
            int nbArgument = 0;
            for( int i = startIndex; i < types.Length; ++i )
            {
                if( !types[i].IsNull ) temp[nbArgument++] = f( types[i] );
            }
            Array.Resize( ref temp, nbArgument );
            return temp;
        }

        public static Type Resolve( MarkupTypeReference[] types, IServiceProvider p )
        {
            Type mainType = types[0].Resolve( p );
            if( mainType.IsGenericTypeDefinition )
            {
                Type[] arguments = Apply( types, 1, x => x.Resolve( p ) );
                return mainType.MakeGenericType( arguments );
            }
            // Check that no arguments are provided.
            for( int i = 1; i < types.Length; ++i )
            {
                if( !types[i].IsNull ) throw new InvalidOperationException( String.Format( "Type '{0}' is not generic: no type argument must be provided.", mainType.FullName ) );
            }
            return mainType;
        }
    }

}
