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
using CK.WPF.Controls;
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
