#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\Logs\Model\ILogExtension.cs) is part of CiviKey. 
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
