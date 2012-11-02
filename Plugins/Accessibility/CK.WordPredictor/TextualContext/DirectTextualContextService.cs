using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using BasicCommandHandlers;
using CK.Plugin;
using CK.WordPredictor.Model;
using CommonServices;

namespace CK.WordPredictor
{
    [Plugin( "{409208EC-81AE-46A1-89E9-0D34943E4FBB}", PublicName = "DirectTextualContext", Categories = new[] { "Advanced" } )]
    public class DirectTextualContextService : IPlugin, ITextualContextService
    {
        SimpleTokenCollection _tokenCollection;

        public event PropertyChangedEventHandler PropertyChanged;

        [RequiredService]
        public ISendKeyCommandHandlerService SendKeyService { get; set; }

        [RequiredService]
        public ICommandManagerService CommandManager { get; set; }

        public DirectTextualContextService()
        {
            _tokenCollection = new SimpleTokenCollection();
        }

        public ITokenCollection Tokens
        {
            get { return _tokenCollection; }
        }

        public int CurrentTokenIndex
        {
            get { return _tokenCollection.Count - 1; }
        }

        public IToken CurrentToken
        {
            get
            {
                if( _position == CaretPosition.OutsideToken ) return null;

                return _tokenCollection[CurrentTokenIndex];
            }
        }

        public int CaretOffset
        {
            get { return CurrentToken.Value.Length - 1; }
        }

        CaretPosition _position;

        public CaretPosition CurrentPosition
        {
            get { return _position; }
        }

        internal void SetToken( string token )
        {
            if( token == "." )
            {
                _tokenCollection = new SimpleTokenCollection();
                _position = CaretPosition.OutsideToken;
            }
            else if( token == " "  )
            {
                _tokenCollection.Add( String.Empty );
                _position = CaretPosition.StartToken;
            }
            else
            {
                //We were on the end of a phrase. We start a new context.
                if( CurrentToken == null )
                {
                    _tokenCollection = new SimpleTokenCollection( token );
                }
                else //We continue in the same context.
                {
                    if( token.Length > 1 && token.IndexOf( ' ' ) == 0 )
                    {
                        SetToken( " " );
                        SetToken( token.TrimStart() );
                    }
                    else
                    {
                        string tokenValue = token.TrimEnd();
                        _tokenCollection[CurrentTokenIndex] = new SimpleToken( CurrentToken.Value + tokenValue );
                    }
                    if( token.Length > 1 && token[token.Length - 1] == ' ' )
                    {
                        SetToken( " " );
                    }
                }

                _position = CaretPosition.EndToken;
            }

            if( PropertyChanged != null )
                PropertyChanged( this, new PropertyChangedEventArgs( "Tokens" ) );

            if( CurrentToken != null )
            {
                if( PropertyChanged != null )
                    PropertyChanged( this, new PropertyChangedEventArgs( "CurrentToken" ) );
            }
        }

        protected virtual void OnKeySent( object sender, KeySentEventArgs e )
        {
            if( e != null ) SetToken( e.Key );
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return info.Error != null;
        }

        public void Start()
        {
            _position = CaretPosition.OutsideToken;
            if( SendKeyService != null )
            {
                SendKeyService.KeySent += OnKeySent;
            }
            if( CommandManager != null )
            {
                CommandManager.CommandSent += OnCommandSent;
            }
        }

        void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            CommandParser p = new CommandParser( e.Command );
            string str;
            if( p.IsIdentifier( out str ) && !e.Canceled && str == "sendString" )
            {
                p.GetNextToken();
                SetToken( p.StringValue );
            }
        }

        public void Stop()
        {
            SendKeyService.KeySent -= OnKeySent;
        }

        public void Teardown()
        {
        }
    }
}
