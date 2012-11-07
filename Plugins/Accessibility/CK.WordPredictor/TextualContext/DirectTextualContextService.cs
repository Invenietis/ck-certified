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
    //[Plugin( "{409208EC-81AE-46A1-89E9-0D34943E4FBB}", PublicName = "DirectTextualContext", Categories = new[] { "Prediction" } )]
    public class DirectTextualContextService : IPlugin, ITextualContextService
    {
        SimpleTokenCollection _tokenCollection;

        public event PropertyChangedEventHandler PropertyChanged;

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ISendKeyCommandHandlerService> SendKeyService { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ISendStringService> SendStringService { get; set; }

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
