using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Storage;
using CK.Windows.Config;
using ContextEditor.ViewModels;

namespace ContextEditor
{
    [Plugin( ContextEditor.PluginIdString,
        PublicName = PluginPublicName,
        Version = ContextEditor.PluginIdVersion,
        Categories = new string[] { "Visual", "Advanced" } )]
    public class ContextEditor : IPlugin
    {
        const string PluginIdString = "{66AD1D1C-BF19-405D-93D3-30CA39B9E52F}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "ContextEditor";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [RequiredService( Required = true )]
        public IContext Context { get; set; }

        ContextEditorBootstrapper bootstrap;
        AppViewModel _appViewModel;
        WindowManager w;

        public bool Setup( IPluginSetupInfo info )
        {
            bootstrap = new ContextEditorBootstrapper();
            return bootstrap != null;
        }

        public void Start()
        {
            w = new WindowManager();
            _appViewModel = new AppViewModel( this );
            dynamic settings = new ExpandoObject();

            settings.SizeToContent = SizeToContent.WidthAndHeight;
            settings.Height = 800;
            settings.Width = 800;
            w.ShowWindow( _appViewModel, null, settings );
        }

        public void Stop()
        {
            Window win = _appViewModel.GetView( null ) as Window;
            if( win != null )
                win.Close();
        }

        public void Teardown()
        {
            _appViewModel = null;
            w = null;
        }

        /// <summary>
        /// This property holds the version of the keyboard that is being edited, before any modification.
        /// if the Filepath contained in this object is null while we are editing a keyboard, it means that this keyboard is a new one.
        /// </summary>
        public KeyboardBackup KeyboardBackup { get; set; }

        /// <summary>
        /// Cancels all modifications made to the keyboard being currently modified
        /// Throws a <see cref="NullReferenceException"/> if <see cref="KeyboardBackup"/> is null.
        /// </summary>
        public void CancelModifications()
        {
            if( KeyboardBackup == null || KeyboardBackup.BackedUpKeyboard == null ) throw new NullReferenceException( "Can't cancel modifications on a null KeyboardBackup" );

            IStructuredSerializable serializableKeyboard = KeyboardBackup.BackedUpKeyboard as IStructuredSerializable;
            if( serializableKeyboard != null )
            {
                if( !String.IsNullOrWhiteSpace( KeyboardBackup.BackUpFilePath ) )
                {
                    using( FileStream str = new FileStream( KeyboardBackup.BackUpFilePath, FileMode.Open ) )
                    {
                        using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, Context.ServiceContainer ) )
                        {
                            //Erasing all properties of the keyboard. We re-apply the backedup ones.
                            serializableKeyboard.ReadContent( reader );
                        }
                    }
                }
                else ///if KeyboardBackupFilePath is null of whitespace, then we were creating a new keyboard. Reverting the chances means destroying the keyboard.
                {
                    KeyboardBackup.BackedUpKeyboard.Destroy();
                }

                //After cancelling modifications, we have no backup left.
                EnsureBackupIsClean();
            }
        }

        /// <summary>
        /// Deletes the backup file if it exists
        /// sets <see cref="KeyboardBackup"/> to null
        /// </summary>
        public void EnsureBackupIsClean()
        {
            if( KeyboardBackup != null && File.Exists( KeyboardBackup.BackUpFilePath ) )
            {
                File.Delete( KeyboardBackup.BackUpFilePath );
                KeyboardBackup = null;
            }
        }
    }


    /// <summary>
    /// Object that links an <see cref="IKeyboard"/> to the file which contains its XML backup.
    /// <see cref="BackUpFilePath"/> can be null of whitespace. In this case, the keyboard is a new one, it has no backup.
    /// </summary>
    public class KeyboardBackup
    {
        /// <summary>
        /// Gets the IKeyboard that has been backed up.
        /// </summary>
        public IKeyboard BackedUpKeyboard { get; private set; }

        /// <summary>
        /// Get the path to the Backed up keyboard
        /// can be null of whitespace. In this case, the keyboard is a new one, it has no backup.
        /// </summary>
        public string BackUpFilePath { get; private set; }

        /// <summary>
        /// Gets whether the backup corresponds to a new keyboard (a backup that.. dosen't backup anything)
        /// </summary>
        public bool IsNew { get { return String.IsNullOrWhiteSpace( BackUpFilePath ); } }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="backedUpKeyboard">The <see cref="IKeyboard"/> that has been backedup</param>
        /// <param name="backedUpFilePath">Th elinked back up file path</param>
        public KeyboardBackup( IKeyboard backedUpKeyboard, string backedUpFilePath )
        {
            if( backedUpKeyboard == null ) throw new ArgumentNullException( "backedUpKeyboard", "The backed up keyboard can't be null in the ctor of a KeyboardBackup object" );
            if( !String.IsNullOrWhiteSpace( backedUpFilePath ) && !File.Exists( backedUpFilePath ) ) throw new ArgumentException( "backedUpFilePath", "The keyboard's backup file path must be either null (in the case of a new keyboard) or be an existing file." );

            BackedUpKeyboard = backedUpKeyboard;
            BackUpFilePath = backedUpFilePath;
        }
    }
}

