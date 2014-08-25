#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\CommonLogger.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Collections.Specialized;
using System.IO;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace CK.Windows.App
{
    /// <summary>
    /// Configures the common logger (abstraction provided by the <see cref="Common.Logging.LogManager"/>).
    /// </summary>
    static internal class CommonLogger
    {
        static string _appLogDirectory;

        static Hierarchy _hierarchy;
        static RollingFileAppender _roller;
        static bool _initialized;

        /// <summary>
        /// Initializes <see cref="Common.Logging.LogManager"/> with a <see cref="Common.Logging.Log4Net.Log4NetLoggerFactoryAdapter"/> after
        /// having configured log4net to emit low levels logs.
        /// </summary>
        /// <param name="appLogDirectory">The directory for low leveel logs.</param>
        /// <param name="dumpToConsole">True to log all traces into the console.</param>
        static public void Initialize( string appLogDirectory, bool dumpToConsole )
        {
            string logPattern = "%date [%thread] %-5level %logger - %message%newline";
            _appLogDirectory = appLogDirectory;
            if( !Directory.Exists( appLogDirectory ) ) Directory.CreateDirectory( appLogDirectory );

            _hierarchy = (Hierarchy)log4net.LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = logPattern;
            patternLayout.ActivateOptions();

            if( dumpToConsole )
            {
                ConsoleAppender console = new ConsoleAppender();
                console.Layout = patternLayout;
                console.ActivateOptions();
                console.Threshold = Level.All;
                _hierarchy.Root.AddAppender( console );
            }

            _roller = new RollingFileAppender();
            _roller.Layout = patternLayout;
            _roller.AppendToFile = true;
            _roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            _roller.MaxSizeRollBackups = 4;
            _roller.MaximumFileSize = "100KB";
            _roller.StaticLogFileName = true;
            _roller.File = Path.Combine( _appLogDirectory, "LowLevel.log" );
            _roller.ActivateOptions();
            _hierarchy.Root.AddAppender( _roller );

            _hierarchy.Configured = true;

            NameValueCollection properties = new NameValueCollection();
            properties["configType"] = "EXTERNAL";
            Common.Logging.LogManager.Adapter = new Common.Logging.Log4Net.Log4NetLoggerFactoryAdapter( properties );

            // Registers to inject last application logs into crash logs.
            AppRecoveryManager.ApplicationCrashed += new System.EventHandler<ApplicationCrashedEventArgs>( OnApplicationCrash );

            _initialized = true;
        }


        /// <summary>
        /// Gets the path of the directory that will be used to store the LowLevel.log files.
        /// </summary>
        static public string AppLogDirectory
        {
            get { return _appLogDirectory; }
        }

        /// <summary>
        /// This method is registered on <see cref="CK.AppRecovery.ApplicationRecovery.ApplicationCrashed"/> event.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        static internal void OnApplicationCrash( object source, ApplicationCrashedEventArgs e )
        {
            if( _initialized )
            {
                _hierarchy.Root.Log( Level.Fatal, "Application crashes. Appending existing log files into Crash log. Over!", null );
                _hierarchy.Root.RemoveAppender( _roller );
                _roller.Close();

                if( e.CrashLog.IsValid )
                {
                    char[] buffer = new char[2048];
                    foreach( string f in Directory.GetFiles( _appLogDirectory ) )
                    {
                        e.CrashLog.Writer.WriteLine( "===== Log file: {0} =====", Path.GetFileName( f ) );
                        try
                        {
                            using( StreamReader r = File.OpenText( f ) )
                            {
                                int nbRead;
                                while( (nbRead = r.ReadBlock( buffer, 0, buffer.Length )) > 0 )
                                {
                                    e.CrashLog.Writer.Write( buffer, 0, nbRead );
                                }
                            }
                        }
                        finally
                        {
                            e.CrashLog.Writer.WriteLine( "===== End of log file: {0} =====", Path.GetFileName( f ) );
                        }
                    }
                }
            }
            else
            {
                e.CrashLog.Writer.WriteLine( "CommonLogger has not been initialized." );
            }
        }

    }
}
