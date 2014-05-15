#region LGPL License
/*----------------------------------------------------------------------------
* This file (Setup\StandardSetup\CiviKeyPostInstallScript\CiviKeyPostInstallScript\Program.cs) is part of CiviKey. 
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
using System.Xml.XPath;
using System.Xml;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Configuration;
using System.Security.Principal;

namespace CiviKeyPostInstallScript
{
    class Program
    {
        public static string _distributionName { get; set; }
        static string _appName;

        /// <summary>
        /// Full path of application-specific data repository, for the current roaming user.
        /// Ends with <see cref="Path.DirectorySeparatorChar"/>.
        /// </summary>
        static string _commonAppDataPath;
        public static string CommonApplicationDataPath
        {
            get { return _commonAppDataPath ?? GetFilePath( Environment.SpecialFolder.CommonApplicationData, out _commonAppDataPath ); }
        }

        /// <summary>
        /// Gets the full path of application-specific data repository, for all users.
        /// Ends with <see cref="Path.DirectorySeparatorChar"/>.
        /// The directory is created if it does not exist.
        /// </summary>
        static string _appDataPath;
        public static string ApplicationDataPath
        {
            get { return _appDataPath ?? GetFilePath( Environment.SpecialFolder.ApplicationData, out _appDataPath ); }
        }


        public static void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e )
        {
            OnError( "Unhandled exception.\n\r" + e.ExceptionObject.ToString() );
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">0 - Version
        ///                    1 - AppName
        ///                    2 - DistributionName
        ///                    3 - AupdateServerUrl
        ///                    4 - UpdaterGUID
        ///                    5 - RemoveExistingCtx
        ///                    6 - IsStandAloneInstance
        ///                    7 - ApplicationDirectory
        ///                    8 - ApplicationExePath</param>
        static void Main( string[] args )
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler( CurrentDomain_UnhandledException );

            if( args == null || args.Length != 9 )
                OnError( @"There is not the right number of parameters, there should be 9 of them :
                            0 - Version
                            1 - AppName
                            2 - DistributionName
                            3 - UpdateServerUrl
                            4 - UpdaterGUID
                            5 - RemoveExistingCtx
                            6 - IsStandAloneInstance
                            7 - ApplicationDirectory
                            8 - ApplicationExePath" );

            string systemConfPath;
            string systemConfDir;
            string userConfPath;
            string userConfDir;
            string contextPath;
            string contextDir;

            //Console.Out.WriteLine( "Version :" + args[0] );
            //Console.Out.WriteLine( "AppName :" + args[1] );
            //Console.Out.WriteLine( "DistributionName :" + args[2] );
            //Console.Out.WriteLine( "AupdateServerUrl :" + args[3] );
            //Console.Out.WriteLine( "UpdaterGUID :" + args[4] );
            //Console.Out.WriteLine( "RemoveExistingCtx :" + args[5] );
            //Console.Out.WriteLine( "IsStandAloneInstance :" + args[6] );
            //Console.Out.WriteLine( "standAloneConfigDir :" + args[7] );
            //Console.Out.WriteLine( "ApplicationExePath :" + args[8] );
            //Console.Read();

            string version = args[0];
            _appName = args[1];
            _distributionName = args[2];
            string updateServerUrl = args[3];
            string updaterGuid = args[4];  //"11C83441-6818-4A8B-97A0-1761E1A54251";

            bool removeExistingCtx;
            bool.TryParse( args[5], out removeExistingCtx );
            bool _isStandAloneInstance;
            bool.TryParse( args[6], out _isStandAloneInstance );

            string standAloneConfigDir = args[7];
            if( !Directory.Exists( standAloneConfigDir ) ) return;

            string applicationExePath = args[8];

            if( _isStandAloneInstance )
            {
                systemConfDir = standAloneConfigDir;
                userConfDir = standAloneConfigDir;
                contextDir = standAloneConfigDir;
            }
            else
            {
                systemConfDir = CommonApplicationDataPath;
                userConfDir = ApplicationDataPath;
                contextDir = ApplicationDataPath;
            }

            systemConfPath = Path.Combine( systemConfDir, "System.config.ck" );
            userConfPath = Path.Combine( userConfDir, "User.config.ck" );
            contextPath = Path.Combine( contextDir, "Context.xml" );

            //Console.Out.WriteLine( "systemConfPath :" + systemConfPath );
            //Console.Out.WriteLine( "userConfPath :" + userConfPath );
            //Console.Out.WriteLine( "contextPath :" + contextPath );

            string hostGuid = "1A2DC25C-E357-488A-B2B2-CD2D7E029856";
            string updateDoneDir = Path.Combine( CommonApplicationDataPath, "Updates" ); //AppNam//DistribName//Updates//UpdateDone
            string updateDone = Path.Combine( updateDoneDir, "UpdateDone" );
            Directory.CreateDirectory( updateDoneDir );
            File.Create( updateDone );

            //Giving read/write/execute rights to any user on UpdateDone
            FileSecurity f = File.GetAccessControl( updateDone );
            var sid = new SecurityIdentifier( WellKnownSidType.BuiltinUsersSid, null );
            NTAccount account = (NTAccount)sid.Translate( typeof( NTAccount ) );
            f.AddAccessRule( new FileSystemAccessRule( account, FileSystemRights.Modify, AccessControlType.Allow ) );
            File.SetAccessControl( updateDone, f );


            if( File.Exists( systemConfPath ) )
            {
                Version previousVersion = null;
                XmlDocument xPathDoc = new XmlDocument();
                xPathDoc.Load( systemConfPath );
                XPathNavigator xPathNav = xPathDoc.CreateNavigator();

                //Console.Read();

                string xPathExp = "//p[@guid='" + hostGuid.ToLower() + "']/data[@key='Version']";
                XPathNodeIterator iterator = xPathNav.Select( xPathNav.Compile( xPathExp ) );
                if( iterator.Count == 1 )
                {
                    iterator.MoveNext();
                    string retrievedVersion = iterator.Current.Value;
                    if( !String.IsNullOrWhiteSpace( retrievedVersion ) )
                    {
                        Version.TryParse( retrievedVersion, out previousVersion );
                    }
                }

                AddEntryToSystemConf( "UpdateServerUrl", updateServerUrl, updaterGuid, "Update Checker", xPathNav );
                AddEntryToSystemConf( "DistributionName", _distributionName, updaterGuid, "Update Checker", xPathNav );
                AddEntryToSystemConf( "Version", version, hostGuid, "Host", xPathNav );

                if( previousVersion != null )
                {
                    Console.Out.WriteLine( "previous version is : " + previousVersion.ToString() );

                    if( previousVersion <= new Version( 2, 7, 0 ) )
                    {
                        Console.Out.WriteLine( "Migration 0.0.0 -> 2.7.0 may be necessary" );
                        Version currentVersion = null;
                        if( Version.TryParse( version, out currentVersion ) )
                        {
                            Console.Out.WriteLine( "current version is : " + currentVersion.ToString() );
                            if( currentVersion >= new Version( 2, 7, 0 ) )
                            {
                                Console.Out.WriteLine( "0.0.0 -> 2.7.0 necessary, starting migration..." );
                                Upgrade26xTov270( xPathNav );
                                Console.Out.WriteLine( "0.0.0 -> 2.7.0 migration done !" );
                            }
                        }
                    }
                    else
                    {
                        Console.Out.WriteLine( "Migration 0.0.0 > 2.7.0 is not necessary" );
                    }
                }


                xPathDoc.Save( systemConfPath );

                Console.Out.WriteLine( "Checking if .NET 3.5 -> .NET 4.0 migration is necessary" );
                UpgradeUser35To40( xPathNav );

                if( removeExistingCtx )
                {
                    if( File.Exists( contextPath ) )
                    {
                        string renamedFilePath = Path.Combine( contextDir, "Context.RenamedBefore_" + version + ".xml" );
                        if( !File.Exists( renamedFilePath ) ) //in case this version has already been installed, don't overwrite the saved context
                        {
                            File.Move( contextPath, renamedFilePath );
                        }
                    }
                }

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
                        Debug.Assert( systemConfDir == userConfDir && systemConfDir == contextDir );
                        appConf.AppSettings.Settings.Add( "ConfigurationDirectory", systemConfDir );
                    }
                }
                appConf.Save( ConfigurationSaveMode.Full );
            }
            else
            {
                OnError( "Problem while reading the system configuration. Path : " + systemConfPath );
            }

            Console.Out.WriteLine( "Installation complete." );
#if DEBUG
            Console.Read();
#endif
        }

        /// <summary>
        /// Upgrades the user configuration (from .NET 3.5 to .NET 4.0 and its breaking change - we now need a ContextProfileCollection in the UserConf)
        /// If the userConf, is in a 4.0 version, this method won't do anything
        /// It returns the contextpath found via the userconf, in case the context has been moved or renamed.
        /// Return String.Empty if a contextpath can't be found.
        /// </summary>
        /// <param name="xPathSystemNav"></param>
        /// <returns></returns>
        private static string UpgradeUser35To40( XPathNavigator xPathSystemNav )
        {
            string xPathExp = "//UserProfile";
            XmlDocument xPathDoc;
            XPathNavigator xPathUserNav;

            string userConfPath = String.Empty;
            string contextPath = String.Empty;

            XPathNodeIterator iterator = xPathSystemNav.Select( xPathSystemNav.Compile( xPathExp ) );
            if( iterator.Count > 0 ) //if there is a path to a userConf
            {
                iterator.MoveNext();

                if( iterator.Count > 1 ) //if there are more than one userProfile, find the last one.
                {
                    while( iterator.Current.GetAttribute( "IsLast", "" ) != "True" ) iterator.MoveNext();
                }

                userConfPath = iterator.Current.GetAttribute( "Address", "" );
                if( !String.IsNullOrEmpty( userConfPath ) && File.Exists( userConfPath ) ) //and it exists
                {
                    Console.Out.WriteLine( "Migrating from .NET 3.5 version to .NET 4.0 version..." );
                    xPathDoc = new XmlDocument();
                    xPathDoc.Load( userConfPath );
                    xPathUserNav = xPathDoc.CreateNavigator();
                    xPathExp = "//data[@key='LastContextPath']";
                    XPathNodeIterator userConfIterator = xPathUserNav.Select( xPathUserNav.Compile( xPathExp ) );
                    if( userConfIterator.Count == 1 ) // and we can find a LastContextPath ( -> this is a v3.5 userConf)
                    {
                        userConfIterator.MoveNext();

                        string contextPathTemp = userConfIterator.Current.Value;
                        Uri uri = new Uri( contextPathTemp );

                        if( !String.IsNullOrEmpty( contextPathTemp ) && File.Exists( contextPathTemp ) ) //and that lastContextPath is reachable
                        {
                            contextPath = contextPathTemp; //Return the contextPath.
                            //Remove the LastContextPath tag 
                            userConfIterator.Current.DeleteSelf();

                            //and add a ContextProfileCollection ( -> upgrade to v4)
                            xPathUserNav = xPathDoc.CreateNavigator();
                            xPathExp = "//PluginStatusCollection";
                            userConfIterator = xPathUserNav.Select( xPathUserNav.Compile( xPathExp ) );

                            userConfIterator.MoveNext();

                            userConfIterator.Current.InsertElementAfter( "", "ContextProfileCollection", "", "" );
                            userConfIterator.Current.MoveToFollowing( "ContextProfileCollection", "" );
                            userConfIterator.Current.AppendChildElement( "", "ContextProfile", "", "" );
                            userConfIterator.Current.MoveToChild( "ContextProfile", "" );
                            userConfIterator.Current.CreateAttribute( "", "DisplayName", "", "Context" );
                            userConfIterator.Current.CreateAttribute( "", "Uri", "", @"file://" + contextPath.Replace( "\\", "/" ) );

                            xPathDoc.Save( userConfPath );
                            Console.Out.WriteLine( "Migration from .NET 3.5 version to .NET 4.0 version done" );
                        }
                    }
                    else
                    {
                        Console.Out.WriteLine( "Migration from .NET 3.5 version to .NET 4.0 version isn't necessary" );
                    }
                }
                else
                {
                    Console.Out.WriteLine( "Migration from .NET 3.5 version to .NET 4.0 version isn't necessary" );
                }
            }
            return contextPath;
        }

        public static void OnError( string message )
        {
            Console.Out.WriteLine( message );
            Console.Out.WriteLine( "" );
            Console.Out.WriteLine( "The installation is not complete, please try again or contact us at contact@invenietis.com" );
            Console.Out.WriteLine( "Press any key to exit." );
            Console.ReadKey();
            return;
        }

        public static void Upgrade26xTov270( XPathNavigator xPathNav )
        {
            //Using the new updating API : http://api.civikey.invenietis.com
            AddEntryToSystemConf( "UpdateServerUrl", "http://api.civikey.invenietis.com/", "11C83441-6818-4A8B-97A0-1761E1A54251", "Update Checker", xPathNav );

            //ADD : AutoBinder
            //<PluginStatus Guid="b63bb144-1c13-4a3b-93bd-ac5233f4f18e" Status="AutomaticStart"></PluginStatus>
            string xPathExp = "//System/PluginStatusCollection";
            XPathNodeIterator iterator = xPathNav.Select( xPathNav.Compile( xPathExp ) );

            iterator.MoveNext();

            iterator.Current.PrependChildElement( String.Empty, "PluginStatus", String.Empty, String.Empty );

            iterator.Current.MoveToFirstChild();

            //we are on the first PluginStatus, which we have just created
            iterator.Current.CreateAttribute( String.Empty, "Guid", String.Empty, "b63bb144-1c13-4a3b-93bd-ac5233f4f18e" );
            iterator.Current.CreateAttribute( String.Empty, "Status", String.Empty, "AutomaticStart" );


            //ADD : 
            //<Plugins>
            //  <p guid="f6b5d818-3c04-4a46-ad65-afc5458a394c" version="1.0.0" name="CK.WindowManager.WindowElementBinder">
            //    <data key="SerializableBindings" type="Structured" typeName="CK.WindowManager.WindowElementBinder+SerializableBindings, CK.WindowManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null">
            //      <Bindings>
            //        <Bind Master="Azerty" Slave="Prediction" Position="Top" />
            //        <Bind Master="Ergonomique" Slave="Prediction" Position="Top" />
            //        <Bind Master="ClickSelector" Slave="AutoClick" Position="Top" />
            //      </Bindings>
            //    </data>
            //  </p>
            //</Plugins>

            xPathExp = "//System/Plugins";
            iterator = xPathNav.Select( xPathNav.Compile( xPathExp ) );

            iterator.MoveNext();

            iterator.Current.PrependChildElement( String.Empty, "p", String.Empty, String.Empty );

            iterator.Current.MoveToFirstChild();
            //We are on the <p> element
            iterator.Current.CreateAttribute( null, "guid", null, "f6b5d818-3c04-4a46-ad65-afc5458a394c" );
            iterator.Current.CreateAttribute( null, "version", null, "1.0.0" );
            iterator.Current.CreateAttribute( null, "name", null, "CK.WindowManager.WindowElementBinder" );

            iterator.Current.PrependChildElement( String.Empty, "data", String.Empty, String.Empty );
            iterator.Current.MoveToFirstChild();
            //We are on the <data> element
            iterator.Current.CreateAttribute( null, "key", null, "SerializableBindings" );
            iterator.Current.CreateAttribute( null, "type", null, "Structured" );
            iterator.Current.CreateAttribute( null, "typeName", null, "CK.WindowManager.WindowElementBinder+SerializableBindings, CK.WindowManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" );

            iterator.Current.PrependChildElement( String.Empty, "Bindings", String.Empty, String.Empty );
            iterator.Current.MoveToFirstChild();
            //We are on the <Bindings> element

            iterator.Current.PrependChildElement( String.Empty, "Bind", String.Empty, String.Empty );
            iterator.Current.MoveToFirstChild();
            //We are on the <Bind> element
            iterator.Current.CreateAttribute( null, "Master", null, "Azerty" );
            iterator.Current.CreateAttribute( null, "Slave", null, "Prediction" );
            iterator.Current.CreateAttribute( null, "Position", null, "Top" );

            iterator.Current.InsertElementAfter( String.Empty, "Bind", String.Empty, String.Empty );
            iterator.Current.MoveToFollowing( "Bind", String.Empty );
            iterator.Current.CreateAttribute( null, "Master", null, "Ergonomique" );
            iterator.Current.CreateAttribute( null, "Slave", null, "Prediction" );
            iterator.Current.CreateAttribute( null, "Position", null, "Top" );

            iterator.Current.InsertElementAfter( String.Empty, "Bind", String.Empty, String.Empty );
            iterator.Current.MoveToFollowing( "Bind", String.Empty );
            iterator.Current.CreateAttribute( null, "Master", null, "ClickSelector" );
            iterator.Current.CreateAttribute( null, "Slave", null, "AutoClick" );
            iterator.Current.CreateAttribute( null, "Position", null, "Top" );
        }

        private static void AddEntryToSystemConf( string key, string value, string pluginId, string pluginName, XPathNavigator xPathNav )
        {
            bool entrySet = false;

            string xPathExp = "//p[@guid='" + pluginId.ToUpper() + "']";
            XPathNodeIterator iterator = xPathNav.Select( xPathNav.Compile( xPathExp ) );
            if( iterator.Count == 1 )
            {
                entrySet = DoUpdateEntryToSystemConf( key, value, entrySet, iterator );
            }//At the end of this if statement, we are either on the right data element, or a the end of the pluginsdata
            else
            {
                xPathExp = "//p[@guid='" + pluginId.ToLower() + "']";
                iterator = xPathNav.Select( xPathNav.Compile( xPathExp ) );
                if( iterator.Count == 1 )
                {
                    entrySet = DoUpdateEntryToSystemConf( key, value, entrySet, iterator );
                }
                else //if the updater entry does not exist, create it
                {
                    CreateEntryToSystemConf( key, value, pluginId, pluginName, xPathNav, ref xPathExp, ref iterator );
                }
            }
        }

        private static void CreateEntryToSystemConf( string key, string value, string pluginId, string pluginName, XPathNavigator xPathNav, ref string xPathExp, ref XPathNodeIterator iterator )
        {
            xPathExp = "//System/Plugins";
            iterator = xPathNav.Select( xPathNav.Compile( xPathExp ) );

            if( iterator.Count == 0 ) //if there is no pluginsData child
            {
                xPathExp = "//System";
                iterator = xPathNav.Select( xPathNav.Compile( xPathExp ) );
                iterator.MoveNext();

                iterator.Current.AppendChildElement( null, "Plugins", null, null );
                iterator.Current.MoveToFirstChild();

                while( iterator.Current.Name != "Plugins" ) iterator.Current.MoveToNext();
            }
            else
            {
                iterator.MoveNext();
            }

            bool pFound = false;
            //Adding the <p>
            if( iterator.Current.MoveToFirstChild() )
            {
                //at least one "<p>" exists
                do
                {
                    if( iterator.Current.GetAttribute( "guid", String.Empty ) == pluginId )
                    {
                        //if the <p> corresponds to the updater data
                        pFound = true;
                        break;
                        //We are on the right <p>
                    }
                } while( iterator.Current.MoveToNext() );

                if( !pFound )
                {
                    iterator.Current.InsertElementAfter( null, "p", null, null );
                    iterator.Current.MoveToNext();
                    iterator.Current.CreateAttribute( null, "guid", null, pluginId );
                    iterator.Current.CreateAttribute( null, "version", null, "1.0.0" );
                    iterator.Current.CreateAttribute( null, "name", null, pluginName );
                }
            }
            else
            {
                //There is nothing in PluginsData, we add the updater's <p>

                iterator.Current.AppendChildElement( null, "p", null, null );
                iterator.Current.MoveToFirstChild();
                iterator.Current.CreateAttribute( null, "guid", null, pluginId );
                iterator.Current.CreateAttribute( null, "version", null, "1.0.0" );
                iterator.Current.CreateAttribute( null, "name", null, pluginName );
                //We are on the right <p>
            }

            //Last step : adding the <data>
            if( iterator.Current.MoveToFirstChild() )
            {
                InsertUpdateElementAfter( iterator, value, key );
            }
            else
            {
                //There are no children yet
                AppendChildUpdateElement( iterator, value, key );
            }
        }

        private static bool DoUpdateEntryToSystemConf( string key, string value, bool entrySet, XPathNodeIterator iterator )
        {
            iterator.MoveNext();

            if( !iterator.Current.MoveToFirstChild() )
            {
                InsertUpdateElementAfter( iterator, value, key );
            }
            else
            {
                do
                {
                    if( iterator.Current.GetAttribute( "key", String.Empty ) == key )
                    {
                        iterator.Current.SetValue( value );
                        entrySet = true;
                        break;
                    }
                } while( iterator.Current.MoveToNext() );

                if( !entrySet )
                {
                    InsertUpdateElementAfter( iterator, value, key );
                }
            }
            return entrySet;
        }

        static void InsertUpdateElementAfter( XPathNodeIterator iterator, string value, string key )
        {
            iterator.Current.InsertElementAfter( null, "data", null, value );
            iterator.Current.MoveToNext();
            iterator.Current.CreateAttribute( null, "key", null, key );
            iterator.Current.CreateAttribute( null, "type", null, "String" );
        }

        static void AppendChildUpdateElement( XPathNodeIterator iterator, string value, string key )
        {
            iterator.Current.AppendChildElement( null, "data", null, value );
            iterator.Current.MoveToFirstChild();
            iterator.Current.CreateAttribute( null, "key", null, key );
            iterator.Current.CreateAttribute( null, "type", null, "String" );
        }

        static string GetFilePath( Environment.SpecialFolder specialFolder, out string savePath )
        {
            string p = Environment.GetFolderPath( specialFolder )
                    + Path.DirectorySeparatorChar
                    + _appName
                    + Path.DirectorySeparatorChar
                    + _distributionName
                    + Path.DirectorySeparatorChar;

            if( !Directory.Exists( p ) ) Directory.CreateDirectory( p );
            savePath = p;
            return p;
        }
    }
}
