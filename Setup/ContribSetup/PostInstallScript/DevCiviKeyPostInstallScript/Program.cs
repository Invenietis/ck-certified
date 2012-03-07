using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.XPath;
using System.Xml;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Configuration;

namespace DevCiviKeyPostInstallScript
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">
        ///                    0 - IsStandAloneInstance
        ///                    1 - ApplicationDirectory
        ///                    2 - ConfigurationDirectory
        ///                    3 - ApplicationExePath
        /// </param>               
        static void Main( string[] args )
        {
            if( args == null || args.Length != 4 )
            {
                OnError( @"There is not the right number of parameters, there should be 4 of them :
                        0 - IsStandAloneInstance - bool
                        1 - ApplicationDirectory - string
                        2 - ConfigurationDirectory - string
                        3 - ApplicationExePath -string" );
            } 

            Console.Out.WriteLine( "IsStandAloneInstance :" + args[0] );
            Console.Out.WriteLine( "ApplicationDirectory " + args[1] );
            Console.Out.WriteLine( "ConfigurationDirectory " + args[2] );
            Console.Out.WriteLine( "ApplicationExePath " + args[3] );

            bool _isStandAloneInstance;
            if( !bool.TryParse( args[0], out _isStandAloneInstance ) )
            {
                OnError( "Problem while reading IsStandAloneInstance Setting from AppSettings. Value : " + args[0] );
            }

            string applicationDirectory = args[1];
            if( !Directory.Exists( applicationDirectory ) )
            {
                OnError( "The specified application directory is not valid : " + applicationDirectory );
            }

            string configurationDirectory = args[2];
            if( !Directory.Exists( configurationDirectory ) )
            {
                OnError( "The specified configuration directory is not valid : " + configurationDirectory );
            }

            string applicationExePath = args[3];
            if( File.Exists( applicationExePath ) )
            {
                Configuration appConf = ConfigurationManager.OpenExeConfiguration( applicationExePath );
                if( appConf != null )
                {
                    if( appConf.AppSettings.Settings["IsStandAloneInstance"] != null )
                    {
                        appConf.AppSettings.Settings.Remove( "IsStandAloneInstance" );
                    }
                    if( appConf.AppSettings.Settings["ConfigurationDirectory"] != null )
                    {
                        appConf.AppSettings.Settings.Remove( "ConfigurationDirectory" );
                    }
                    
                    appConf.AppSettings.Settings.Add( "IsStandAloneInstance", _isStandAloneInstance.ToString() );
                    if( _isStandAloneInstance )
                    {
                        appConf.AppSettings.Settings.Add( "ConfigurationDirectory", configurationDirectory );
                    }
                    appConf.Save( ConfigurationSaveMode.Full );
                }
                else
                {
                    OnError( "Problem while reading IsStandAloneInstance Setting from AppSettings. ApplicationExePath : " + applicationExePath );
                }
                
            }
        }

        public static void OnError(string message)
        {
            Console.Out.WriteLine( message );
            Console.Out.WriteLine( "The installation is not complete, please try again or contact us at contact@invenietis.com" );
            Console.Out.WriteLine( "Press any key to exit." );
            Console.ReadKey();
            return;
        }
    }
}
