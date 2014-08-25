#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ExportKeyboard.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
        bool _isClosing;

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

            _view.Closing += OnClosing;

            _view.Show();
        }

        void OnClosing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            if( !_isClosing )
            {
                _isClosing = true;
                Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( PluginIdString ), ConfigUserAction.Stopped );
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
