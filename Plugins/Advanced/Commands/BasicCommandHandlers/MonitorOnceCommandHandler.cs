#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\MonitorOnceCommandHandler.cs) is part of CiviKey. 
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
using CommonServices;
using CK.Plugin;
using System.Windows.Forms;
using CK.Context;
using CK.Core;
using System.Collections.Generic;
using CK.Keyboard.Model;
using CK.Plugins.SendInputDriver;

namespace BasicCommandHandlers
{
    [Plugin( "{E9EAB9C1-8F46-4DE1-AAED-0F6371C49F50}", Categories = new string[] { "Advanced" },
        PublicName = "Monitor command handler", Version = "1.0.0" )]
    public class MonitorOnceCommandHandler : BasicCommandHandler, IMonitorOnceCommandHandlerService
    {
        Dictionary<string, string> _clickActions;

        private const string CMD = "MonitorOnce";
        private const string SendKeyEventName = "SendKey";

        public MonitorOnceCommandHandler()
        {
            _clickActions = new Dictionary<string, string>();
        }

        [RequiredService]
        public IContext Context { get; set; }

        [RequiredService]
        public ISendKeyCommandHandlerService SendKeyCommandHandler { get; set; }

        [RequiredService]
        public ISendStringService SendStringCommandHandler { get; set; }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( !e.Canceled )
            {
                CommandParser p = new CommandParser( e.Command );
                if( p.MatchIdentifier( CMD ) )
                {
                    string eventKey;
                    string eventName;

                    if( p.MatchIsolatedChar( '.' ) )
                    {
                        if( p.IsIdentifier( out eventName ) )
                        {
                            if( p.Match( CommandParser.Token.OpenPar ) )
                            {
                                if( p.IsString( out eventKey ) )
                                {
                                    if( p.Match( CommandParser.Token.ClosePar ) )
                                    {
                                        if( p.MatchIsolatedChar( ':' ) )
                                        {
                                            Parse( eventKey, eventName, e.Command.Split( ':' )[1] );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void Parse( string key, string eventName, string actionContent )
        {
            switch( eventName )
            {
                case SendKeyEventName:
                    RegisterOnSendKey( key, actionContent );
                    break;
            }
        }

        public void RegisterOnSendKey( string key, string command )
        {
            if( !_clickActions.ContainsKey( key ) )
            {
                if( _clickActions.Count == 0 )
                {
                    SendStringCommandHandler.StringSent += new EventHandler<StringSentEventArgs>( OnStringSent );
                    SendKeyCommandHandler.KeySent += new EventHandler<KeySentEventArgs>( OnKeySent );
                }
                _clickActions.Add( key, command );
            }
            else
            {
                _clickActions.Remove( key );
                if( _clickActions.Count == 0 )
                {
                    SendStringCommandHandler.StringSent += new EventHandler<StringSentEventArgs>( OnStringSent );
                    SendKeyCommandHandler.KeySent -= new EventHandler<KeySentEventArgs>( OnKeySent );
                }
            }
        }

        void OnStringSent( object sender, StringSentEventArgs e )
        {
            ValueSent();
        }

        void OnKeySent( object sender, KeySentEventArgs e )
        {
            ValueSent();
        }

        void ValueSent()
        {
            foreach( var command in _clickActions )
                CommandManager.SendCommand( command.Key, command.Value );
            _clickActions.Clear();

            SendKeyCommandHandler.KeySent -= new EventHandler<KeySentEventArgs>( OnKeySent );
            SendStringCommandHandler.StringSent -= new EventHandler<StringSentEventArgs>( OnStringSent );
        }
    }
}
