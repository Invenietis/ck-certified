using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Plugin.Config;
using CK.WordPredictor;
using Moq;
using NUnit.Framework;

namespace WordPredictorTest
{
    [TestFixture]
    public class WordPredictorTest
    {
        [Test]
        public void WordPredictorServiceTest()
        {
            DirectTextualContextService t = new DirectTextualContextService();

            WordPredictorServiceBase w = new SybilleWordPredictorService()
            {
                Feature = TestHelper.MockFeature( 10 ).Object,
                PluginDirectoryPath = () => TestHelper.SybilleResourceFullPath,
                TextualContextService = t
            };
            w.Setup( null );
            w.Start();

            Task.WaitAll( w.AsyncEngineContinuation );

            t.SetToken( "Je" );
            Assert.That( w.Words.Count > 0 );
            Console.WriteLine( String.Join( " ", w.Words.Select( o => o.Word ).ToArray() ) );
            t.SetToken( " " );
            t.SetToken( "Bon" );
            Assert.That( w.Words.Count > 0 );
            Console.WriteLine( String.Join( " ", w.Words.Select( o => o.Word ).ToArray() ) );

            w.Stop();
        }
    }
}
