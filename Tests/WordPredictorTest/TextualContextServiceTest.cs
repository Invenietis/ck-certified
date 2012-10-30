using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CK.WordPredictor.Model;
using Moq;
using NUnit.Framework;
using WordPredictor;

namespace WordPredictorTest
{
    [TestFixture]
    public class TextualContextServiceTest
    {

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

        [Test]
        public void When_Textual_Context_Token_Changed_The_Predictor_Word_List_Should_Be_Impacted()
        {
            var t = new DirectTextualContextService();
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

            t.SetToken( "J" );
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
