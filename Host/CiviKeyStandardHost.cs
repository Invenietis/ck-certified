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
* Copyright � 2007-2012, 
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
using CK.Context.SemVer;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using CommonServices;
using CK.Plugin.Config;
using System.Threading;
using System.Globalization;
using CK.Monitoring;

namespace Host
{
    /// <summary>
    /// Singleton host. Its private constructor is safe (no exceptions can be 
    /// thrown except an out of memory: we can safely ignore this pathological case).
    /// </summary>
    public class CivikeyStandardHost : AbstractContextHost, IHostInformation, IHostHelp, IContextSaver
    {
        IActivityMonitor _log;
        SemanticVersion20 _appVersion;
        bool _firstApplySucceed;
        NotificationManager _notificationMngr;
        readonly CKAppParameters applicationParameters;

        /// <summary>
        /// The SubAppName is the name of the package (Standard, Steria etc...)
        /// using that, we can have different contexts/userconfs for each packages installed on the computer.
        /// </summary>
        private CivikeyStandardHost( CKAppParameters parameters )
        {
            applicationParameters = parameters;
            
            ApplicationUniqueId = new SimpleUniqueId( App.ApplicationId );
        }

        /// <summary>
        /// Singleton instance.
        /// </summary>
        static readonly public CivikeyStandardHost Instance = new CivikeyStandardHost( CKApp.CurrentParameters );

        /// <summary>
        /// Gets a unique identifier for a CiviKey application
        /// Is mainly used to identify an instance of CiviKey in crashlogs
        /// </summary>
        public IUniqueId ApplicationUniqueId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current version of the Civikey-Standard application.
        /// It is stored in the system configuration file and updated by the installer.
        /// </summary>
        public SemanticVersion20 AppVersion
        {
            get { return _appVersion ?? (_appVersion = SemanticVersion20.Parse( (string)SystemConfig.GetOrSet( "Version", "2.7" ) )); }
        }

        public override IContext CreateContext()
        {
            var goPath = Path.Combine( CKApp.CurrentParameters.ApplicationDataPath, @"AppLogs\" );
            var logPath = Path.Combine( goPath, @"GrandOutputDefault\" );
            var goConfigPath = Path.Combine( goPath, "GrandOutput.config" );
            if( !File.Exists( goConfigPath ) )
            {
                File.WriteAllText( goConfigPath, string.Format( @"<GrandOutputConfiguration>
    <Channel MinimalFilter=""Debug"">
        <Add Type=""BinaryFile"" Name=""All"" Path=""{0}"" />
    </Channel>
</GrandOutputConfiguration>", logPath ) );
            }

            CK.Core.SystemActivityMonitor.RootLogPath = Path.Combine( CKApp.CurrentParameters.ApplicationDataPath, @"AppLogs\" ); ;
            CK.Monitoring.GrandOutput.EnsureActiveDefaultWithDefaultSettings();


            //WARNING : DO NOT get information from the system configuration or the user configuration before discovering.
            //Getting info from these conf will trigger the LoadSystemConf or LoadUserConf, which will parse configurations set in the corresponding files.
            //If a system conf is found and loaded at this point, plugin will be set as disabled (because the plugins are not yet discovered). If there is a userconf, the requirements will be parsed again later, and everything will work fine.
            //The problem occurs when there is no user conf. (this happens when CiviKey is launched for the first time)

            Monitor = new ActivityMonitor( "CiviKey" );
            IContext ctx = base.CreateContext();

            _log = Monitor;

            using( _log.OpenInfo().Send( "LAUNCHING" ) )
            {
                _notificationMngr = new NotificationManager();

                // Discover available plugins.
                string pluginPath = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "Plugins" );
                _log.Info().Send( "Discovering plugins..." );
                if( Directory.Exists( pluginPath ) ) ctx.PluginRunner.Discoverer.Discover( new DirectoryInfo( pluginPath ), true );
                _log.Info().Send( "Plugins discovered" );
                _log.Info().Send( String.Format( "Launching {0} > Distribution : {1} > Version : {2}, GUID : {3}", CKApp.CurrentParameters.AppName, CKApp.CurrentParameters.DistribName, AppVersion, ApplicationUniqueId.UniqueId ) );

                var hostRequirements = new RequirementLayer( "CivikeyStandardHost" );
                hostRequirements.PluginRequirements.AddOrSet( new Guid( "{2ed1562f-2416-45cb-9fc8-eef941e3edbc}" ), RunningRequirement.MustExistAndRun );//KeyboardContext
                hostRequirements.PluginRequirements.AddOrSet( new Guid( "{0F740086-85AC-46EB-87ED-12A4CA2D12D9}" ), RunningRequirement.MustExistAndRun );//SendInput
                hostRequirements.PluginRequirements.AddOrSet( new Guid( "{B91D6A8D-2294-4BAA-AD31-AC1F296D82C4}" ), RunningRequirement.MustExistAndRun );//Window Executor
                hostRequirements.PluginRequirements.AddOrSet( new Guid( "{D173E013-2491-4491-BF3E-CA2F8552B5EB}" ), RunningRequirement.MustExistAndRun );//KeyboardDisplayer

                hostRequirements.PluginRequirements.AddOrSet( new Guid( "{55A95F2F-2D67-4AE1-B5CF-4880337F739F}" ), RunningRequirement.MustExistAndRun );//WindowSaver
                hostRequirements.PluginRequirements.AddOrSet( new Guid( "{3F8140F5-AD63-4EF4-AB6C-A9A7EE18078A}" ), RunningRequirement.MustExistAndRun );//WindowStateManager

                //Command handlers. These plugins register their protocols onto the keyboard editor.
                //Therefor, we need them started in order to be able to create any type of Key Command.
                hostRequirements.PluginRequirements.AddOrSet( new Guid( "{664AF22C-8C0A-4112-B6AD-FB03CDDF1603}" ), RunningRequirement.MustExistAndRun );//FileLauncherCommandHandler
                hostRequirements.PluginRequirements.AddOrSet( new Guid( "{418F670B-46E8-4BE2-AF37-95F43040EEA6}" ), RunningRequirement.MustExistAndRun );//KeySequenceCommandHandler
                hostRequirements.PluginRequirements.AddOrSet( new Guid( "{78D84978-7A59-4211-BE04-DD25B5E2FDC1}" ), RunningRequirement.MustExistAndRun );//TextTemplateCommandHandler
                hostRequirements.PluginRequirements.AddOrSet( new Guid( "{4EDBED5A-C38E-4A94-AD34-18720B09F3B7}" ), RunningRequirement.MustExistAndRun );//ClicCommandHandler
                hostRequirements.PluginRequirements.AddOrSet( new Guid( "{B2EC4D13-7A4F-4F9E-A713-D5F8DDD161EF}" ), RunningRequirement.MustExistAndRun );//MoveMouseCommandHandler

                // ToDoJL
                //hostRequirements.PluginRequirements.AddOrSet( new Guid( "{DC7F6FC8-EA12-4FDF-8239-03B0B64C4EDE}" ), RunningRequirement.MustExistAndRun );//HelpUpdater
                hostRequirements.ServiceRequirements.AddOrSet( "Help.Services.IHelpViewerService", RunningRequirement.MustExistAndRun );
            //hostRequirements.ServiceRequirements.AddOrSet( "Help.Services.IHelpUpdaterService", RunningRequirement.MustExistAndRun );

                ctx.PluginRunner.Add( hostRequirements );

                // Load or initialize the ctx.
                LoadResult res = Instance.LoadContext( Assembly.GetExecutingAssembly(), "Host.Resources.Contexts.ContextCiviKey.xml" );
                _log.Info().Send( "Context loaded successfully." );

                // Initializes Services.
                {
                    ctx.ServiceContainer.Add<IHostInformation>( this );
                    ctx.ServiceContainer.Add<IHostHelp>( this );
                    ctx.ServiceContainer.Add<IContextSaver>( this );
                    // inject specific xaml serializers.
                    ctx.ServiceContainer.Add<IStructuredSerializer<Size>>( new XamlSerializer<Size>() );
                    ctx.ServiceContainer.Add<IStructuredSerializer<Color>>( new XamlSerializer<Color>() );
                    ctx.ServiceContainer.Add<IStructuredSerializer<LinearGradientBrush>>( new XamlSerializer<LinearGradientBrush>() );
                    ctx.ServiceContainer.Add<IStructuredSerializer<TextDecorationCollection>>( new XamlSerializer<TextDecorationCollection>() );
                    ctx.ServiceContainer.Add<IStructuredSerializer<FontWeight>>( new XamlSerializer<FontWeight>() );
                    ctx.ServiceContainer.Add<IStructuredSerializer<FontStyle>>( new XamlSerializer<FontStyle>() );
                    ctx.ServiceContainer.Add<IStructuredSerializer<Image>>( new XamlSerializer<Image>() );
                    ctx.ServiceContainer.Add<IStructuredSerializer<BitmapSource>>( new BitmapSourceSerializer<BitmapSource>() );
                    ctx.ServiceContainer.Add<IStructuredSerializer<InteropBitmap>>( new BitmapSourceSerializer<InteropBitmap>() );
                    ctx.ServiceContainer.Add<IStructuredSerializer<CachedBitmap>>( new BitmapSourceSerializer<CachedBitmap>() );
                    ctx.ServiceContainer.Add<IStructuredSerializer<BitmapFrame>>( new BitmapSourceSerializer<BitmapFrame>() );
                    ctx.ServiceContainer.Add<IStructuredSerializer<BitmapImage>>( new BitmapSourceSerializer<BitmapImage>() );
                    //ctx.ServiceContainer.Add<INotificationService>( _notificationMngr );
                }

                Context.PluginRunner.ApplyDone += OnApplyDone;

                _log.Info().Send( "Starting Apply..." );
                _firstApplySucceed = Context.PluginRunner.Apply();

                ctx.ConfigManager.SystemConfiguration.PropertyChanged += OnSystemConfigurationPropertyChanged;
                ctx.ConfigManager.UserConfiguration.PropertyChanged += OnUserConfigurationPropertyChanged;
            }


            return ctx;
        }

        void OnSystemConfigurationPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            //If the user has changed, we need to load the corresponding user configuration
            if( e.PropertyName == "CurrentUserProfile" )
            {
                using( _log.OpenInfo().Send( "OnSystemConfigurationPropertyChanged" ) )
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
        }

        void OnUserConfigurationPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
        }

        private void OnApplyDone( object sender, ApplyDoneEventArgs e )
        {
            _log.Info().Send( String.Format( "Apply Done. (Success : {0}).", e.Success ) );
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

        #region IHostHelp Members

        public event EventHandler<EventArgs> ShowHostHelp;

        public INamedVersionedUniqueId FakeHostHelpId
        {
            get
            {
                return new SimpleNamedVersionedUniqueId( Guid.Empty, new Version( AppVersion.Major, AppVersion.Minor, AppVersion.Patch ), "Application" );
            }
        }

        public void FireShowHostHelp()
        {
            if( ShowHostHelp != null )
                ShowHostHelp( this, EventArgs.Empty );
        }

        public Stream GetDefaultHelp()
        {
            return typeof( CivikeyStandardHost ).Assembly.GetManifestResourceStream( "Host.Resources.hosthelpcontent.zip" );
        }

        #endregion
    }
}
