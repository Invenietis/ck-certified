#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\SendInputDriver\SendStringPlugin.cs) is part of CiviKey. 
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
using CK.Plugin;
using CK.InputDriver;
using CK.Core;

namespace CK.Plugins.SendInputDriver
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion, Categories = new string[] { "Advanced" } )]
    public class SendStringPlugin : IPlugin, ISendStringService, IDynamicService
    {
        #region Plugin description

        const string PluginGuidString = "{4F82CC10-1115-4FF0-B483-E95EEEA21107}";
        const string PluginVersion = "2.0.0";
        const string PluginPublicName = "Send string command handler";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        public event EventHandler<StringSentEventArgs>  StringSent;
        public event EventHandler<StringSendingEventArgs>  StringSending;

        public event EventHandler<NativeKeySentEventArgs>  KeySent;
        public event EventHandler<NativeKeySendingEventArgs>  KeySending;

        private CommonServices.ICommandManagerService _cm;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public CommonServices.ICommandManagerService CommandManager
        {
            get { return this._cm; }
            set { this._cm = value; }
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Teardown()
        {
        }

        public virtual void Start()
        {
            if( _cm != null )
                _cm.CommandSent += new EventHandler<CommonServices.CommandSentEventArgs>( OnCommandSent );
        }

        public virtual void Stop()
        {
            if( _cm != null )
                _cm.CommandSent -= new EventHandler<CommonServices.CommandSentEventArgs>( OnCommandSent );
        }

        protected void OnCommandSent( object sender, CommonServices.CommandSentEventArgs e )
        {
            if( e.Command.StartsWith( "sendString:" ) )
            {
                SendString( e.Command.Substring( "sendString:".Length ) );
            }
            else if( e.Command.StartsWith( "sendKey:" ) )
            {
                var keyString = e.Command.Substring( "sendKey:".Length );
                Native.KeyboardKeys result;
                if( Enum.TryParse<Native.KeyboardKeys>( keyString, true, out result ) )
                {
                    SendKeyboardKey( result );
                }
            }
        }

        public void SendString( string s )
        {
            SendString( this, s );
        }

        public void SendString( object sender, string s )
        {
            DoSendString( sender, s );
        }

        public void SendKeyboardKey( Native.KeyboardKeys key )
        {
            NativeKeySendingEventArgs e = new NativeKeySendingEventArgs( key );
            if( this.KeySending != null )
            {
                this.KeySending( this, e );
            }
            if( !e.Cancel )
            {
                KeyboardProcessor.Process( key );
                if( this.KeySent != null )
                {
                    this.KeySent( this, new NativeKeySentEventArgs( key ) );
                }
            }
        }

        private void DoSendString( object sender, string s )
        {
            StringSendingEventArgs e = new StringSendingEventArgs( s );
            if( this.StringSending != null )
            {
                this.StringSending( this, e );
            }
            if( !e.Cancel )
            {
                KeyboardProcessor.ProcessString( s );
                if( this.StringSent != null )
                {
                    this.StringSent( this, new StringSentEventArgs( s ) );
                }
            }
        }

    }
}

