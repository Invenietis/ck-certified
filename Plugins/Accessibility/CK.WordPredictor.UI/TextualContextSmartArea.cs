using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CK.Plugin;
using CK.WordPredictor.Model;
using CK.WordPredictor.UI.Models;
using CK.WordPredictor.UI.ViewModels;

namespace CK.WordPredictor.UI
{
    [Plugin( "{69E910CC-C51B-4B80-86D3-E86B6C668C61}", PublicName = "Word Prediction UI - TextualContextSmartArea", Categories = new string[] { "Prediction", "Visual" } )]
    public class TextualContextSmartArea : IPlugin, ITextualContextService
    {
        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            TextualContextAreaViewModel vm = new TextualContextAreaViewModel( this );
            TextualContextSmartAreaWindow window = new TextualContextSmartAreaWindow( vm );
            window.Width = 600;
            window.Height = 200;
            window.Show();

            TextualContextPreviewViewModel previewVm = new TextualContextPreviewViewModel( this );
            TextualContextPreview preview = new TextualContextPreview( previewVm );
            preview.Width = 600;
            preview.Height = 200; 
            preview.Show();
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
        }

        int _caretIndex;

        int[] _tokenSeparatorIndexes = new int[0];

        TokenCollection _tokenCollection = new TokenCollection();

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
            get { return _tokenCollection[CurrentTokenIndex]; }
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
                    int i = 0, 
                        previousWordIndex = 0;
                    while( i < _tokenSeparatorIndexes.Length )
                    {
                        int wordIndex = _tokenSeparatorIndexes[i];
                        if( wordIndex > _caretIndex )
                        {
                            if( i > 0 )
                            {
                                previousWordIndex = _tokenSeparatorIndexes[i - 1];
                                return _caretIndex - previousWordIndex;
                            }
                            else return _caretIndex;
                        }
                        i++;
                    }
                    return _caretIndex - previousWordIndex;
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


        internal void SetCaretIndex( int caretGlobalIndex )
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
        internal void SetText( string value )
        {
            if( String.IsNullOrWhiteSpace( value ) )
            {
                _tokenSeparatorIndexes = new int[0];
                return;
            }

            string[] tokens = Normalization( value ); ;
            if( tokens.Length > 1 )
            {
                //int j = 0;
                //for( int i = 0; i < value.Length; ++i )
                //{
                //    if( value[i] == ' ' ) _tokenSeparatorIndexes[j++] = i;
                //}

                _tokenSeparatorIndexes = new int[tokens.Length - 1];
                _tokenSeparatorIndexes[0] = tokens[0].Length + 1;

                for( int i = 1; i < _tokenSeparatorIndexes.Length; i++ )
                {
                    // + 1 for whitespace
                    _tokenSeparatorIndexes[i] = _tokenSeparatorIndexes[i - 1] + 1 + tokens[i].Length; // The index of the whitespace
                }
            }

            _tokenCollection = new TokenCollection( tokens );

            OnPropertyChanged( "Tokens" );

            //// The Current Token Index is smaller than the number of token: It is an insertion.
            //if( CurrentTokenIndex < tokens.Length )
            //{
            //    //A new token has been added to the list. Impact the model. 
            //    //1. Found the insertion point
            //    int insertionPoint = 0;

            //    // The CurrentToken is the current word where the Caret is.
            //    // If the caret is at the begin of a token, InsertBefore. At the end, InsertAfter.
            //    if( CurrentPosition == CaretPosition.InsideToken ) throw new NotSupportedException();
            //    insertionPoint = CurrentPosition == CaretPosition.StartToken ? CurrentTokenIndex : CurrentTokenIndex + 1;


            //    string token = null;

            //    // The Caret is correctly positionned to the end of the word. 
            //    // So its offset should be the length of the token 

            //    if( CurrentPosition == CaretPosition.EndToken ) // For the first insertion, the caret is in startposition
            //    {
            //        token = tokens[CurrentTokenIndex];
            //        if( token.Length == 0 )
            //        {
            //            _tokenCollection.InsertAt( insertionPoint, String.Empty );
            //        }
            //        else
            //        {
            //            _tokenCollection[insertionPoint] = new Token( token );
            //        }
            //    }
            //}
            //if( CurrentTokenIndex > tokens.Length ) //Deletion
            //{

            //}

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
