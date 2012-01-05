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
using Host.Resources;
using CK.Windows.App;
using System.Collections.Generic;
using CK.Core;
using System.Linq;

namespace Host
{
    /// <summary>
    /// Singleton host. Its private constructor is safe (no exceptions can be 
    /// thrown except an out of memory: we can safely ignore this pathological case).
    /// </summary>
    public class CivikeyStandardHost : AbstractContextHost, IHostInformation
    {
        Version _appVersion;
        bool _firstApplySucceed;
        NotificationManager _notificationMngr;
        CKAppParameters applicationParameters;


        /// <summary>
        /// Gets the current version of the Civikey-Standard application.
        /// It is stored in the system configuration file and updated by the installer.
        /// </summary>
        public Version AppVersion
        {
            get { return _appVersion ?? (_appVersion = new Version( (string)SystemConfig.GetOrSet( "Version", "2.5" ) )); }
        }
        
         /// <summary>
        /// The SubAppName is the name of the package (Standard, Steria etc...)
        /// using that, we can have different contexts/userconfs for each packages installed on the computer.
        /// </summary>
        private CivikeyStandardHost(CKAppParameters parameters)
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

            _notificationMngr = new NotificationManager();                        

            // Discover available plugins.
            string pluginPath = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "Plugins" );
            if( Directory.Exists( pluginPath ) ) ctx.PluginRunner.Discoverer.Discover( new DirectoryInfo( pluginPath ), true );

            RequirementLayer hostRequirements = new RequirementLayer( "CivikeyStandardHost" );
            hostRequirements.PluginRequirements.AddOrSet( new Guid( "{2ed1562f-2416-45cb-9fc8-eef941e3edbc}" ), RunningRequirement.MustExistAndRun );

            ctx.PluginRunner.Add( hostRequirements );

            // Load or initialize the ctx.
            LoadResult res = Instance.LoadContext();        

            // Initializes Services.
            {
                ctx.ServiceContainer.Add<IHostInformation>( this );
                // inject specific xaml serializers.
                ctx.ServiceContainer.Add( typeof( IStructuredSerializer<Size> ), new XamlSerializer<Size>() );
                ctx.ServiceContainer.Add( typeof( IStructuredSerializer<Color> ), new XamlSerializer<Color>() );
                ctx.ServiceContainer.Add( typeof( IStructuredSerializer<LinearGradientBrush> ), new XamlSerializer<LinearGradientBrush>() );
                ctx.ServiceContainer.Add( typeof( IStructuredSerializer<TextDecorationCollection> ), new XamlSerializer<TextDecorationCollection>() );
                ctx.ServiceContainer.Add( typeof( IStructuredSerializer<FontWeight> ), new XamlSerializer<FontWeight>() );
                ctx.ServiceContainer.Add( typeof( IStructuredSerializer<FontStyle> ), new XamlSerializer<FontStyle>() );
                ctx.ServiceContainer.Add( typeof( IStructuredSerializer<Image> ), new XamlSerializer<Image>() );
                ctx.ServiceContainer.Add( typeof( INotificationService ), _notificationMngr );
            }

            Context.PluginRunner.ApplyDone += new EventHandler<ApplyDoneEventArgs>( OnApplyDone );
            
            _firstApplySucceed = Context.PluginRunner.Apply();
            return ctx;
        }

        private void OnApplyDone( object sender, ApplyDoneEventArgs e )
        {
        //ExecutionPlanREsult dosen't exist anymore in the applydoneEventArg, how should we let the user decide what to do ?

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

        /// <summary>
        /// Gets or sets the full path of the user configuration file.
        /// Defaults to "User.config.ck" file in <see cref="ApplicationDataPath"/>.
        /// </summary>
        public virtual string DefaultUserConfigPath
        {
            get { return applicationParameters.ApplicationDataPath + "User.config.ck"; }
        }

        /// <summary>
        /// Gets the full path of the machine configuration file.
        /// Defaults to "System.config.ck" file in <see cref="CommonApplicationDataPath"/>.
        /// </summary>
        public virtual string DefaultSystemConfigPath
        {
            get { return applicationParameters.CommonApplicationDataPath + "System.config.ck"; }
        }

        protected override Uri GetSystemConfigAddress()
        {
            return new Uri( DefaultSystemConfigPath );
        }

        protected override Uri GetDefaultUserConfigAddress( bool saving )
        {
            return new Uri( DefaultUserConfigPath );
        }

        protected override KeyValuePair<string, Uri> GetDefaultContextProfile( bool saving )
        {
            var e = new ContextProfileRequiredEventArgs( Context, saving )
            {
                Address = new Uri(Path.Combine(applicationParameters.ApplicationDataPath, "Context.xml")),
                DisplayName = String.Format("", DateTime.Now ) // R.NewContextDisplayName
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
            get { return applicationParameters.SubAppName; }
        }
    }
}
