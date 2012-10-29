using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CK.WordPredictor.Model;
using Moq;
using NUnit.Framework;

namespace WordPredictorTest
{
    [TestFixture]
    public class TextualContextServiceTest
    {
        class TokenStub : IToken
        {
            public TokenStub( string v )
            {
                Value = v;
            }
            public string Value { get; set; }
        }
        
        class WordPredictedStub : IWordPredicted
        {
            public WordPredictedStub( string w )
            {
                Word = w;
            }
            public string Word { get; private set; }

            public double Weight
            {
                get { return 0; }
            }
        }

        class SampleTokenCollection : ITokenCollection
        {
            IList<IToken> _token;

            public void Add( string token )
            {
                var ts = new TokenStub( token );
                _token.Add( ts );
                if( CollectionChanged != null )
                    CollectionChanged( this, new System.Collections.Specialized.NotifyCollectionChangedEventArgs( System.Collections.Specialized.NotifyCollectionChangedAction.Add, ts ) );
            }

            public SampleTokenCollection( params string[] tokens )
            {
                _token = new List<IToken>( tokens.Select( t => new TokenStub( t ) ) );
            }

            public event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

            #region IReadOnlyList<IToken> Members

            public int IndexOf( object item )
            {
                return _token.IndexOf( (IToken)item );
            }

            public IToken this[int index]
            {
                get { return _token[index]; }
                set { _token[index] = value; }
            }

            #endregion

            #region IReadOnlyCollection<IToken> Members

            public bool Contains( object item )
            {
                return _token.Contains( item );
            }

            public int Count
            {
                get { return _token.Count; }
            }

            #endregion

            #region IEnumerable<IToken> Members

            public IEnumerator<IToken> GetEnumerator()
            {
                return _token.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _token.GetEnumerator();
            }

            #endregion
        }

        class NoPredictionPredictorEngine : IWordPredictorEngine
        {
            public IEnumerable<IWordPredicted> Predict( ITextualContextService textualService )
            {
                if( textualService == null ) throw new ArgumentNullException( "textualService" );
                foreach( var w in textualService.Tokens )
                {
                    yield return new WordPredictedStub( w.Value );
                }
            }
        }

        class LiveTextualContextService : ITextualContextService
        {
            public event PropertyChangedEventHandler PropertyChanged;

            SampleTokenCollection _tokenCollection;

            public LiveTextualContextService( string token )
            {
                _tokenCollection = new SampleTokenCollection( token );
                _position = CaretPosition.EndToken;
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

            public void SetToken( string token )
            {
                if( token == "." )
                {
                    _tokenCollection = new SampleTokenCollection();
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
                        _tokenCollection = new SampleTokenCollection( token );
                    }
                    else //We continue in the same context.
                    {
                        _tokenCollection[CurrentTokenIndex] = new TokenStub( CurrentToken.Value + token );
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
        }

        [Test]
        public void When_Textual_Context_Token_Changed_The_Predictor_Word_List_Should_Be_Impacted()
        {
            var t = new LiveTextualContextService( "J" );
            var p = new Mock<IWordPredictorService>();
            p.Setup( e => e.CurrentEngine ).Returns( new NoPredictionPredictorEngine() );

            t.PropertyChanged += ( sender, e ) =>
            {
                if( e.PropertyName == "CurrentToken" )
                {
                    ITextualContextService textualService = sender as ITextualContextService;
                    Assert.That( textualService, Is.Not.Null );
                    p.Setup( w => w.Words )
                        .Returns(
                            () => new ReadOnlyObservableCollection<IWordPredicted>(
                            new ObservableCollection<IWordPredicted>( p.Object.CurrentEngine.Predict( textualService ) ) ) )
                        .Verifiable();
                }
            };

            t.SetToken( "e" );
            Assert.That( p.Object.Words.FirstOrDefault().Word == "Je" );

            t.SetToken( " " );
            Assert.That( p.Object.Words.Count == 2 );
            Assert.That( p.Object.Words[1].Word == String.Empty );

            t.SetToken( "sui" );
            Assert.That( p.Object.Words.Count == 2 );
            Assert.That( p.Object.Words[0].Word == "Je" );
            Assert.That( p.Object.Words[1].Word == "sui" );

            t.SetToken( "s" );
            Assert.That( p.Object.Words.Count == 2 );
            Assert.That( p.Object.Words[0].Word == "Je" );
            Assert.That( p.Object.Words[1].Word == "suis" );

            t.SetToken( " " );
            t.SetToken( "buggué" );
            Assert.That( p.Object.Words.Count == 3 );
            Assert.That( p.Object.Words[0].Word == "Je" );
            Assert.That( p.Object.Words[1].Word == "suis" );
            Assert.That( p.Object.Words[2].Word == "buggué" );

            t.SetToken( "." );
            Assert.That( p.Object.Words.Count == 0, "dot token reset the context." );

            p.VerifyAll();
        }
    }
}
