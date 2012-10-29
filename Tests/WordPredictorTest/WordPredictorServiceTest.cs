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
    public class WordPredictorServiceTest
    {
        class TokenStub : IToken
        {
            public TokenStub( string v )
            {
                Value = v;
            }
            public string Value { get; set; }
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

        [Test]
        public void When_Textual_Context_Token_Changed_Word_Predictor_Should_Return_A_List_Of_Words()
        {
            var t = new Mock<ITextualContextService>();
            var p = new Mock<IWordPredictorService>();

            var sample = new SampleTokenCollection( "Je", "suis", "un", "test" ) ;
            t.Setup( e => e.Tokens ).Returns( sample)
                .Raises( s => s.PropertyChanged += ( sender, e ) => { }, new PropertyChangedEventArgs( "Tokens" ) )
                .Verifiable();
            t.Setup( e => e.CurrentToken ).Returns( new TokenStub( "test" ) )
                .Raises( s => s.PropertyChanged += ( sender, e ) => { }, new PropertyChangedEventArgs( It.IsAny<string>() ) )
                .Verifiable();
            t.Setup( e => e.CurrentPosition ).Returns( CaretPosition.EndToken );
            t.Setup( e => e.CaretOffset ).Returns( 0 );
            t.Setup( e => e.CurrentTokenIndex ).Returns( 3 );

            t.Object.PropertyChanged += ( sender, e ) =>
            {
                if( e.PropertyName == "Tokens" )
                {
                    ITextualContextService textualService = sender as ITextualContextService;
                    Assert.That( textualService, Is.Not.Null );
                    p.Setup( w => w.Words )
                        .Returns(
                            () => new ReadOnlyObservableCollection<IWordPredicted>(
                            new ObservableCollection<IWordPredicted>( Predict( textualService.CurrentToken.Value ) ) ) )
                        .Verifiable();
                }
            };

            sample.Add( "buggué" );

            t.VerifyAll();
        }

        private IEnumerable<IWordPredicted> Predict( string token )
        {
            yield return new WordPredictedStub( token );
        }


        //[Test]
        //public void When_I_Predict_A_Word_I_Should_Receive_Event()
        //{
        //    string letters = "Bon";

        //    var w = new Mock<IWordPredictorService>();

        //    w.Setup( e => e.Predict( It.IsAny<string>() ) )
        //        .Raises(
        //            s => s.WordPredicted += ( sender, e ) => { },
        //            () => new WordPredictedEventArgs( letters, letters ) )
        //        .Verifiable();

        //    w.Object.WordPredicted += ( sender, e ) =>
        //    {
        //        Assert.That( e, Is.Not.Null );
        //        Assert.That( e.Letters, Is.EqualTo( letters ) );
        //        Assert.That( e.WordPredicted, Is.EqualTo( letters ) );
        //    };

        //    w.Object.Predict( letters );

        //    w.VerifyAll();
        //}

    }
}
