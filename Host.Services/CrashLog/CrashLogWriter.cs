#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host.Services\CrashLog\CrashLogWriter.cs) is part of CiviKey. 
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
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace CK.AppRecovery
{
    /// <summary>
    /// Exposes a <see cref="TextWriter"/> and safe write methods (as much as they can be).
    /// </summary>
    public class CrashLogWriter : IDisposable
    {
        class Indent : IDisposable
        {
            TextWriter _w;
            string _saved;

            public Indent( TextWriter w )
            {
                _w = w;
                _saved = w.NewLine;
                w.NewLine = w.NewLine + ' ';
            }

            public void Dispose()
            {
                _w.NewLine = _saved;
            }

        }

        /// <summary>
        /// Gets the <see cref="TextWriter"/> to write to.
        /// </summary>
        public TextWriter Writer { get; private set; }

        /// <summary>
        /// Gets whether the <see cref="Writer"/> is available. 
        /// </summary>
        public bool IsValid { get { return Writer != TextWriter.Null; } }

        public void WriteProperty( string name, object o )
        {
            WriteProperty( Writer, name, o );
        }

        public void WriteLineObject( object o )
        {
            WriteObject( Writer, o );
            Writer.WriteLine();
        }

        public void WriteObjectToString( object o, bool isInline )
        {
            WriteLineObjectToString( Writer, o, isInline );
        }

        public void WriteException( Exception e )
        {
            WriteException( e );
        }

        public void Dispose()
        {
            if( Writer != TextWriter.Null )
            {
                Writer.Flush();
                Writer.Dispose();
                Writer = TextWriter.Null;
            }
        }

        internal CrashLogWriter( TextWriter w )
        {
            Writer = w;
        }

        static public bool WriteProperty( TextWriter w, string name, object o )
        {
            w.Write( name ?? "(null name)" );
            w.Write( " = " );
            return WriteObjectToString( w, o, true );
        }

        static public void WriteLineProperty( TextWriter w, string name, object o )
        {
            if( WriteProperty( w, name, o ) ) w.WriteLine();
        }

        static public void WriteLineProperties( TextWriter w, string name, object[] objects )
        {
            w.Write( name ?? "(null name)" );
            w.WriteLine( " = " );
            w.WriteLine( "[" );
            if(objects != null)
                foreach( var o in objects ) WriteLineObjectToString( w, o, true );
            w.WriteLine( "]" );
        }

        static public void WriteObject( TextWriter w, object o )
        {
            if( o != null )
            {
                Exception ex = o as Exception;
                if( ex != null ) WriteException( w, ex );
                else
                {
                    w.WriteLine( "=== Object: Type={0}", o.GetType().AssemblyQualifiedName );
                    WriteLineObjectToString( w, o, false );
                    w.Write( "=== End of Object" );
                }
            }
            else w.Write( "=== Null object" );
        }

        public static void WriteIndented( TextWriter w, string s )
        {
            if( s != null ) w.Write( Regex.Replace( s, @"\r\n?", w.NewLine, RegexOptions.CultureInvariant ) );
        }

        public static void WriteLineIndented( TextWriter w, string s )
        {
            WriteLineIndented( w, s );
            w.WriteLine();
        }

        private static bool WriteObjectToString( TextWriter w, object o, bool isInline )
        {
            try
            {
                w.Write( o != null ? o.ToString() : "(null)" );
                return true;
            }
            catch( Exception ex )
            {
                if( isInline ) w.WriteLine();
                using( new Indent( w ) )
                {
                    w.WriteLine( "=== Exception in object.ToString()" );
                    WriteLineException( w, ex );
                }
                w.WriteLine( "=== End of Exception in object.ToString()" );
                return false;
            }
        }

        private static void WriteLineObjectToString( TextWriter w, object o, bool isInline )
        {
            if( WriteObjectToString( w, o, isInline ) ) w.WriteLine();
        }

        public static void WriteLineException( TextWriter w, object ex )
        {
            WriteException( w, ex );
            w.WriteLine();
        }

        public static void WriteException( TextWriter w, object ex )
        {
            Exception e = ex as Exception;
            if( e == null )
            {
                WriteProperty( w, "Exception", e );
                return;
            }
            using( new Indent( w ) )
            {
                w.WriteLine( "== Exception" );
                WriteLineProperty( w, "ExceptionType", e.GetType().Name );
                WriteLineProperty( w, "Message", e.Message );
                WriteLineProperty( w, "Source", e.Source );
                if( e.TargetSite != null )
                {
                    WriteLineProperty( w, "TargetSite", e.TargetSite.DeclaringType.AssemblyQualifiedName + '.' + e.TargetSite.Name );
                }
                using( new Indent( w ) )
                {
                    w.WriteLine( "== StackTrace" );
                    WriteIndented( w, e.StackTrace );
                }
                w.WriteLine();
                w.WriteLine( "== End of StackTrace" );

                if( e.Data != null && e.Data.Count > 0 )
                {
                    w.Write( "== Exception Data" );
                    using( new Indent( w ) )
                    {
                        foreach( DictionaryEntry data in e.Data )
                        {
                            w.WriteLine();
                            WriteLineProperty( w, "Key", data.Key );
                            WriteProperty( w, "Value", data.Value );
                        }
                    }
                    w.WriteLine();
                    w.WriteLine( "== End of Exception Data" );
                }
                if( e.InnerException != null )
                {
                    using( new Indent( w ) )
                    {
                        w.WriteLine( "== InnerException" );
                        WriteLineException( w, e.InnerException );
                    }
                    w.WriteLine( "== End of InnerException" );
                }
            }
            w.Write( "== End of Exception" );
        }

    }
}
