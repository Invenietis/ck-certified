using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using BasicCommandHandlers;
using CK.Plugin;
using CK.WordPredictor.Model;
using CommonServices;
using CK.Plugins.SendInput;

namespace CK.WordPredictor
{
    [Plugin( "{409208EC-81AE-46A1-89E9-0D34943E4FBB}", PublicName = "DirectTextualContext", Categories = new[] { "Prediction" } )]
    public class DirectTextualContextService : IPlugin, ITextualContextService
    {
        SimpleTokenCollection _tokenCollection;

        public event PropertyChangedEventHandler PropertyChanged;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ISendKeyCommandHandlerService SendKeyService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ISendStringService SendStringService { get; set; }

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
            else if( token == " " )
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
                        string tokenFull = CurrentToken.Value + tokenValue;

                        _tokenCollection[CurrentTokenIndex] = new SimpleToken( tokenFull );
                        if( token.Length > 1 && token[token.Length - 1] == ' ' )
                        {
                            _tokenCollection.Add( String.Empty );
                            _position = CaretPosition.StartToken;
                        }
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

        protected virtual void OnStringSent( object sender, StringSentEventArgs e )
        {
            SetToken( e.StringVal );
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return info.Error != null;
        }

        public void Start()
        {
            _position = CaretPosition.OutsideToken;
            if( SendKeyService != null )
                SendKeyService.KeySent += OnKeySent;

            if( SendStringService != null )
                SendStringService.StringSent += OnStringSent;
        }

        public void Stop()
        {
            if( SendKeyService != null )
                SendKeyService.KeySent -= OnKeySent;
            if( SendStringService != null )
                SendStringService.StringSent -= OnStringSent;
        }

        public void Teardown()
        {
        }
    }
}
