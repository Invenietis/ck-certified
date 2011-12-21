using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace CK.Plugins.ObjectExplorer
{
    public static class ILogExtension
    {

        /// <summary>
        /// Extension method for classes implementing the <see cref="MethodInfo"/> interface, gets the Method's signature, in order to distinguish different overrides
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static string GetSimpleSignature( this MethodInfo m )
        {
            StringBuilder b = new StringBuilder();
            b.Append( m.ReturnType ).Append( ' ' ).Append( m.Name );
            ParameterInfo[] parameters = m.GetParameters();
            if( parameters.Length > 0 )
            {
                b.Append( '(' );
                foreach( var p in m.GetParameters() ) b.Append( p.ParameterType ).Append( ',' );
                b.Length = b.Length - 1;
                b.Append( ')' );
            }
            return b.ToString();
        }

        /// <summary>
        /// Extension method for classes implementing the <see cref="ILogMethodConfig"/> interface, gets the Method's signature, in order to distinguish different overrides
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static string GetSimpleSignature( this ILogMethodConfig m )
        {
            StringBuilder b = new StringBuilder();
            b.Append( m.ReturnType ).Append( ' ' ).Append( m.Name );
            if( m.Parameters.Count > 0 )
            {
                b.Append( '(' );
                foreach( var p in m.Parameters ) b.Append( p.ParameterType ).Append( ',' );
                b.Length = b.Length - 1;
                b.Append( ')' );
            }
            return b.ToString();
        }

        public static string GetCleanSignature( this ILogMethodConfig m)
        {
            StringBuilder b = new StringBuilder();

            b.Append( TypeStringCleaner( m.ReturnType ) ).Append( ' ' ).Append( TypeStringCleaner( m.Name ) ).Append( '(' );
            foreach( var p in m.Parameters ) b.Append( TypeStringCleaner( p.ParameterType ) ).Append( ',' );
            if( m.Parameters.Count > 0 )
                b.Length = b.Length - 1;
            b.Append(')');
            return b.ToString();            
        }

        private static string TypeStringCleaner( string s )
        {
            StringBuilder strBuilder= new StringBuilder();

            s = s.Replace( "[]", "{}" );
            s = s.Replace( "`1", "" );
            s = s.Replace( "[", " " );
            s = s.Replace( "]", "" );
            s = s.Replace( "{}", "[]" );
            string[] splittedString = s.Split( ' ' );
            Debug.Assert( splittedString.Length >= 1 );
            if( splittedString.Length == 1 ) return splittedString[0].Split( '.' ).Last();

            for( int i = 0; i < splittedString.Length; i++ )
            {
                splittedString[i] = splittedString[i].Split( '.' ).Last(); //Removing namespaces
            }

            for( int i = 1; i < splittedString.Length; i++ )
            {
                splittedString[i] = "[" + splittedString[i];
            }

            for( int i = 0; i < splittedString.Length; i++ )
            {
                strBuilder.Append( splittedString[i] );
            }

            for( int i = 0; i < splittedString.Length - 1; i++ )
            {
                strBuilder.Append( "]" );
            }

            return strBuilder.ToString();
        }

    }
}
