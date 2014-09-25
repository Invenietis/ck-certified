#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor\TextualContext\TextualContextService.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using CK.Core;
using CK.InputDriver;
using CK.Plugin;
using CK.Plugins.SendInputDriver;
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

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ISendKeyCommandHandlerService> SendKeyService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPredictionTextAreaService> PredictionTextAreaService { get; set; }

        public event EventHandler TextualContextChanging;

        public event EventHandler TextualContextChanged;

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
                PredictionTextAreaService.Service.PredictionAreaTextSent += OnPredictionAreaContentSent;
                PredictionTextAreaService.Service.PredictionAreaContentChanged += OnPredictionAreaServicePropertyChanged;
            }
            PredictionTextAreaService.ServiceStatusChanged += OnPredictionAreaServiceStatusChanged;

            if( CommandTextualContextService != null && CommandTextualContextService.Service != null )
            {
                CommandTextualContextService.Service.TextualContextClear += OnTextualContextClear;
            }
            CommandTextualContextService.ServiceStatusChanged += OnCommandTextualContextServiceStatusChanged;

            if( SendKeyService != null && SendKeyService.Service != null )
            {
                SendKeyService.Service.KeySent += OnOldKeySent;
            }
            SendKeyService.ServiceStatusChanged += OnSendKeyServiceStatusChanged;

            if( SendStringService != null && SendStringService.Service != null )
            {
                SendStringService.Service.KeySent += OnKeySent;
                SendStringService.Service.StringSent += OnStringSent;
            }
            SendStringService.ServiceStatusChanged += OnSendStringServiceStatusChanged;
        }

        public void Stop()
        {
            if( PredictionTextAreaService.Status == InternalRunningStatus.Starting )
            {
                PredictionTextAreaService.Service.PredictionAreaTextSent -= OnPredictionAreaContentSent;
                PredictionTextAreaService.Service.PredictionAreaContentChanged -= OnPredictionAreaServicePropertyChanged;
            }
            PredictionTextAreaService.ServiceStatusChanged -= OnPredictionAreaServiceStatusChanged;

            if( CommandTextualContextService.Status == InternalRunningStatus.Starting )
            {
                CommandTextualContextService.Service.TextualContextClear -= OnTextualContextClear;
            }
            CommandTextualContextService.ServiceStatusChanged -= OnCommandTextualContextServiceStatusChanged;

            if( SendKeyService.Status == InternalRunningStatus.Started )
            {
                SendKeyService.Service.KeySent -= OnOldKeySent;
            }
            SendKeyService.ServiceStatusChanged -= OnSendKeyServiceStatusChanged;

            if( SendStringService.Status == InternalRunningStatus.Started )
            {
                SendStringService.Service.StringSent -= OnStringSent;
                SendStringService.Service.KeySent -= OnKeySent;
            }
            SendStringService.ServiceStatusChanged -= OnSendStringServiceStatusChanged;
        }

        #endregion

        class ChangeWrapper : IDisposable
        {
            TextualContextService _service;

            public ChangeWrapper( TextualContextService service )
            {
                _service = service;
                if( _service.TextualContextChanging != null )
                    _service.TextualContextChanging( this, EventArgs.Empty );
            }

            public void Dispose()
            {
                if( _service.TextualContextChanged != null )
                    _service.TextualContextChanged( this, EventArgs.Empty );
            }
        }

        #region Event Registration

        protected virtual void OnOldKeySent( object sender, KeySentEventArgs e )
        {
            if( !PredictionTextAreaService.Service.IsDriven )
            {
                if( e.Key == "{BACKSPACE}" && _rawContext != null && _rawContext.Length > 0 )
                {
                    var raw = _rawContext.Substring( 0, _rawContext.Length - 1 );
                    _caretIndex = raw.Length;
                    SetRawText( raw );
                }
                else if( e.Key == "{ENTER}" )
                {
                    ClearContext();
                }
            }
        }

        protected virtual void OnKeySent( object sender, NativeKeySentEventArgs e )
        {
            if( !PredictionTextAreaService.Service.IsDriven )
            {
                if( e.Key == Native.KeyboardKeys.Space && _rawContext != null && _rawContext.Length > 0 )
                {
                    var raw = _rawContext.Substring( 0, _rawContext.Length - 1 );
                    _caretIndex = raw.Length;
                    SetRawText( raw );
                }
                else if( e.Key == Native.KeyboardKeys.Enter )
                {
                    ClearContext();
                }
            }
        }

        protected virtual void OnStringSent( object sender, StringSentEventArgs e )
        {
            using( PredictionLogger.Instance.OpenTrace().Send( "OnStringSent" ) )
            {
                string val = e.StringVal;
                if( val != null )
                {
                    PredictionLogger.Instance.Trace().Send( val );
                    SetToken( e.StringVal );
                }
            }
        }

        protected virtual void OnTextualContextClear( object sender, EventArgs e )
        {
            ClearContext();
        }

        protected virtual void OnPredictionAreaServicePropertyChanged( object sender, PredictionAreaContentEventArgs e )
        {
            using( new ChangeWrapper( this ) )
            {
                PredictionLogger.Instance.Trace().Send( "CaretIndex {0}", e.CaretIndex );

                _caretIndex = e.CaretIndex;

                InternalSetRawText( e.Text );
            }
        }

        protected virtual void OnPredictionAreaContentSent( object sender, PredictionAreaContentEventArgs e )
        {
            CommandTextualContextService.Service.ClearTextualContext();
            SendStringService.Service.SendString( e.Text );
        }

        #endregion

        private void SetToken( string token )
        {
            using( new ChangeWrapper( this ) )
            {
                string newRawContext = String.Concat( _rawContext, token );

                _caretIndex = newRawContext.Length;
                InternalSetRawText( newRawContext );
            }
        }

        private void ClearContext()
        {
            using( new ChangeWrapper( this ) )
            {
                _rawContext = null;
                _tokenCollection.Clear();
                _caretIndex = 0;
                _tokenSeparatorIndexes = new int[0];
            }
        }


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
                    int wordSeparatorIndex = _tokenSeparatorIndexes[i];
                    if( wordSeparatorIndex > _caretIndex ) return i;
                    i++;
                }
                return i;
            }
        }

        public IToken CurrentToken
        {
            get
            {
                if( _tokenCollection.Count == 0 ) return null;
                // We are outside a token (on a whitespace and at the end of the phrase)
                if( CurrentTokenIndex >= _tokenCollection.Count ) return null;

                return _tokenCollection[CurrentTokenIndex];
            }
        }

        /// <summary>
        /// aa gh|ijk lmn
        /// In this case, returns "2", the caret offset has the token as start reference
        /// </summary>
        public int CaretOffset
        {
            get
            {
                if( _caretIndex == 0 ) return 0;

                if( CurrentTokenIndex == 0 ) return _caretIndex;

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
                int offset = CaretOffset;
                IToken curToken = CurrentToken;

                if( offset == 0 && curToken == null )
                {
                    return CaretPosition.OutsideToken;
                }

                if( offset == 0 )
                {
                    return CaretPosition.StartToken;
                }

                if( curToken != null )
                {
                    if( curToken.Value.Length > offset ) return CaretPosition.InsideToken;
                    if( curToken.Value.Length == offset ) return CaretPosition.EndToken;
                }

                return CaretPosition.OutsideToken;
            }
        }

        static char[] _separators = new char[] { ' ' };

        /// <summary>
        /// Splits the context (seperates the different words)
        /// </summary>
        string[] Normalization( string context )
        {
            return context.Split( _separators );
        }

        // WORD1  WORD2 WORD3
        public void SetRawText( string value )
        {
            using( new ChangeWrapper( this ) )
            {
                if( value == _rawContext ) return;

                InternalSetRawText( value );
            }
        }

        private void InternalSetRawText( string value )
        {

            _rawContext = value;
            _tokenCollection.Clear( false );

            if( String.IsNullOrWhiteSpace( value ) )
            {
                _tokenSeparatorIndexes = new int[0];
            }
            else
            {
                string[] tokens = Normalization( value );
                if( tokens.Length == 1 )
                {
                    _tokenCollection.Add( tokens[0] );
                }
                if( tokens.Length > 1 )
                {
                    _tokenSeparatorIndexes = new int[tokens.Length - 1];
                    _tokenSeparatorIndexes[0] = tokens[0].Length + 1;

                    for( int i = 1; i < _tokenSeparatorIndexes.Length; i++ )
                    {
                        // + 1 for whitespace
                        _tokenSeparatorIndexes[i] = _tokenSeparatorIndexes[i - 1] + 1 + tokens[i].Length; // The index of the whitespace
                    }
                    if( tokens[tokens.Length - 1] == String.Empty )
                    {
                        Array.Resize( ref tokens, tokens.Length - 1 );
                        _tokenCollection.AddRange( tokens );
                    }
                    else _tokenCollection.AddRange( tokens );
                }
            }
        }

        #endregion

        #region Helpers
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

        #endregion

        #region Service status change handling

        void OnCommandTextualContextServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current <= InternalRunningStatus.Stopping )
            {
                CommandTextualContextService.Service.TextualContextClear -= OnTextualContextClear;
            }
            if( e.Current == InternalRunningStatus.Starting )
            {
                CommandTextualContextService.Service.TextualContextClear += OnTextualContextClear;
            }
        }

        private void OnPredictionAreaServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current <= InternalRunningStatus.Stopping )
            {
                PredictionTextAreaService.Service.PredictionAreaTextSent -= OnPredictionAreaContentSent;
                PredictionTextAreaService.Service.PredictionAreaContentChanged -= OnPredictionAreaServicePropertyChanged;
            }
            if( e.Current == InternalRunningStatus.Starting )
            {
                PredictionTextAreaService.Service.PredictionAreaTextSent += OnPredictionAreaContentSent;
                PredictionTextAreaService.Service.PredictionAreaContentChanged += OnPredictionAreaServicePropertyChanged;
            }
        }

        void OnSendStringServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current <= InternalRunningStatus.Stopping )
            {
                SendStringService.Service.KeySent -= OnKeySent;
                SendStringService.Service.StringSent -= OnStringSent;
            }
            if( e.Current == InternalRunningStatus.Starting )
            {
                SendStringService.Service.KeySent += OnKeySent;
                SendStringService.Service.StringSent += OnStringSent;
            }
        }

        void OnSendKeyServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current <= InternalRunningStatus.Stopping )
            {
                SendKeyService.Service.KeySent -= OnOldKeySent;
            }
            if( e.Current == InternalRunningStatus.Starting )
            {
                SendKeyService.Service.KeySent += OnOldKeySent;
            }
        }

        #endregion
    }
}
