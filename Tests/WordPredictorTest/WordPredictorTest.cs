using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var configAccessor = new Mock<IPluginConfigAccessor>();
            var pluginConfig = new Mock<IObjectPluginConfig>();
            configAccessor.Setup( e => e.User ).Returns( pluginConfig.Object );
            
            DirectTextualContextService t = new DirectTextualContextService();
            
            WordPredictorService w = new WordPredictorService();
            w.ResourcePath = SybilleEngineTest.ResourceFullPath;
            w.Config = configAccessor.Object;
            w.TextualContextService = t;
            
            w.Start();

            t.SetToken( "Je" );
            Assert.That( w.Words.Count > 0 );
            Console.WriteLine( String.Join(" ", w.Words.Select( o => o.Word ).ToArray() ) );
            t.SetToken( " " );
            t.SetToken( "Bon" );
            Assert.That( w.Words.Count > 0 );
            Console.WriteLine( String.Join( " ", w.Words.Select( o => o.Word ).ToArray() ) );

            w.Stop();
        }
    }
}
