using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Context;
using Host.Services;
using System.IO;
using CK.Plugin.Hosting;
using System.Reflection;
using CK.Plugin;
using CK.Keyboard.Model;
using CommonServices;
using CK.Storage;
using Host.Services.Helper;
using System.Windows;
using System.Windows.Media;
using CK.Core;
using Host.Resources;
using System.Windows.Forms;
using System.Windows.Controls;

namespace Host
{
    /// <summary>
    /// Singleton host. Its private constructor is safe (no exceptions can be 
    /// thrown except an out of memory: we can safely ignore this pathological case).
    /// </summary>
    public class CivikeyStandardHost : StandardContextHost
    {
        IService<IKeyboardContext> _kbCtx;
        NotificationManager _notificationMngr;
        Version _version;
        bool _firstApplySucceed;

        /// <summary>
        /// Gets the current version of the Civikey-Standard application.
        /// It is stored in the system configuration file and updated by the installer.
        /// </summary>
        public Version Version
        {
            get { return _version ?? (_version = new Version( (string)SystemConfig.GetOrSet( "Version", "2.5" ) )); }
        }

        /// <summary>
        /// Note that the subAppName ("2.5") is just a way to have another folder 
        /// to isolate a big version if needed once.
        /// It is not really used currently.
        /// </summary>
        private CivikeyStandardHost()
            : base( "Civikey", "2.5" )
        {
        }

        /// <summary>
        /// Singleton instance.
        /// </summary>
        static readonly public CivikeyStandardHost Instance = new CivikeyStandardHost();

        public override IContext CreateContext()
        {
            IContext ctx = base.CreateContext();
            
            _notificationMngr = new NotificationManager();
            // Exceptions are displayed through the Notification service.
            ctx.Loaded += ( o, e ) => _notificationMngr.ShowNotification( Guid.Empty, R.ContextLoadedTitle, R.ContextLoadedContent, 5000, NotificationTypes.Ok );

            // Discover available plugins.
            string pluginPath = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "Plugins" );
            if( Directory.Exists( pluginPath ) ) ctx.PluginRunner.Discoverer.Discover( new DirectoryInfo( pluginPath ), true );

            // Load or initialize the ctx.
            if( ContextPath != null && File.Exists( ContextPath ) )
            {
                Instance.LoadContext( CivikeyStandardHost.Instance.ContextPath );
            }
            else
            {
                Instance.LoadContext( Assembly.GetExecutingAssembly(), "Host.Resources.Contexts.ContextCiviKey.xml" );
            }

            // Initialize requirements.
            {
                RequirementLayer hostRequirements = new RequirementLayer( "CivikeyStandardHost" );
                // If we use an embedded context to load a default keyboard
                // this code can be replaced by an embeded user config ... 
                hostRequirements.ServiceRequirements.AddOrSet( typeof( IKeyboardContext ).AssemblyQualifiedName, RunningRequirement.MustExistTryStart );

                // LogPlugin
                hostRequirements.PluginRequirements.AddOrSet( new Guid( "{FEA8570C-2ECE-44b3-B1CE-0DBA414D5045}" ), RunningRequirement.MustExistAndRun );

                ctx.PluginRunner.Add( hostRequirements );
            }
            // Initializes Services.
            {
                // inject specific xaml serializers.
                // Question : Can't we find better solution ? (maybe more generic)
                // ==> like ServiceContainer.Add<IXamlSerializer<T>>( (T (type)) => new XamlSerializer<T>() );
                ctx.ServiceContainer.Add<IStructuredSerializer<Size>>( () => new XamlSerializer<Size>() );
                ctx.ServiceContainer.Add<IStructuredSerializer<Color>>( () => new XamlSerializer<Color>() );
                ctx.ServiceContainer.Add<IStructuredSerializer<LinearGradientBrush>>( () => new XamlSerializer<LinearGradientBrush>() );
                ctx.ServiceContainer.Add<IStructuredSerializer<TextDecorationCollection>>( () => new XamlSerializer<TextDecorationCollection>() );
                ctx.ServiceContainer.Add<IStructuredSerializer<FontWeight>>( () => new XamlSerializer<FontWeight>() );
                ctx.ServiceContainer.Add<IStructuredSerializer<FontStyle>>( () => new XamlSerializer<FontStyle>() );
                ctx.ServiceContainer.Add<IStructuredSerializer<Image>>( () => new XamlSerializer<Image>() );
                ctx.ServiceContainer.Add<INotificationService>( _notificationMngr );
            }
            Context.PluginRunner.ApplyDone += new EventHandler<ApplyDoneEventArgs>( OnApplyDone );
            _firstApplySucceed = Context.PluginRunner.Apply();
            return ctx;
        }

        private void OnApplyDone( object sender, ApplyDoneEventArgs e )
        {
            if( e.ExecutionPlanResult != null)
            {
                if(
                    (e.ExecutionPlanResult.Status == ExecutionPlanResultStatus.SetupError 
                    || e.ExecutionPlanResult.Status == ExecutionPlanResultStatus.StartError))                    
                {
                    if( System.Windows.MessageBox.Show( String.Format( R.PluginThrewExceptionAtStart, e.ExecutionPlanResult.Culprit.PublicName ), R.PluginThrewExceptionAtStartTitle, MessageBoxButton.YesNo ) == MessageBoxResult.Yes )
                    {//if the user wants to try launching CiviKey without the culprit
                        Context.PluginRunner.Apply();
                    }
                    else
                    {//otherwise, stop CiviKey
                        Context.RaiseExitApplication( true );
                    }
                }
            }
            else //e is null means that the plan couldn't be resolved, the system hasn't been changed, but requirements are messy
            {
                if( _notificationMngr != null )
                    _notificationMngr.ShowNotification( Guid.Empty, R.ForbiddenActionTitle, R.ForbiddenAction, 5000, NotificationTypes.Warning );
                else
                    System.Windows.MessageBox.Show( R.ForbiddenAction );
                
                //TODO : Revert the configuration (if it is possible ?)
            }
        }

        public void Initialize()
        {
            if( _firstApplySucceed )
            {
                _kbCtx = Instance.Context.GetService<IService<IKeyboardContext>>();
                if( _kbCtx != null && _kbCtx.Status == RunningStatus.Started )
                {
                    // At this point KeyboardContext exists, and the service is started.
                    if( _kbCtx.Service.Keyboards.Count > 0 )
                        _notificationMngr.ShowNotification( Guid.Empty, R.CKIsRunning, R.EditConfig, 5000 );
                    else
                        _notificationMngr.ShowNotification( Guid.Empty, R.CKIsRunning, R.ContextIsEmpty, 10000, NotificationTypes.Message, ( o, e ) => LoadContext() );
                }
            }
            else
            {
                // If the apply fails, for this moment we says "Oups ..."
                // but we can retry an apply or ask the user what to do.
                _notificationMngr.ShowNotification( Guid.Empty, R.Oups, R.ErrorApplyingConfiguration, 0, NotificationTypes.Error );
            }
        }

        public void LoadContext()
        {
            IContext ctx = Context;

            using( OpenFileDialog dialog = new OpenFileDialog() )
            {
                dialog.CheckFileExists = true;
                dialog.Filter = "Xml files (*.xml)|*.xml";
                if( dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK )
                {
                    try
                    {
                        if( !LoadContext( dialog.FileName ) )
                        {
                            ctx.GetService<INotificationService>( true ).ShowNotification( Guid.Empty, R.Oups, R.UnableToLoadContext, 5000, NotificationTypes.Error );
                        }
                    }
                    catch( Exception ex )
                    {
                        ctx.GetService<INotificationService>( true ).ShowNotification( Guid.Empty, ex );
                    }
                    finally
                    {
                        ctx.PluginRunner.Apply();
                    }
                }
            }
        }
    }
}
