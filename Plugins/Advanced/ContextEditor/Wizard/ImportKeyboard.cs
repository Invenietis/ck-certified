#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ImportKeyboard.cs) is part of CiviKey. 
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
    [Plugin( ImportKeyboard.PluginGuidString, PublicName = ImportKeyboard.PluginPublicName, Version = ImportKeyboard.PluginIdVersion )]
    public class ImportKeyboard : IPlugin
    {

        const string PluginGuidString = "{D94D1757-5BFB-4B80-9C8E-1B108F5C7086}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Keyboard import";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginIdVersion, PluginPublicName );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [RequiredService( Required = true )]
        public IContext Context { get; set; }

        ISharedDictionary _sharedDictionary;

        ImportKeyboardViewModel _vm;
        ImportKeyboardView _view;
        bool _isClosing;

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _sharedDictionary = Context.ServiceContainer.GetService<ISharedDictionary>();

            _vm = new ImportKeyboardViewModel( this, KeyboardContext.Service.Keyboards );
            _view = new ImportKeyboardView()
            {
                DataContext = _vm
            };

            _view.Closing += OnClosing;

            _view.Show();
        }

        void OnClosing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            if( !_isClosing )
            {
                _isClosing = true;
                Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( PluginGuidString ), ConfigUserAction.Stopped );
                Context.PluginRunner.Apply();
                return;
            }
            else
            {
                _isClosing = false;
                _vm = null;
            }
        }

        public void Stop()
        {
            if( !_isClosing )
            {
                _isClosing = true;
                _view.Close();
            }
            else
            {
                _isClosing = false;
                _vm = null;
            }
        }

        public void Teardown()
        {
        }

        #endregion

        public List<string> GetImportKeyboardNames( string filePath )
        {
            List<string> keyboardNames = new List<string>();
            try
            {
                using( FileStream str = new FileStream( filePath, FileMode.Open ) )
                {
                    using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, Context.ServiceContainer ) )
                    {
                        XmlReader r = reader.Xml;
                        if( !r.IsStartElement( "Keyboard" ) ) r.ReadToFollowing( "Keyboard" );
                        while( r.IsStartElement( "Keyboard" ) )
                        { 
                            keyboardNames.Add( r.GetAttribute( "Name" ) );
                            r.ReadToNextSibling( "Keyboard" );
                        }
                    }
                }
                return keyboardNames;
            }
            catch( Exception e )
            {
                return new List<string>();
            }
        }

        public void ImportKeyboards( string filePath, string whiteListFilter = "" )
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
                        if( !r.IsStartElement( "Keyboard" ) ) r.ReadToFollowing( "Keyboard" );
                        while( r.IsStartElement( "Keyboard" ) )
                        {
                            string n = r.GetAttribute( "Name" );
                            IKeyboard k = null;
                            if( string.IsNullOrWhiteSpace( whiteListFilter ) || filter.Contains( n ) )
                            {
                                if( KeyboardContext.Service.Keyboards.FirstOrDefault( kb => kb.Name == n ) != null )
                                {
                                    if( KeyboardContext.Service.Keyboards[n] == KeyboardContext.Service.CurrentKeyboard )
                                    {
                                        k = KeyboardContext.Service.Keyboards[n];
                                        k.Rename( k.GetHashCode().ToString() );
                                    }
                                    else
                                    {
                                        KeyboardContext.Service.Keyboards[n].Destroy();
                                    }
                                }
                                //IKeyboard keyboard = KeyboardContext.Service.Keyboards.Create( n );
                                //reader.ReadInlineObjectStructured( keyboard );

                                IStructuredSerializable serializableKeyboard = (IStructuredSerializable)KeyboardContext.Service.Keyboards.Create( n );
                                if( serializableKeyboard == null ) throw new CKException( "The IKeyboard implementation should be IStructuredSerializable" );
                                using( var sub = reader.OpenSubReader() )
                                {
                                    _sharedDictionary.RegisterReader( sub, CK.SharedDic.MergeMode.None );
                                    //Erasing all properties of the keyboard. We re-apply the backedup ones.
                                    serializableKeyboard.ReadContent( sub );
                                }

                                if( k != null )
                                {
                                    KeyboardContext.Service.CurrentKeyboard = (IKeyboard)serializableKeyboard;
                                    k.Destroy();
                                }
                            }
                            else 
                            {
                                r.ReadToNextSibling( "Keyboard" );
                            }
                        }
                    }
                }
            }
        }
    }
}
