using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WordPredictor;
using CK.WordPredictor.Model;
using CK.WordPredictor.Engines;
using NUnit.Framework;

namespace WordPredictorTest
{
    [TestFixture]
    public class WordPredictorEngineFactoryTest
    {
        [Test]
        public void Create_Should_Throw_Exception_If_No_Engine_With_The_Given_Name_Is_Available()
        {
            SybilleWordPredictorEngineFactory f = new SybilleWordPredictorEngineFactory( TestHelper.SybilleResourceFullPath );
            Assert.Throws<ArgumentException>( () => f.Create( "lucene" ) );
        }

        [Test]
        public void Sybillye_And_Semantic_Sybille_Are_Available()
        {
            SybilleWordPredictorEngineFactory f = new SybilleWordPredictorEngineFactory( TestHelper.SybilleResourceFullPath );
            Assert.That( f.Create( "sybille" ), Is.Not.Null );
            Assert.That( f.Create( "sem-sybille" ), Is.Not.Null );
        }

        class DisposableEngine : IWordPredictorEngine, IDisposable
        {
            public bool DisposedCalled { get; private set; }
            public void Dispose()
            {
                DisposedCalled = true;
            }

            public bool IsWeightedPrediction
            {
                get { throw new NotImplementedException(); }
            }

            public IEnumerable<IWordPredicted> Predict( ITextualContextService textualService, int maxSuggestedWord )
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void Release_Should_Call_Dispose()
        {
            SybilleWordPredictorEngineFactory f = new SybilleWordPredictorEngineFactory( TestHelper.SybilleResourceFullPath );
            var engine = new DisposableEngine();
            f.Release( engine );
            Assert.That( engine.DisposedCalled );
        }
    }
}
