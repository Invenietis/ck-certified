using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using WordPredictor;

namespace WordPredictorTest
{
    public static class TestHelper
    {
        //void AttachEvent<TEventArgs>( Func<EventHandler<TEventArgs>> eventArgs )
        //{
        //    eventArgs() += ( sender, e ) => { };
        //}
    }

    [TestFixture]
    public class WordPredictorServiceTest
    {

        [Test]
        public void When_I_Predict_A_Word_I_Should_Receive_Event()
        {
            string letters = "Bon";

            var w = new Mock<IWordPredictorService>();

            w.Setup( e => e.Predict( It.IsAny<string>() ) )
                .Raises(
                    s => s.WordPredicted += ( sender, e ) => { },
                    () => new WordPredictedEventArgs( letters, letters ) )
                .Verifiable();

            w.Object.WordPredicted += ( sender, e ) =>
            {
                Assert.That( e, Is.Not.Null );
                Assert.That( e.Letters, Is.EqualTo( letters ) );
                Assert.That( e.WordPredicted, Is.EqualTo( letters ) );
            };

            w.Object.Predict( letters );

            w.VerifyAll();
        }

    }
}
