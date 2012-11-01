using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CK.WordPredictor;
using CK.WordPredictor.Engines;
using CK.WordPredictor.Model;
using CommonServices;
using Moq;
using NUnit.Framework;

namespace CK.WordPredictorTest
{
    [TestFixture]
    public class TextualContextServiceTest
    {
        class NoPredictionPredictorEngine : IWordPredictorEngine
        {
            public IEnumerable<IWordPredicted> Predict( ITextualContextService textualService, int maxSuggestedWords )
            {
                if( textualService == null ) throw new ArgumentNullException( "textualService" );
                foreach( var w in textualService.Tokens )
                {
                    yield return new WeightlessWordPredicted( w.Value );
                }
            }

            public bool IsWeightedPrediction
            {
                get { return false; }
            }
        }

        [Test]
        public void When_Textual_Context_Token_Changed_The_Predictor_Word_List_Should_Be_Impacted()
        {
            var t = new DirectTextualContextService();
            var p = new Mock<IWordPredictorService>();

            t.PropertyChanged += ( sender, e ) =>
            {
                if( e.PropertyName == "CurrentToken" )
                {
                    ITextualContextService textualService = sender as ITextualContextService;
                    Assert.That( textualService, Is.Not.Null );
                    p.Setup( w => w.Words )
                        .Returns(
                            () => new WordPredictedCollection( new ObservableCollection<IWordPredicted>( new NoPredictionPredictorEngine().Predict( textualService, 10 ) ) ) )
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

        private static DirectTextualContextService SetupDirectTextualContextService( string keyToSend )
        {
            DirectTextualContextService sut = new DirectTextualContextService();
            Mock<ISendKeyCommandHandlerService> sendKey = new Mock<ISendKeyCommandHandlerService>();
            sendKey.Setup( e => e.SendKey( It.IsAny<string>() ) ).Raises( e => e.KeySent += ( sender, eventArgs ) => { }, new KeySentEventArgs( keyToSend ) ).Verifiable();

            sut.SendKeyService = sendKey.Object;
            return sut;
        }

        [Test]
        public void When_Input_Is_Received_The_Context_Should_Be_Updated()
        {
            string keyToSend = "J";
            bool propertyChangedMustBeRaised = false;

            DirectTextualContextService sut = SetupDirectTextualContextService( keyToSend );
            sut.PropertyChanged += ( sender, e ) =>
            {
                propertyChangedMustBeRaised = true;
            };

            sut.Start();
            sut.SendKeyService.SendKey( keyToSend );

            Assert.That( propertyChangedMustBeRaised == true );
            Assert.That( sut.CurrentToken.Value == keyToSend );
        }

        [Test]
        public void If_The_Plugin_Is_Not_Started_Or_Stopped_It_Should_Not_Handle_Keys_From_SendKeyService()
        {
            bool propertyChangedRaised = false;
            string keyToSend = "X";
            DirectTextualContextService sut = SetupDirectTextualContextService( keyToSend );
            sut.PropertyChanged += ( sender, e ) =>
            {
                propertyChangedRaised = true;
            };

            sut.SendKeyService.SendKey( keyToSend );
            Assert.That( propertyChangedRaised == false );
            Assert.That( sut.CurrentToken == null );

            sut.Start();
            sut.SendKeyService.SendKey( keyToSend );
            Assert.That( propertyChangedRaised == true );
            Assert.That( sut.CurrentToken.Value == keyToSend );

            propertyChangedRaised = false;
            sut.Stop();
            sut.SendKeyService.SendKey( keyToSend );
            Assert.That( propertyChangedRaised == false );
            Assert.That( sut.CurrentToken.Value == keyToSend );
        }


    }
}
