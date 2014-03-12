using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.SharedDic;
using CK.Storage;
using KeyboardEditor.s;
using KeyboardEditor.Wizard.ViewModels;

namespace KeyboardEditor
{
    [Plugin( ExportKeyboard.PluginIdString, PublicName = ExportKeyboard.PluginPublicName, Version = ExportKeyboard.PluginIdVersion )]
    public class ExportKeyboard : IPlugin
    {
        const string PluginIdString = "{244C578B-322A-4733-A34B-EEC0558F61D5}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Keyboard export";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [RequiredService( Required = true )]
        public IContext Context { get; set; }

        ISharedDictionary _sharedDictionary;
        private ISharedDictionary SharedDictionary { get { return _sharedDictionary ?? (_sharedDictionary = Context.ServiceContainer.GetService<ISharedDictionary>()); } }

        ExportKeyboardViewModel _vm;
        ExportKeyboardView _view;

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _vm = new ExportKeyboardViewModel( this, KeyboardContext.Service.Keyboards );
            _view = new ExportKeyboardView()
            {
                DataContext = _vm
            };
            _view.Show();
        }

        public void Stop()
        {
            _view.Hide();
        }

        public void Teardown()
        {
        }

        #endregion

        public void ExportKeyboards( string fileName, IEnumerable<IKeyboard> keyboardToExport )
        {
            FileInfo fileInfo = new FileInfo( fileName );
            if( fileInfo.Exists ) fileInfo.Delete();
            using( FileStream str = new FileStream( fileName, FileMode.CreateNew ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( str, Context.ServiceContainer ) )
                {
                    SharedDictionary.RegisterWriter( writer );
                    foreach( var k in keyboardToExport )
                    {
                        IStructuredSerializable serializableModel = k as IStructuredSerializable;
                        if( serializableModel != null )
                        {
                            writer.Xml.WriteStartElement( "Keyboard" );
                            serializableModel.WriteContent( writer );
                            writer.Xml.WriteEndElement();
                        }
                    }
                }
            }
        }
    }
}
