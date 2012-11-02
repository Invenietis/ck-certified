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
            var mKbContext = TestHelper.MockKeyboardContext( InKeyboardWordPredictor.CompatibilityKeyboardName, InKeyboardWordPredictor.PredictionZoneName );

            InKeyboardWordPredictor p = new InKeyboardWordPredictor()
            {
                Context = mKbContext.Object,
                Config = TestHelper.MockPluginConfigAccessor().Object
            };
            p.Start();

            IZone predictionZone = p.Context.Keyboards[InKeyboardWordPredictor.CompatibilityKeyboardName].Zones[InKeyboardWordPredictor.PredictionZoneName];
            Assert.That( predictionZone != null );
            Assert.That( InKeyboardWordPredictor.DefaultMaxDisplayedWords == 10 );
            Assert.That( predictionZone.Keys.Count == p.MaxDisplayedWords );

            mKbContext.VerifyAll();
        }

        [Test]
        public void When_Plugin_Is_Stopped_It_Must_Destroys_Prediction_Zone_Previously_Created()
        {
            var mKbContext = TestHelper.MockKeyboardContext( InKeyboardWordPredictor.CompatibilityKeyboardName, InKeyboardWordPredictor.PredictionZoneName );
            InKeyboardWordPredictor p = new InKeyboardWordPredictor()
            {
                Context = mKbContext.Object,
                Config = TestHelper.MockPluginConfigAccessor().Object
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
            var textualService = new DirectTextualContextService();
            // Predictor service plugin usage
            var predictorService = new WordPredictorService()
            {
                TextualContextService = textualService,
                PluginDirectoryPath = () => TestHelper.SybilleResourceFullPath,
                Config = TestHelper.MockPluginConfigAccessor().Object
            };

            // Mocking of IKeyboardContext
            var mKbContext = TestHelper.MockKeyboardContext( InKeyboardWordPredictor.CompatibilityKeyboardName, InKeyboardWordPredictor.PredictionZoneName );

            // The Plugin Under Test.
            var pluginSut = new InKeyboardWordPredictor()
            {
                WordPredictorService = predictorService,
                Context = mKbContext.Object,
                Config = TestHelper.MockPluginConfigAccessor().Object
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
            Assert.That( keys.Select( e => e.CurrentLayout.Current.Visible == true ).Count() == predictorService.Words.Count );

            mKbContext.VerifyAll();
        }

        [Test]
        public void There_Must_Be_Only_One_Command_Associated_With_The_Key_OnKeyPressed()
        {
            // Mocking of IKeyboardContext
            var mKbContext = TestHelper.MockKeyboardContext( InKeyboardWordPredictor.CompatibilityKeyboardName, InKeyboardWordPredictor.PredictionZoneName );

            // The Plugin Under Test.
            var pluginSut = new InKeyboardWordPredictor()
            {
                WordPredictorService = TestHelper.MockPredictorService().Object,
                Context = mKbContext.Object,
                Config = TestHelper.MockPluginConfigAccessor().Object
            };

            //TODO write test
        }
    }
}