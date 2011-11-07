using System;
using CommonServices;
using CK.Plugin;
using System.Windows.Forms;
using CK.Context;
using CK.Core;
using System.Collections.Generic;
using CK.Keyboard.Model;

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
                                            Parse( eventKey, eventName, e.Command.Split(':')[1] );
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
                    SendKeyCommandHandler.KeySent += new EventHandler<KeySentEventArgs>( OnKeySent );
                _clickActions.Add( key, command );
            }
            else
            {
                _clickActions.Remove( key );
                if( _clickActions.Count == 0 )
                    SendKeyCommandHandler.KeySent -= new EventHandler<KeySentEventArgs>( OnKeySent );
            }
        }

        void OnKeySent( object sender, KeySentEventArgs e )
        {
            foreach( var command in _clickActions )
                CommandManager.SendCommand( command.Key, command.Value );
            _clickActions.Clear();

            SendKeyCommandHandler.KeySent -= new EventHandler<KeySentEventArgs>( OnKeySent );
        }
    }
}
