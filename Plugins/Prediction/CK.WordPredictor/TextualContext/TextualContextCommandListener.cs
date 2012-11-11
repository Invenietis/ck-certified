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
    [Plugin( "{AD9B0316-0498-4E79-989E-1EB43F9644C7}", PublicName = "TextualContext - Command Listener ", Categories = new[] { "Prediction" } )]
    public class TextualContextCommandListener : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ISendKeyCommandHandlerService> SendKeyService { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ISendStringService> SendStringService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ITextualContextService TextualContextService { get; set; }

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
            string rawContext = TextualContextService.RawContext;
            string newRawContext = String.Concat( rawContext, token );
            TextualContextService.SetRawText( newRawContext );
            TextualContextService.SetCaretIndex( newRawContext.Length );
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
            return info.Error != null;
        }

        public void Start()
        {
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
