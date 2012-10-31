using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.WordPredictor.UI;
using Moq;
using NUnit.Framework;

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

            Assert.That( p.Context.Keyboards["Azerty"].Zones["Prediction"] != null );
            mKbContext.VerifyAll();
        }

        [Test]
        public void When_Plugin_Is_Stopped_It_Must_Destroys_Prediction_Zone_Previously_Created()
        {
            var mKbContext = MockKeyboardContext();
            InKeyboardWordPredictor p = new InKeyboardWordPredictor();
            p.Context = mKbContext.Object;
            p.Start();
            Assert.That( p.Context.Keyboards["Azerty"].Zones["Prediction"] != null );

            p.Stop();

            Assert.That( p.Context.Keyboards["Azerty"].Zones["Prediction"] == null );
            mKbContext.VerifyAll();
        }

        private static Mock<IKeyboardContext> MockKeyboardContext()
        {
            var mzCollection = new Mock<IZoneCollection>();
            var mkb = new Mock<IKeyboard>();
            var mkbCollection = new Mock<IKeyboardCollection>();
            var mKbContext = new Mock<IKeyboardContext>();
            var mZone = new Mock<IZone>();

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
                .Callback<string>( ( z ) => Assert.That( z == "Prediction" ) )
                .Returns( mZone.Object );
            mkb
                .Setup( e => e.Zones )
                .Returns( mzCollection.Object )
                .Verifiable();
            mkbCollection
                .Setup( e => e[It.IsAny<string>()] )
                .Callback<string>( z => Assert.That( z == "Azerty" ) )
                .Returns( mkb.Object )
                .Verifiable();
            mKbContext
                .Setup( e => e.Keyboards )
                .Returns( mkbCollection.Object )
                .Verifiable();

            return mKbContext;
        }

    }
}