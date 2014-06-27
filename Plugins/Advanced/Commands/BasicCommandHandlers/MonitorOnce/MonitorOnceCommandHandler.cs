#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\MonitorOnce\MonitorOnceCommandHandler.cs) is part of CiviKey. 
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
using CK.Context;
using System.Collections.Generic;
using CK.Plugins.SendInputDriver;
using ProtocolManagerModel;
using BasicCommandHandlers.Resources;

namespace BasicCommandHandlers
{
    [Plugin( "{E9EAB9C1-8F46-4DE1-AAED-0F6371C49F50}", Categories = new string[] { "Advanced" },
        PublicName = "MonitorOnce command handler", Version = "1.6.0" )]
    public class MonitorOnceCommandHandler : BasicCommandHandler, IMonitorOnceCommandHandlerService
    {
        Dictionary<string, string> _clickActions;

        private const string SENDKEY = "sendkey";
        private const string PROTOCOL = "monitoronce";

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

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IProtocolEditorsManager> ProtocolManagerService { get; set; }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( !e.Canceled && e.Command.StartsWith( PROTOCOL ) )
            {
                string parameter = e.Command.Substring( e.Command.IndexOf( ":" ) + 1 );
                //ex : monitoronce:sendkey,autoclose-shift,mode:remove,shift
                //     protocol:whatcommandtomonitor,thekeyofthecommandtoexecutewhenmonitoredmethodistriggered,thecommandtoexecutewhenmonitoredmethodistriggered

                string[] splittedParameter = parameter.Split( ',' );

                Parse( splittedParameter[0], splittedParameter[1], parameter.Substring( splittedParameter[0].Length + splittedParameter[1].Length + 2 ) );
            }
        }

        void Parse( string eventName, string key, string actionContent )
        {
            switch( eventName )
            {
                case SENDKEY:
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
                    SendStringCommandHandler.StringSent -= new EventHandler<StringSentEventArgs>( OnStringSent );
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
            SendKeyCommandHandler.KeySent -= new EventHandler<KeySentEventArgs>( OnKeySent );
            SendStringCommandHandler.StringSent -= new EventHandler<StringSentEventArgs>( OnStringSent );

            foreach( var command in _clickActions )
                CommandManager.SendCommand( command.Key, command.Value );
            _clickActions.Clear();   
        }

        public override void Start()
        {
            base.Start();
            ProtocolManagerService.Service.Register(
                                        new VMProtocolEditorWrapper(
                                        "monitoronce",
                                        R.MonitorOnceProtocolTitle,
                                        R.MonitorOnceProtocolDescription,
                                        typeof( MonitorOnceCommandParameterManager ) ),
                                        typeof( IMonitorOnceCommandHandlerService ) );
        }
    }
}
