#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host.Services\CrashLog\CrashLogManager.cs) is part of CiviKey. 
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
using CK.AppRecovery;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using CK.Reflection;
//using CK.WPF.Controls;
using Caliburn.Micro;

namespace CK.AppRecovery
{

    static public class CrashLogManager
    {
        public static string CrashLogDirectory { get; set; }

        public static void HandleExistingCrashLogs()
        {
            string crashPath = CrashLogDirectory;
            if( !Directory.Exists( crashPath ) ) return;

            CrashLogWindow w = new CrashLogWindow( new CrashLogWindowViewModel( crashPath ) );
            w.ShowDialog();

            if( Directory.GetFiles( crashPath ).Length == 0 )
            {
                Directory.Delete( crashPath, true );
            }
        }

        static public CrashLogWriter CreateNewCrashLog()
        {
            return CreateNew( "crashLog" );
        }

        static public CrashLogWriter CreateNew( string crashFilePrefix )
        {
            StreamWriter w = null;
            try
            {
                if( !Directory.Exists( CrashLogDirectory ) ) Directory.CreateDirectory( CrashLogDirectory );
                string date = DateTime.UtcNow.ToString( "u" );
                string path = Path.Combine( CrashLogDirectory, String.Format( "{0}-{1}.log", crashFilePrefix, date.Replace( ':', '-' ) ) );
                w = new StreamWriter( path, true, Encoding.UTF8 );
                w.AutoFlush = true;
                CrashLogWriter.WriteLineProperty( w, "UniqueID", Guid.NewGuid().ToString() );
                CrashLogWriter.WriteLineProperty( w, "UtcDate", date );
                return new CrashLogWriter( w );
            }
            catch( Exception )
            {
                try { if( w != null ) w.Dispose(); } catch {}
                return new CrashLogWriter( TextWriter.Null );
            }
        }

    }
}
