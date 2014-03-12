using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Storage;
using KeyboardEditor.Wizard.ViewModels;
using KeyboardEditor.Wizard.Views;

namespace KeyboardEditor.Wizard
{
    [Plugin( "{D94D1757-5BFB-4B80-9C8E-1B108F5C7086}", PublicName = "Keyboard import", Version = "1.0.0" )]
    public class ImportKeyboard : IPlugin
    {

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [RequiredService( Required = true )]
        public IContext Context { get; set; }

        ISharedDictionary _sharedDictionary;
        private ISharedDictionary SharedDictionary { get { return _sharedDictionary ?? (_sharedDictionary = Context.ServiceContainer.GetService<ISharedDictionary>()); } }

        ImportKeyboardViewModel _vm;
        ImportKeyboardView _view;

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _vm = new ImportKeyboardViewModel( this, KeyboardContext.Service.Keyboards );
            _view = new ImportKeyboardView()
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

        public List<string> GetImportKeyboardNames( string filePath )
        {
            List<string> keyboardNames = new List<string>();
            using( FileStream str = new FileStream( filePath, FileMode.Open ) )
            {
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, Context.ServiceContainer ) )
                {
                    XmlReader r = reader.Xml;
                    //r.Read();
                    while( r.IsStartElement( "Keyboard" ) )
                    {
                        keyboardNames.Add( r.GetAttribute( "Name" ) );
                        r.ReadToNextSibling( "Keyboard" );
                    }
                }
            }
            return keyboardNames;
        }

        public void ImportKeyboards( string filePath, string whiteListFilter = "")
        {
            Debug.Assert( filePath != null );
            HashSet<string> filter = new HashSet<string>( whiteListFilter.Split( '|' ) );

            if( !String.IsNullOrWhiteSpace( filePath ) )
            {
                using( FileStream str = new FileStream( filePath, FileMode.Open ) )
                {
                    using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, Context.ServiceContainer ) )
                    {
                        XmlReader r = reader.Xml;
                        while( r.IsStartElement( "Keyboard" ) )
                        {
                            string n = r.GetAttribute( "Name" );
                            if( string.IsNullOrWhiteSpace( whiteListFilter ) || filter.Contains( n ) )
                            {
                                IKeyboard kb = KeyboardContext.Service.Keyboards.Create( n );
                                reader.ReadInlineObjectStructured( kb );
                            }
                            r.ReadToNextSibling( "Keyboard" );
                        }
                    }
                }
            }
        }
    }
}
