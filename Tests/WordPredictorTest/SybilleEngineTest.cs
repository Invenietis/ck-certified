using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CK.Predictor;
using CK.Predictor.Model;
using NUnit.Framework;

namespace WordPredictorTest
{
    [TestFixture]
    public class SybilleEngineTest
    {
        public static string ResourceFullPath = @"F:\Users\Cedric\Documents\Dev\__Dev4\Civikey\ck-certified\Plugins\Accessibility\WordPredictor\Sybille";

        [Test]
        public void Sybille_Dictionnary_Should_Load_And_Provide_Results()
        {
            WordPredictorEngineFactory f = new WordPredictorEngineFactory( ResourceFullPath );
            IPredictorEngine engine = f.Create( "sybille" );
            TestEngine( engine );
            f.Release( engine );
        }

        [Test]
        public void Sem_Sybille_Should_Load_And_Provide_Results()
        {
            WordPredictorEngineFactory f = new WordPredictorEngineFactory( ResourceFullPath );
            IPredictorEngine engine = f.Create( "sem-sybille" );
            TestEngine( engine );
            f.Release( engine );
        }

        private static void TestEngine( IPredictorEngine engine )
        {
            DirectTextualContextService textualContextService = new DirectTextualContextService();
            textualContextService.SetToken( "Je" );
            var predicted = engine.Predict( textualContextService, 20 );
            Assert.That( predicted, Is.Not.Null );
            Assert.That( predicted.Count() > 0 );
        }

    }
}
