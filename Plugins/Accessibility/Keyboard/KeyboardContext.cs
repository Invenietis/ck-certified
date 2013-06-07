#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\Keyboard\KeyboardContext.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Storage;
using CK.Plugin.Config;
using System.Xml;
using CK.Core;
using System.Diagnostics;
using CK.Plugin;

namespace CK.Keyboard
{
    [Plugin( KeyboardContext.PluginIdString, Version = KeyboardContext.PluginIdVersion, PublicName = PluginPublicName )]
    public partial class KeyboardContext : IKeyboardContext, IPlugin, IStructuredSerializable, IStructuredSerializer<KeyboardCollection>
    {
        const string PluginIdString = "{2ED1562F-2416-45cb-9FC8-EEF941E3EDBC}";
        const string PluginIdVersion = "2.5.2";
        const string PluginPublicName = "CK.KeyboardContext";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        bool _isKeyboardContextDirty;
        KeyboardCollection _keyboards;

        public KeyboardContext()
            : this( null )
        {
        }

        public KeyboardContext( IServiceProvider sp )
        {
            _keyboards = new KeyboardCollection( this );

            _empty = new KeyboardMode( this );
            _modes = new Dictionary<string, KeyboardMode>( StringComparer.Ordinal );
            _modes.Add( String.Empty, _empty );

            if( sp != null ) sp.GetService<ISimpleServiceContainer>( true ).Add<IStructuredSerializer<KeyboardCollection>>( this );
        }

        public IPluginConfigAccessor Configuration { get; set; }

        [RequiredService]
        public IConfigContainer ConfigContainer { get; set; }

        [RequiredService]
        public ISimplePluginRunner PluginRunner { get; set; }

        public IKeyboardCollection Keyboards
        {
            get { return _keyboards; }
        }

        IKeyboard IKeyboardContext.CurrentKeyboard
        {
            get { return _keyboards.Current; }
            set { _keyboards.Current = (Keyboard)value; }
        }

        internal Keyboard CurrentKeyboard
        {
            get { return _keyboards.Current; }
            set { _keyboards.Current = value; }
        }

        public event EventHandler<CurrentKeyboardChangedEventArgs> CurrentKeyboardChanged
        {
            add { _keyboards.CurrentChanged += value; }
            remove { _keyboards.CurrentChanged -= value; }
        }

        public event EventHandler<CurrentKeyboardChangingEventArgs> CurrentKeyboardChanging
        {
            add { _keyboards.CurrentChanging += value; }
            remove { _keyboards.CurrentChanging -= value; }
        }

        public bool IsDirty
        {
            get { return _isKeyboardContextDirty; }
        }

        public void SetKeyboardContextDirty()
        {
            _isKeyboardContextDirty = true;
        }

        public bool Setup( IPluginSetupInfo info )
        {
            Configuration.ConfigChanged += new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );

            var ctx = Configuration.Context.GetOrSet<KeyboardCollection>( "KeyboardCollection", new KeyboardCollection( this ) );

            return true;
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
        }

        public void Start()
        {
            _keyboards.OnStart();
        }

        public void Teardown()
        {

        }

        public void Stop()
        {
            _keyboards.OnStop();
        }

        object IStructuredSerializer<KeyboardCollection>.ReadInlineContent( IStructuredReader sr, KeyboardCollection o )
        {
            _keyboards.Clear();
            _keyboards.ReadInlineContent( sr );
            return _keyboards;
        }

        void IStructuredSerializer<KeyboardCollection>.WriteInlineContent( IStructuredWriter sw, KeyboardCollection o )
        {
            _keyboards.WriteInlineContent( sw );
        }

        public void ReadContent( IStructuredReader sr )
        {
            sr.ServiceContainer.Add<IStructuredSerializer<KeyboardCollection>>( this );

            sr.Xml.Read();
            sr.ReadInlineObjectStructuredElement( "Keyboards", _keyboards );
        }

        public void WriteContent( IStructuredWriter sw )
        {
            sw.ServiceContainer.Add<IStructuredSerializer<KeyboardCollection>>( this );

            sw.WriteInlineObjectStructuredElement( "Keyboards", _keyboards );
        }
    }
}
