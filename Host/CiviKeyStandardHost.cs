#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\CiviKeyStandardHost.cs) is part of CiviKey. 
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
using System.IO;
using System.Reflection;
using CK.Context;
using CK.Plugin;
using CK.Storage;
using Host.Services;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using Host.Services.Helper;
using CK.Windows.App;
using System.Collections.Generic;
using CK.Core;
using System.ComponentModel;
using CK.Plugin.Config;
using Common.Logging;

namespace Host
{
    /// <summary>
    /// Singleton host. Its private constructor is safe (no exceptions can be 
    /// thrown except an out of memory: we can safely ignore this pathological case).
    /// </summary>
    public class CivikeyStandardHost : AbstractContextHost, IHostInformation, IHostHelp
    {
        static ILog _log = LogManager.GetLogger( typeof( CivikeyStandardHost ) );
        Guid _guid;
        Version _appVersion;
        bool _firstApplySucceed;
        NotificationManager _notificationMngr;
        CKAppParameters applicationParameters;
        IVersionedUniqueId _fakeUniqueIdForTheHost;

        public event EventHandler<HostHelpEventArgs> ShowHostHelp;

        public void FireShowHostHelp()
        {
            if( _fakeUniqueIdForTheHost == null ) _fakeUniqueIdForTheHost = new SimpleVersionedUniqueId( Guid.Empty, AppVersion );
            if( ShowHostHelp != null )
                ShowHostHelp( this, new HostHelpEventArgs { HostUniqueId = _fakeUniqueIdForTheHost } );
        }

        /// <summary>
        /// Gets a unique identifier for a CiviKey application
        /// Is mainly used to identify an instance of CiviKey in crashlogs
        /// TODO : this hsould be put in the CK-Desktop layer in order to be transmitted directly to the crashlog web server, and stored in files corresponding to this GUID
        /// </summary>
        private Guid ApplicationGUID
        {
            get
            {
                if( _guid == Guid.Empty ) _guid = (Guid)SystemConfig.GetOrSet( "Guid", Guid.NewGuid() );

                return _guid;
            }
        }

        /// <summary>
        /// Gets the current version of the Civikey-Standard application.
        /// It is stored in the system configuration file and updated by the installer.
        /// </summary>
        public Version AppVersion
        {
            get { return _appVersion ?? ( _appVersion = new Version( (string)SystemConfig.GetOrSet( "Version", "2.5" ) ) ); }
        }

        /// <summary>
        /// The SubAppName is the name of the package (Standard, Steria etc...)
        /// using that, we can have different contexts/userconfs for each packages installed on the computer.
        /// </summary>
        private CivikeyStandardHost( CKAppParameters parameters )
        {
            applicationParameters = parameters;
        }

        /// <summary>
        /// Singleton instance.
        /// </summary>
        static readonly public CivikeyStandardHost Instance = new CivikeyStandardHost( CKApp.CurrentParameters );

        public override IContext CreateContext()
        {
            IContext ctx = base.CreateContext();

            _log.Debug( "LAUNCHING" );
            _log.Debug( String.Format( "Launching {0} > Distribution : {1} > Version : {2}, GUID : {3}", CKApp.CurrentParameters.AppName, CKApp.CurrentParameters.DistribName, AppVersion, ApplicationGUID ) );

            _notificationMngr = new NotificationManager();
            
            // Discover available plugins.
            string pluginPath = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "Plugins" );
            if( Directory.Exists( pluginPath ) ) ctx.PluginRunner.Discoverer.Discover( new DirectoryInfo( pluginPath ), true );

            RequirementLayer hostRequirements = new RequirementLayer( "CivikeyStandardHost" );
            hostRequirements.PluginRequirements.AddOrSet( new Guid( "{2ed1562f-2416-45cb-9fc8-eef941e3edbc}" ), RunningRequirement.MustExistAndRun );//KeyboardContext
            hostRequirements.ServiceRequirements.AddOrSet( "CommonServices.Accessbility.IHelpService", RunningRequirement.MustExistAndRun );
            hostRequirements.PluginRequirements.AddOrSet( new Guid( "{0F740086-85AC-46EB-87ED-12A4CA2D12D9}" ), RunningRequirement.MustExistAndRun );//SendInput
            hostRequirements.PluginRequirements.AddOrSet( new Guid( "{B91D6A8D-2294-4BAA-AD31-AC1F296D82C4}" ), RunningRequirement.MustExistAndRun );//Window Executor

            ctx.PluginRunner.Add( hostRequirements )                                                                                                                                                                                                                    ;

            // Load or initialize the ctx.
            LoadResult res = Instance.LoadContext( Assembly.GetExecutingAssembly(), "Host.Resources.Contexts.ContextCiviKey.xml" );
            // Initializes Services.
            {
                ctx.ServiceContainer.Add<IHostInformation>( this );
                ctx.ServiceContainer.Add<IHostHelp>( this );
                // inject specific xaml serializers.
                ctx.ServiceContainer.Add<IStructuredSerializer<Size>>( new XamlSerializer<Size>() );
                ctx.ServiceContainer.Add<IStructuredSerializer<Color>>( new XamlSerializer<Color>() );
                ctx.ServiceContainer.Add<IStructuredSerializer<LinearGradientBrush>>( new XamlSerializer<LinearGradientBrush>() );
                ctx.ServiceContainer.Add<IStructuredSerializer<TextDecorationCollection>>( new XamlSerializer<TextDecorationCollection>() );
                ctx.ServiceContainer.Add<IStructuredSerializer<FontWeight>>( new XamlSerializer<FontWeight>() );
                ctx.ServiceContainer.Add<IStructuredSerializer<FontStyle>>( new XamlSerializer<FontStyle>() );
                ctx.ServiceContainer.Add<IStructuredSerializer<Image>>( new XamlSerializer<Image>() );
                //ctx.ServiceContainer.Add<INotificationService>( _notificationMngr );
            }

            Context.PluginRunner.ApplyDone += new EventHandler<ApplyDoneEventArgs>( OnApplyDone );

            _firstApplySucceed = Context.PluginRunner.Apply();

            ctx.ConfigManager.SystemConfiguration.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler( OnSystemConfigurationPropertyChanged );
            ctx.ConfigManager.UserConfiguration.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler( OnUserConfigurationPropertyChanged );


            return ctx;
        }

        void OnSystemConfigurationPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            //If the user has changed, we need to load the corresponding user configuration
            if( e.PropertyName == "CurrentUserProfile" )
            {

                Uri previousContextAdress = Context.ConfigManager.UserConfiguration.CurrentContextProfile.Address;

                SaveContext();

                SaveUserConfig( Context.ConfigManager.SystemConfiguration.PreviousUserProfile.Address, false );
                Context.ConfigManager.Extended.HostUserConfig.Clear();
                LoadUserConfig( Context.ConfigManager.SystemConfiguration.CurrentUserProfile.Address );

                Context.ConfigManager.Extended.Container.Clear( Context );
                LoadContext( Context.ConfigManager.UserConfiguration.CurrentContextProfile.Address );

                Context.PluginRunner.Apply( true );
            }
        }

        void OnUserConfigurationPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
        }

        private void OnApplyDone( object sender, ApplyDoneEventArgs e )
        {
            //ExecutionPlanResult dosen't exist anymore in the applydoneEventArg, how should we let the user decide what to do ?

            //    if( e.ExecutionPlanResult != null )
            //    {
            //        if(
            //            (e.ExecutionPlanResult.Status == ExecutionPlanResultStatus.SetupError
            //            || e.ExecutionPlanResult.Status == ExecutionPlanResultStatus.StartError) )
            //        {
            //            if( System.Windows.MessageBox.Show( String.Format( R.PluginThrewExceptionAtStart, e.ExecutionPlanResult.Culprit.PublicName ), R.PluginThrewExceptionAtStartTitle, MessageBoxButton.YesNo ) == MessageBoxResult.Yes )
            //            {//if the user wants to try launching CiviKey without the culprit
            //                Context.PluginRunner.Apply();
            //            }
            //            else
            //            {//otherwise, stop CiviKey
            //                Context.RaiseExitApplication( true );
            //            }
            //        }
            //    }
            //    else //e is null means that the plan couldn't be resolved, the system hasn't been changed, but requirements are messy
            //    {
            //        if( _notificationMngr != null )
            //            _notificationMngr.ShowNotification( Guid.Empty, R.ForbiddenActionTitle, R.ForbiddenAction, 5000, NotificationTypes.Warning );
            //        else
            //            System.Windows.MessageBox.Show( R.ForbiddenAction );

            //        //TODO : Revert the configuration (if it is possible ?)
            //    }
        }

        /// <summary>
        /// Fired whenever a an address (and a display name) is required for the context.
        /// </summary>
        public event EventHandler<ContextProfileRequiredEventArgs> ContextAddressRequired;

        #region File paths

        const string _defaultSystemConfigurationFileName = "System.config.ck";
        const string _defaultUserConfigurationFileName = "User.config.ck";

        /// <summary>
        /// Gets the full path of the user configuration file.
        /// Defaults to "User.config.ck" file in <see cref="ApplicationDataPath"/>.
        /// </summary>
        private string DefaultUserConfigPath
        {
            get { return applicationParameters.ApplicationDataPath + _defaultUserConfigurationFileName; }
        }

        /// <summary>
        /// Gets the full path of the machine configuration file.
        /// Defaults to "System.config.ck" file in <see cref="CommonApplicationDataPath"/>.
        /// </summary>
        private string DefaultSystemConfigPath
        {
            get { return applicationParameters.CommonApplicationDataPath + _defaultSystemConfigurationFileName; }
        }

        public override Uri GetSystemConfigAddress()
        {
            return GetDevelopmentPath( _defaultSystemConfigurationFileName ) ?? new Uri( DefaultSystemConfigPath );
        }

        protected override Uri GetDefaultUserConfigAddress( bool saving )
        {
            return GetDevelopmentPath( _defaultUserConfigurationFileName ) ?? new Uri( DefaultUserConfigPath );
        }

        /// <summary>
        /// If there is a civikey.exe.config file and that it contains a IsStandAloneInstance set to true, we set the default config path to ApplicationFolder/Configurations/FileName
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>The development Uri if IsStandAloneInstance is true, null otherwise </returns>
        private Uri GetDevelopmentPath( string fileName )
        {
            string _isStandAloneInstanceString = System.Configuration.ConfigurationManager.AppSettings.Get( "IsStandAloneInstance" );
            bool _isStandAloneInstance = false;
            if( !String.IsNullOrEmpty( _isStandAloneInstanceString ) )
            {
                if( Boolean.TryParse( _isStandAloneInstanceString, out _isStandAloneInstance ) && _isStandAloneInstance )
                {
                    //string dirPath = Path.Combine( Path.Combine( Path.GetDirectoryName( Assembly.GetEntryAssembly().Location ), ".." ), "Configurations" );
                    //if( !Directory.Exists( dirPath ) ) Directory.CreateDirectory( dirPath );
                    string dirPath = System.Configuration.ConfigurationManager.AppSettings.Get( "ConfigurationDirectory" );
                    if( !String.IsNullOrEmpty( dirPath ) && Directory.Exists( dirPath ) )
                    {
                        string filePath = Path.Combine( dirPath, fileName );
                        return new Uri( filePath );
                    }
                }
            }
            return null;
        }

        protected override KeyValuePair<string, Uri> GetDefaultContextProfile( bool saving )
        {
            var e = new ContextProfileRequiredEventArgs( Context, saving )
            {
                Address = GetDevelopmentPath( "Context.xml" ) ?? new Uri( Path.Combine( applicationParameters.ApplicationDataPath, "Context.xml" ) ),
                DisplayName = String.Format( "Context-{0}.xml", DateTime.Now ) // R.NewContextDisplayName
            };
            var h = ContextAddressRequired;
            if( h != null )
            {
                h( this, e );
                if( e.Address == null ) throw new CKException( "" ); //R.ContextAddressRequired 
            }
            return new KeyValuePair<string, Uri>( e.DisplayName, e.Address );
        }

        #endregion

        public string AppName
        {
            get { return applicationParameters.AppName; }
        }

        public string ApplicationDataPath
        {
            get { return applicationParameters.ApplicationDataPath; }
        }

        public string CommonApplicationDataPath
        {
            get { return applicationParameters.CommonApplicationDataPath; }
        }

        public string SubAppName
        {
            get { return applicationParameters.DistribName; }
        }
    }
}
