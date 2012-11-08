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
    //[Plugin( "{409208EC-81AE-46A1-89E9-0D34943E4FBB}", PublicName = "SimpleTextualContextService", Categories = new[] { "Prediction" } )]
    public class SimpleTextualContextService : IPlugin, ITextualContextService
    {
        SimpleTokenCollection _tokenCollection;

        public event PropertyChangedEventHandler PropertyChanged;

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ISendKeyCommandHandlerService> SendKeyService { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ISendStringService> SendStringService { get; set; }

        public SimpleTextualContextService()
        {
        }

        public string RawContext
        {
            get { return String.Join( " ", _tokenCollection.Select( e => e.Value ) ); }
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
            get { return CurrentToken != null ? CurrentToken.Value.Length : 0; }
        }

        CaretPosition _position;

        public CaretPosition CurrentPosition
        {
            get { return _position; }
        }

        protected virtual bool IsResetContextToken( string token )
        {
            if( String.IsNullOrWhiteSpace( token ) ) return false;

            return Char.IsPunctuation( token.Trim()[0] );
        }

        protected virtual bool IsTokenSeparator( string token )
        {
            if( String.IsNullOrEmpty( token ) ) return false;
            if( String.IsNullOrEmpty( token.Trim() ) ) return true;

            char c = token[0];
            return Char.IsWhiteSpace( c ) || Char.IsSeparator( c );
        }

        internal void SetToken( string token )
        {
            if( IsResetContextToken( token ) )
            {
                _tokenCollection = new SimpleTokenCollection();
                _position = CaretPosition.OutsideToken;
            }
            else if( IsTokenSeparator( token ) )
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

            if( CurrentToken != null && PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( "RawContext" ) );
                PropertyChanged( this, new PropertyChangedEventArgs( "CurrentToken" ) );
            }
        }

        protected virtual void OnKeySent( object sender, KeySentEventArgs e )
        {
            if( e.Key != null ) SetToken( e.Key );
        }

        protected virtual void OnStringSent( object sender, StringSentEventArgs e )
        {
            if( e.StringVal != null ) SetToken( e.StringVal );
        }

        public bool Setup( IPluginSetupInfo info )
        {
            _tokenCollection = new SimpleTokenCollection();
            return info.Error != null;
        }

        public void Start()
        {
            _position = CaretPosition.OutsideToken;

            if( SendKeyService.Service != null )
                SendKeyService.Service.KeySent += OnKeySent;
            SendKeyService.ServiceStatusChanged += OnSendKeyServiceStatusChanged;

            if( SendStringService != null )
                SendStringService.Service.StringSent += OnStringSent;
            SendStringService.ServiceStatusChanged += OnSendStringServiceStatusChanged;

        }

        void OnSendStringServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == RunningStatus.Stopping )
            {
                SendStringService.Service.StringSent -= OnStringSent;
            }
            if( e.Current == RunningStatus.Starting )
            {
                SendStringService.Service.StringSent += OnStringSent;
            }
        }

        void OnSendKeyServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == RunningStatus.Stopping )
            {
                SendKeyService.Service.KeySent -= OnKeySent;
            }
            if( e.Current == RunningStatus.Starting )
            {
                SendKeyService.Service.KeySent += OnKeySent;
            }
        }

        public void Stop()
        {
            if( SendKeyService.Service != null )
                SendKeyService.Service.KeySent -= OnKeySent;
            SendKeyService.ServiceStatusChanged -= OnSendKeyServiceStatusChanged;

            if( SendStringService.Service != null )
                SendStringService.Service.StringSent -= OnStringSent;
            SendStringService.ServiceStatusChanged -= OnSendStringServiceStatusChanged;
        }

        public void Teardown()
        {
        }
    }
}
