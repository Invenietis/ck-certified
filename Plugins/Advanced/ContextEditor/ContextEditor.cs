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
using CK.Plugin.Config;
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

        ISharedDictionary _sharedDictionary;
        private ISharedDictionary SharedDictionary { get { return _sharedDictionary ?? ( _sharedDictionary = Context.ServiceContainer.GetService<ISharedDictionary>() ); } }

        /// <summary>
        /// Backs up a keyboard.
        /// Returns the file path where the keyboard has been backed up.
        /// 
        /// Throws a CKException if the IKeyboard implementation is not IStructuredSerializable
        /// </summary>
        /// <param name="keyboardToBackup">The keyboard ot backup</param>
        /// <returns>the path to the file in which the keyboard has been saved</returns>
        public string BackupKeyboard(IKeyboard keyboardToBackup)
        {
            string backupFileName = GenerateBackupFileName();
            IStructuredSerializable serializableModel = keyboardToBackup as IStructuredSerializable;
            if( serializableModel != null )
            {
                using( FileStream str = new FileStream( backupFileName, FileMode.CreateNew ) )
                {
                    using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( str, Context.ServiceContainer ) )
                    {
                        SharedDictionary.RegisterWriter( writer );
                        writer.Xml.WriteStartElement( "Keyboard" );
                        serializableModel.WriteContent( writer );
                        writer.Xml.WriteEndElement();
                    }
                }
                KeyboardBackup = new KeyboardBackup( keyboardToBackup, backupFileName );
            }
            else throw new CKException( "The IKeyboard implementation should be IStructuredSerializable" );

            return backupFileName;
        }

        //outputs a filepath of the form : [tempfolder]/CiviKey/CK-[GUID].txt
        private string GenerateBackupFileName()
        {
            string fileName = String.Format( "CK-{0}.txt", Guid.NewGuid().ToString() );
            string folderPath = Path.Combine( System.IO.Path.GetTempPath(), "CiviKey", "KeyboardEditorBackup" );
            if( !Directory.Exists( folderPath ) ) Directory.CreateDirectory( folderPath );
            string keyboardFilePath = Path.Combine( folderPath, fileName );
            return keyboardFilePath;
        }

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
                            _sharedDictionary.RegisterReader( reader, CK.SharedDic.MergeMode.None );
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
            else throw new CKException( "The IKeyboard implementation should be IStructuredSerializable" );
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


}

