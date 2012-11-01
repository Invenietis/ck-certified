using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.WordPredictor.Model;
using CK.WordPredictor.UI;
using Moq;
using NUnit.Framework;
using System.ComponentModel;
using System.Collections.Specialized;
using CK.WordPredictor;

namespace WordPredictorTest
{
    [TestFixture]
    public class InKeyboardWordPredictorTests
    {
        [Test]
        public void When_Plugin_Is_Started_It_Must_Create_A_Prediction_Zone_In_The_Context()
        {
            var mKbContext = MockKeyboardContext();

            InKeyboardWordPredictor p = new InKeyboardWordPredictor();
            p.Context = mKbContext.Object;
            p.Start();

            Assert.That( p.Context.Keyboards[InKeyboardWordPredictor.CompatibilityKeyboardName].Zones[InKeyboardWordPredictor.PredictionZoneName] != null );
            mKbContext.VerifyAll();
        }

        [Test]
        public void When_Plugin_Is_Stopped_It_Must_Destroys_Prediction_Zone_Previously_Created()
        {
            var mKbContext = MockKeyboardContext();
            InKeyboardWordPredictor p = new InKeyboardWordPredictor();
            p.Context = mKbContext.Object;
            p.Start();
            Assert.That( p.Context.Keyboards[InKeyboardWordPredictor.CompatibilityKeyboardName].Zones[InKeyboardWordPredictor.PredictionZoneName] != null );

            p.Stop();

            Assert.That( p.Context.Keyboards[InKeyboardWordPredictor.CompatibilityKeyboardName].Zones[InKeyboardWordPredictor.PredictionZoneName] == null );
            mKbContext.VerifyAll();
        }

        [Test]
        public void When_A_Word_Is_Predicted_It_Must_Appears_In_Prediction_Zone()
        {
            // Texual service plugin usage
            var textualService = new DirectTextualContextService();
            // Predictor service plugin usage
            var predictorService = new WordPredictorService()
            {
                TextualContextService = textualService,
                PluginDirectoryPath = () => TestHelper.SybilleResourceFullPath,
                Config = TestHelper.MockPluginConfigAccessor().Object
            };

            // Mocking of IKeyboardContext
            var mKbContext = MockKeyboardContext( ( mockedZone ) =>
            {
                var keyCollectionMock = new Mock<IKeyCollection>();
                keyCollectionMock.Setup( e => e.Create() ).Verifiable();
                keyCollectionMock.Setup( e => e.Count ).Returns( () =>
                {
                    return predictorService.Words.Count;
                } );

                mockedZone.Setup( e => e.Keys ).Returns( keyCollectionMock.Object );
            } );


            // The Plugin Under Test.
            var pluginSut = new InKeyboardWordPredictor()
            {
                WordPredictorService = predictorService,
                Context = mKbContext.Object
            };

            // Start all depending plugins
            textualService.Start();
            predictorService.Start();
            pluginSut.Start();

            // Start test. When a token is inserted into the textual service, it will triggers the predictor service to make a prediction.
            textualService.SetToken( "J" );
            Assert.That( predictorService.Words.Count > 0 );

            // We need to assert that the SUT correctly creates Keys into the Prediction Zone, according to its specs.

            // Test
            var keys = pluginSut.Context.Keyboards[InKeyboardWordPredictor.CompatibilityKeyboardName].Zones[InKeyboardWordPredictor.PredictionZoneName].Keys;
            Assert.That( keys.Count > 0 );
        }


        private static Mock<IKeyboardContext> MockKeyboardContext( Action<Mock<IZone>> zoneMockConfiguration = null )
        {
            var mzCollection = new Mock<IZoneCollection>();
            var mkb = new Mock<IKeyboard>();
            var mkbCollection = new Mock<IKeyboardCollection>();
            var mKbContext = new Mock<IKeyboardContext>();
            var mZone = new Mock<IZone>();

            if( zoneMockConfiguration != null )
            {
                zoneMockConfiguration( mZone );
            }
            mZone.Setup( e => e.Destroy() ).Callback( () =>
            {
                mzCollection
                    .Setup( x => x[It.IsAny<string>()] ).Returns( () => null );
            } );
            mzCollection
                .Setup( e => e.Create( It.IsAny<string>() ) )
                .Verifiable();
            mzCollection
                .Setup( e => e[It.IsAny<string>()] )
                .Callback<string>( ( z ) => Assert.That( z == InKeyboardWordPredictor.PredictionZoneName ) )
                .Returns( mZone.Object );

            mkb
                .Setup( e => e.Zones )
                .Returns( mzCollection.Object )
                .Verifiable();

            mkbCollection
                .Setup( e => e[It.IsAny<string>()] )
                .Callback<string>( z => Assert.That( z == InKeyboardWordPredictor.CompatibilityKeyboardName ) )
                .Returns( mkb.Object )
                .Verifiable();

            mKbContext.Setup( e => e.CurrentKeyboard ).Returns( mkb.Object );

            mKbContext
                .Setup( e => e.Keyboards )
                .Returns( mkbCollection.Object )
                .Verifiable();

            return mKbContext;
        }

    }
}