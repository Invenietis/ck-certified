using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Plugin;
using CK.Plugins.SendInput;
using CK.WordPredictor.Model;
using CommonServices;

namespace CK.WordPredictor
{

    [Plugin( "{86777945-654D-4A56-B301-5E92B498A685}", PublicName = "TextualContext", Categories = new string[] { "Prediction", "Visual" } )]
    public class TextualContextService : IPlugin, ITextualContextService
    {
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ICommandTextualContextService> CommandTextualContextService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ISendStringService> SendStringService { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ISendKeyCommandHandlerService> SendKeyService { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IPredictionTextAreaService> PredictionTextAreaService { get; set; }

        #region IPlugin Initialization

        public bool Setup( IPluginSetupInfo info )
        {
            _tokenCollection = new TokenCollection();
            _tokenSeparatorIndexes = new int[0];
            return info.Error != null;
        }

        public void Teardown()
        {
            _tokenCollection = null;
            _tokenSeparatorIndexes = null;
        }

        public void Start()
        {
            if( PredictionTextAreaService != null && PredictionTextAreaService.Service != null )
            {
                PredictionTextAreaService.Service.TextSent += OnPredictionAreaContentSent;
                PredictionTextAreaService.Service.PropertyChanged += OnPredictionAreaServicePropertyChanged;
            }
            PredictionTextAreaService.ServiceStatusChanged += OnPredictionAreaServiceStatusChanged;

            if( CommandTextualContextService != null && CommandTextualContextService.Service != null )
            {
                CommandTextualContextService.Service.TextualContextClear += OnTextualContextClear;
            }
            CommandTextualContextService.ServiceStatusChanged += OnCommandTextualContextServiceStatusChanged;

            if( SendKeyService != null && SendKeyService.Service != null )
            {
                SendKeyService.Service.KeySent += OnKeySent;
            }
            SendKeyService.ServiceStatusChanged += OnSendKeyServiceStatusChanged;

            if( SendStringService != null && SendStringService.Service != null )
            {
                SendStringService.Service.StringSent += OnStringSent;
            }
            SendStringService.ServiceStatusChanged += OnSendStringServiceStatusChanged;
        }

        public void Stop()
        {
            if( PredictionTextAreaService != null && PredictionTextAreaService.Service != null )
            {
                PredictionTextAreaService.Service.TextSent -= OnPredictionAreaContentSent;
                PredictionTextAreaService.Service.PropertyChanged -= OnPredictionAreaServicePropertyChanged;
            }
            PredictionTextAreaService.ServiceStatusChanged -= OnPredictionAreaServiceStatusChanged;

            if( CommandTextualContextService != null && CommandTextualContextService.Service != null )
            {
                CommandTextualContextService.Service.TextualContextClear -= OnTextualContextClear;
            }
            CommandTextualContextService.ServiceStatusChanged -= OnCommandTextualContextServiceStatusChanged;

            if( SendKeyService != null && SendKeyService.Service != null )
            {
                SendKeyService.Service.KeySent -= OnKeySent;
            }
            SendKeyService.ServiceStatusChanged -= OnSendKeyServiceStatusChanged;

            if( SendStringService != null && SendStringService.Service != null )
            {
                SendStringService.Service.StringSent -= OnStringSent;
            }
            SendStringService.ServiceStatusChanged -= OnSendStringServiceStatusChanged;
        }

        #endregion

        #region Event Registration

        protected virtual void OnKeySent( object sender, KeySentEventArgs e )
        {
            if( e.Key != null ) SetToken( e.Key );
        }

        protected virtual void OnStringSent( object sender, StringSentEventArgs e )
        {
            if( e.StringVal != null ) SetToken( e.StringVal );
        }

        void SetToken( string token )
        {
            string newRawContext = String.Concat( _rawContext, token );

            _caretIndex = newRawContext.Length;
            InternalSetRawText( newRawContext );

            NotifyPropertiesChanged( "CurrentToken", "Tokens", "CurrentTokenIndex", "CaretOffset", "CurrentPosition" );
        }

        protected virtual void OnTextualContextClear( object sender, EventArgs e )
        {
            _rawContext = null;
            _tokenCollection.Clear();
            _caretIndex = 0;
            _tokenSeparatorIndexes = new int[0];

            NotifyPropertiesChanged( "CurrentToken", "Tokens", "CurrentTokenIndex", "CaretOffset", "CurrentPosition" );
        }

        private void OnPredictionAreaServicePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "Text" )
            {
                SetRawText( PredictionTextAreaService.Service.Text );
            }
            if( e.PropertyName == "CaretIndex" )
            {
                SetCaretIndex( PredictionTextAreaService.Service.CaretIndex );
            }
        }

        private void OnPredictionAreaContentSent( object sender, PredictionAreaContentEventArgs e )
        {
            CommandTextualContextService.Service.ClearTextualContext();
            SendStringService.Service.SendString( e.Text );
        }

        #endregion

        #region ITextualContextService Implementation

        int _caretIndex;
        string _rawContext;
        int[] _tokenSeparatorIndexes;
        TokenCollection _tokenCollection;

        public ITokenCollection Tokens
        {
            get { return _tokenCollection; }
        }

        public int CurrentTokenIndex
        {
            get
            {
                int i = 0;
                while( i < _tokenSeparatorIndexes.Length )
                {
                    int wordIndex = _tokenSeparatorIndexes[i];
                    if( wordIndex > _caretIndex ) return i;
                    i++;
                }
                return i;
            }
        }

        public IToken CurrentToken
        {
            get { return _tokenCollection.Count == 0 ? null : _tokenCollection[CurrentTokenIndex]; }
        }

        /// <summary>
        /// aa gh|ijk lmn
        /// </summary>
        public int CaretOffset
        {
            get
            {
                if( _caretIndex == 0 ) return 0;

                if( _tokenSeparatorIndexes.Length == 0 )
                {
                    return _caretIndex;
                }
                else if( _tokenSeparatorIndexes.Length == 1 )
                {
                    int wordSeparatorIndex = _tokenSeparatorIndexes[0];
                    return wordSeparatorIndex > _caretIndex ? _caretIndex : _caretIndex - wordSeparatorIndex;
                }
                else
                {
                    int i = 1, 
                        previousWordIndex = 0,
                        wordIndex = 0;
                    while( i < _tokenSeparatorIndexes.Length )
                    {
                        wordIndex = _tokenSeparatorIndexes[i];
                        if( wordIndex > _caretIndex )
                        {
                            previousWordIndex = _tokenSeparatorIndexes[i - 1];
                            return _caretIndex - previousWordIndex;
                        }
                        i++;
                    }
                    return _caretIndex - wordIndex;
                }
            }
        }

        public CaretPosition CurrentPosition
        {
            get
            {
                if( CaretOffset == 0 )
                {
                    return CaretPosition.StartToken;
                }
                if( CurrentToken != null )
                {
                    if( CurrentToken.Value.Length > CaretOffset ) return CaretPosition.InsideToken;
                    if( CurrentToken.Value.Length == CaretOffset ) return CaretPosition.EndToken;
                    if( CurrentToken.Value.Length < CaretOffset ) return CaretPosition.OutsideToken;
                }
                return CaretPosition.EndToken;
            }
        }

        public void SetCaretIndex( int caretGlobalIndex )
        {
            _caretIndex = caretGlobalIndex;

            NotifyPropertiesChanged( "CurrentToken", "CurrentTokenIndex", "CaretOffset", "CurrentPosition" );
        }

        /// <summary>
        /// return a 
        /// </summary>
        string[] Normalization( string context )
        {
            return context.Split( new char[] { ' ' } );
        }

        // WORD1  WORD2 WORD3
        public void SetRawText( string value )
        {
            if( value == _rawContext ) return;

            InternalSetRawText( value );

            NotifyPropertiesChanged( "CurrentToken", "Tokens" );
        }

        private void InternalSetRawText( string value )
        {

            _rawContext = value;

            if( String.IsNullOrWhiteSpace( value ) )
            {
                _tokenSeparatorIndexes = new int[0];
                _tokenCollection.Clear( false );
            }
            else
            {

                string[] tokens = Normalization( value ); ;
                if( tokens.Length > 1 )
                {
                    _tokenSeparatorIndexes = new int[tokens.Length - 1];
                    _tokenSeparatorIndexes[0] = tokens[0].Length + 1;

                    for( int i = 1; i < _tokenSeparatorIndexes.Length; i++ )
                    {
                        // + 1 for whitespace
                        _tokenSeparatorIndexes[i] = _tokenSeparatorIndexes[i - 1] + 1 + tokens[i].Length; // The index of the whitespace
                    }
                }

                _tokenCollection.Clear( false );
                _tokenCollection.AddRange( tokens, false );

            }
        }

        #endregion

        internal static bool IsResetContextToken( string token )
        {
            if( String.IsNullOrWhiteSpace( token ) ) return false;

            return Char.IsPunctuation( token.Trim()[0] );
        }

        internal static bool IsTokenSeparator( string token )
        {
            if( String.IsNullOrEmpty( token ) ) return false;
            if( String.IsNullOrEmpty( token.Trim() ) ) return true;

            char c = token[0];
            return Char.IsWhiteSpace( c ) || Char.IsSeparator( c );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged( string propertyName )
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if( handler != null )
            {
                var e = new PropertyChangedEventArgs( propertyName );
                handler( this, e );
            }
        }

        void OnCommandTextualContextServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == RunningStatus.Stopping )
            {
                CommandTextualContextService.Service.TextualContextClear -= OnTextualContextClear;
            }
            if( e.Current == RunningStatus.Starting )
            {
                CommandTextualContextService.Service.TextualContextClear += OnTextualContextClear;
            }
        }

        private void OnPredictionAreaServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == RunningStatus.Stopping )
            {
                PredictionTextAreaService.Service.TextSent -= OnPredictionAreaContentSent;
                PredictionTextAreaService.Service.PropertyChanged -= OnPredictionAreaServicePropertyChanged;
            }
            if( e.Current == RunningStatus.Starting )
            {
                PredictionTextAreaService.Service.TextSent += OnPredictionAreaContentSent;
                PredictionTextAreaService.Service.PropertyChanged += OnPredictionAreaServicePropertyChanged;
            }
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

        private void NotifyPropertiesChanged( params string[] properties )
        {
            foreach( string p in properties )
            {
                OnPropertyChanged( p );
            }
        }

    }
}
