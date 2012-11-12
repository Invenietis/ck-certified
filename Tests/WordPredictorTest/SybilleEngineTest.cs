using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CK.WordPredictor;
using CK.WordPredictor.Model;
using CK.WordPredictor.Engines;
using NUnit.Framework;

namespace WordPredictorTest
{
    [TestFixture]
    public class SybilleEngineTest
    {

        [Test]
        public void Sybille_Dictionnary_Should_Load_And_Provide_Results()
        {
            SybilleWordPredictorEngineFactory f = new SybilleWordPredictorEngineFactory( TestHelper.SybilleResourceFullPath, TestHelper.MockFeature( 8 ).Object );
            IWordPredictorEngine engine = f.Create( "sybille" );
            TestEngine( engine );
            f.Release( engine );
        }

        [Test]
        public void Sem_Sybille_Should_Load_And_Provide_Results()
        {
            SybilleWordPredictorEngineFactory f = new SybilleWordPredictorEngineFactory( TestHelper.SybilleResourceFullPath, TestHelper.MockFeature( 8 ).Object );
            IWordPredictorEngine engine = f.Create( "sem-sybille" );
            TestEngine( engine );
            f.Release( engine );
        }

        private static void TestEngine( IWordPredictorEngine engine )
        {
            TextualContextService textualContextService = new TextualContextService();
            textualContextService.SetRawText( "Je" );
            var predicted = engine.Predict( textualContextService, 20 );
            Assert.That( predicted, Is.Not.Null );
            Assert.That( predicted.Count() > 0 );
        }

    }
}
