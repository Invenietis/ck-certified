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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CK.Plugin;
using CommonServices;
using CK.Plugins.SendInput;

namespace WordPredictorTest
{
    [TestFixture]
    public class InKeyboardWordPredictorTests
    {
        [Test]
        public void When_Plugin_Is_Started_It_Must_Create_A_Prediction_Zone_In_The_Context()
        {
            var mKbContext = TestHelper.MockKeyboardContext( InKeyboardWordPredictor.CompatibilityKeyboardName, InKeyboardWordPredictor.PredictionZoneName );

            InKeyboardWordPredictor p = new InKeyboardWordPredictor()
            {
                Feature = TestHelper.MockFeature( 10 ).Object,
                Context = mKbContext.Object
            };
            p.Start();

            IZone predictionZone = p.Context.Keyboards[InKeyboardWordPredictor.CompatibilityKeyboardName].Zones[InKeyboardWordPredictor.PredictionZoneName];
            Assert.That( predictionZone != null );
            //Assert.That( InKeyboardWordPredictor.DefaultMaxDisplayedWords == 10 );
            Assert.That( predictionZone.Keys.Count == p.Feature.MaxSuggestedWords );

            mKbContext.VerifyAll();
        }

        [Test]
        public void When_Plugin_Is_Stopped_It_Must_Destroys_Prediction_Zone_Previously_Created()
        {
            var mKbContext = TestHelper.MockKeyboardContext( InKeyboardWordPredictor.CompatibilityKeyboardName, InKeyboardWordPredictor.PredictionZoneName );
            InKeyboardWordPredictor p = new InKeyboardWordPredictor()
            {
                Feature = TestHelper.MockFeature( 10 ).Object,
                Context = mKbContext.Object,
                WordPredictorService = TestHelper.MockPredictorService().MockServiceWrapper()
            };

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
            var textualService = new SimpleTextualContextService()
            {
                SendKeyService = ServiceHelper.MockServiceWrapper<ISendKeyCommandHandlerService>(),
                SendStringService = ServiceHelper.MockServiceWrapper<ISendStringService>()
            };
            // Predictor service plugin usage

            SybilleWordPredictorService.PluginDirectoryPath = TestHelper.SybilleResourceFullPath;
            var predictorService = new SybilleWordPredictorService()
            {
                Feature = TestHelper.MockFeature( 10 ).Object,
                TextualContextService = textualService,
            };

            // Mocking of IKeyboardContext
            var mKbContext = TestHelper.MockKeyboardContext( InKeyboardWordPredictor.CompatibilityKeyboardName, InKeyboardWordPredictor.PredictionZoneName );

            // The Plugin Under Test.
            var pluginSut = new InKeyboardWordPredictor()
            {
                Feature = TestHelper.MockFeature( 10 ).Object,
                WordPredictorService = predictorService.MockServiceWrapper<IWordPredictorService>(),
                Context = mKbContext.Object
            };

            // Start all depending plugins
            textualService.Start();
            predictorService.Setup( null );
            predictorService.Start();
            pluginSut.Start();
            Task.WaitAll( predictorService.AsyncEngineContinuation );
            // Start test. When a token is inserted into the textual service, it will triggers the predictor service to make a prediction.
            textualService.SetToken( "J" );
            Assert.That( predictorService.Words.Count > 0 );

            // We need to assert that the SUT correctly creates Keys into the Prediction Zone, according to its specs.

            // Test
            var keys = pluginSut.Context.Keyboards[InKeyboardWordPredictor.CompatibilityKeyboardName].Zones[InKeyboardWordPredictor.PredictionZoneName].Keys;
            Assert.That( keys.Select( e => e.CurrentLayout.Current.Visible == true ).Count() == predictorService.Words.Count );

            mKbContext.VerifyAll();
        }

        [Test]
        public void There_Must_Be_Only_One_Command_Associated_With_The_Key_OnKeyPressed()
        {
            // Mocking of IKeyboardContext
            var mKbContext = TestHelper.MockKeyboardContext( InKeyboardWordPredictor.CompatibilityKeyboardName, InKeyboardWordPredictor.PredictionZoneName );

            var wordList = new ObservableCollection<IWordPredicted>();
            // The Plugin Under Test.
            var mkWordPredictor = TestHelper.MockPredictorService( wordList );

            var pluginSut = new InKeyboardWordPredictor()
            {
                Feature = TestHelper.MockFeature( 10 ).Object,
                WordPredictorService = mkWordPredictor.MockServiceWrapper<IWordPredictorService>(),
                Context = mKbContext.Object
            };

            pluginSut.Start();
            var predictedWord = new Mock<IWordPredicted>();
            predictedWord.Setup( e => e.Word ).Returns( "OneWord" );
            wordList.Add( predictedWord.Object );
            {
                IKeyCollection keyCollection = pluginSut.Context.CurrentKeyboard.Zones[InKeyboardWordPredictor.PredictionZoneName].Keys;
                Assert.That( keyCollection[0].CurrentLayout.Current.Visible, Is.True );
                Assert.That( keyCollection[1].CurrentLayout.Current.Visible, Is.False );

                Assert.That( keyCollection[0].Current.OnKeyPressedCommands, Is.Not.Null );
                Assert.That( keyCollection[0].Current.OnKeyPressedCommands.Commands, Is.Not.Null );
                Assert.That( keyCollection[0].Current.OnKeyPressedCommands.Commands.Count == 1 );
                Assert.That( keyCollection[0].Current.OnKeyPressedCommands.Commands[0] == "sendPredictedWord:oneword" );
            }
            wordList.Add( predictedWord.Object );
            {
                IKeyCollection keyCollection = pluginSut.Context.CurrentKeyboard.Zones[InKeyboardWordPredictor.PredictionZoneName].Keys;
                Assert.That( keyCollection[0].CurrentLayout.Current.Visible, Is.True );
                Assert.That( keyCollection[1].CurrentLayout.Current.Visible, Is.True );

                Assert.That( keyCollection[0].Current.OnKeyPressedCommands, Is.Not.Null );
                Assert.That( keyCollection[0].Current.OnKeyPressedCommands.Commands, Is.Not.Null );
                Assert.That( keyCollection[0].Current.OnKeyPressedCommands.Commands.Count == 1 );
                Assert.That( keyCollection[0].Current.OnKeyPressedCommands.Commands[0] == "sendPredictedWord:oneword" );

                Assert.That( keyCollection[1].Current.OnKeyPressedCommands, Is.Not.Null );
                Assert.That( keyCollection[1].Current.OnKeyPressedCommands.Commands, Is.Not.Null );
                Assert.That( keyCollection[1].Current.OnKeyPressedCommands.Commands.Count == 1 );
                Assert.That( keyCollection[1].Current.OnKeyPressedCommands.Commands[0] == "sendPredictedWord:oneword" );
            }
        }
    }
}