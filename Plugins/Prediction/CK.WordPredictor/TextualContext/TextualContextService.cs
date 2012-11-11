using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{

    [Plugin( "{86777945-654D-4A56-B301-5E92B498A685}", PublicName = "TextualContext", Categories = new string[] { "Prediction", "Visual" } )]
    public class TextualContextService : IPlugin, ITextualContextService
    {
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ISendTextualContextService SendTextualContextService { get; set; }

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
            if( SendTextualContextService != null )
                SendTextualContextService.TextualContextSent += SendTextualContextService_TextualContextSent;
        }

        public void Stop()
        {
            if( SendTextualContextService != null )
                SendTextualContextService.TextualContextSent -= SendTextualContextService_TextualContextSent;
        }

        void SendTextualContextService_TextualContextSent( object sender, EventArgs e )
        {
            _rawContext = null;
            _tokenCollection.Clear();
            _caretIndex = 0;
            _tokenSeparatorIndexes = new int[0];

            OnPropertyChanged( "Tokens" );
            OnPropertyChanged( "RawContext" );
            OnPropertyChanged( "CurrentToken" );
            OnPropertyChanged( "CurrentTokenIndex" );
            OnPropertyChanged( "CurrentPosition" );
            OnPropertyChanged( "CaretOffset" );
        }

        int _caretIndex;
        string _rawContext;
        int[] _tokenSeparatorIndexes;
        TokenCollection _tokenCollection;

        public string RawContext
        {
            get { return _rawContext; }
        }

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

            OnPropertyChanged( "CurrentToken" );
            OnPropertyChanged( "CurrentTokenIndex" );
            OnPropertyChanged( "CurrentPosition" );
            OnPropertyChanged( "CaretOffset" );
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
            _rawContext = value;

            if( String.IsNullOrWhiteSpace( value ) )
            {
                _tokenSeparatorIndexes = new int[0];
                return;
            }

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

            OnPropertyChanged( "RawContext" );
            OnPropertyChanged( "Tokens" );
            OnPropertyChanged( "CurrentToken" );
        }


        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged( string propertyName )
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if( handler != null )
            {
                var e = new PropertyChangedEventArgs( propertyName );
                handler( this, e );
            }
        }
    }
}
