using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using CK.WordPredictor;
using CK.WordPredictor.Model;
using Moq;
using NUnit.Framework;

namespace WordPredictorTest
{
    public class TestHelper
    {
        public static string SybilleResourceFullPath = @"F:\Users\Cedric\Documents\Dev\__Dev4\Civikey\ck-certified\Plugins\Accessibility\CK.WordPredictor\";

        public static Mock<IPluginConfigAccessor> MockPluginConfigAccessor()
        {
            var configAccessor = new Mock<IPluginConfigAccessor>();
            var pluginConfig = new Mock<IObjectPluginConfig>();
            configAccessor.Setup( e => e.User ).Returns( pluginConfig.Object );

            return configAccessor;
        }

        public static Mock<IWordPredictorService> MockPredictorService( ObservableCollection<IWordPredicted> wordsToReturn = null )
        {
            var p = new Mock<IWordPredictorService>();
            var c =  wordsToReturn ?? new ObservableCollection<IWordPredicted>( WordCollection( 10 ) );
            p.Setup( w => w.Words )
                .Returns(
                    () => new WordPredictedCollection( c ) );
            return p;
        }

        private static IEnumerable<IWordPredicted> WordCollection( int count )
        {
            for( int i=0; i < count; ++i )
            {
                var moq = new Mock<IWordPredicted>();
                moq.SetupAllProperties();
                yield return moq.Object;
            }
        }

        public static Func<Mock<IKey>, Mock<IKey>> MockKey( int index = 0 )
        {
            return ( mockedKey ) =>
            {
                var mockLayoutKey = new Mock<ILayoutKey>();
                var mockLayoutKeyModeCurrent = new Mock<ILayoutKeyModeCurrent>();
                var mockKeyModeCurrent = new Mock<IKeyModeCurrent>();
                var mockKeyProgram = new Mock<IKeyProgram>();

                mockLayoutKeyModeCurrent.SetupAllProperties();
                mockLayoutKey.Setup( e => e.Current ).Returns( mockLayoutKeyModeCurrent.Object );

                mockKeyProgram.Setup( e => e.Commands ).Returns( new List<string>() );

                mockKeyModeCurrent.SetupAllProperties();
                mockKeyModeCurrent.Setup( e => e.OnKeyPressedCommands ).Returns( mockKeyProgram.Object );

                mockedKey.Setup( e => e.CurrentLayout ).Returns( mockLayoutKey.Object );
                mockedKey.Setup( e => e.Current ).Returns( mockKeyModeCurrent.Object );
                mockedKey.SetupProperty( e => e.Index, index );
                return mockedKey;
            };
        }

        public static Func<Mock<IZone>, Mock<IZone>> MockZone()
        {
            int keyCount = 0;
            return ( mockedZone ) =>
            {
                var keyCollectionMock = new Mock<IKeyCollection>();
                var mockKeyFactory = MockKey();
                var list = new List<IKey>();
                Func<IKey> create = () =>
                {
                    var key = mockKeyFactory( new Mock<IKey>() ).Object;
                    list.Add( key );
                    return key;
                };
                keyCollectionMock.Setup( e => e.Create() ).Callback( () => keyCount++ ).Returns( create );
                keyCollectionMock.Setup( e => e.Create( It.IsAny<int>() ) ).Callback( () => keyCount++ ).Returns( create );
                keyCollectionMock.Setup( e => e[It.IsAny<int>()] ).Returns<int>( e => list[e] );
                keyCollectionMock.Setup( e => e.Count ).Returns( () =>
                {
                    return keyCount;
                } );
                keyCollectionMock.Setup( e => e.GetEnumerator() ).Returns( () => new List<IKey>( Keys( keyCount ) ).GetEnumerator() );
                mockedZone.Setup( e => e.Keys ).Returns( keyCollectionMock.Object );

                return mockedZone;
            };
        }

        private static IEnumerable<IKey> Keys( int keyCount )
        {
            var mockKeyFactory = MockKey();
            for( int i = 0; i < keyCount; ++i ) yield return mockKeyFactory( new Mock<IKey>() ).Object;
        }


        public static Mock<IKeyboardContext> MockKeyboardContext( string keyboardName, string zoneName )
        {
            var mZone = new Mock<IZone>();
            var mzCollection = new Mock<IZoneCollection>();
            var mkb = new Mock<IKeyboard>();
            var mkbLayout = new Mock<ILayout>();
            var mkbCollection = new Mock<IKeyboardCollection>();
            var mKbContext = new Mock<IKeyboardContext>();

            MockZone()( mZone ).Setup( e => e.Destroy() ).Callback( () =>
            {
                mzCollection
                    .Setup( x => x[It.IsAny<string>()] ).Returns( () => null );
            } );
            mzCollection
                .Setup( e => e.Create( It.IsAny<string>() ) )
                .Callback<string>( ( z ) =>
                {
                    Assert.That( z == zoneName );
                    mzCollection.Setup( e => e[It.IsAny<string>()] ).Returns( mZone.Object );
                } )
                .Returns( mZone.Object );

            mzCollection
                .Setup( e => e[It.IsAny<string>()] )
                .Callback<string>( ( z ) => Assert.That( z == zoneName ) );

            mkbLayout.SetupAllProperties();
            mkb.Setup( e => e.Zones ).Returns( mzCollection.Object ).Verifiable();
            mkb.Setup( e => e.Name ).Returns( keyboardName );
            mkb.Setup( e => e.CurrentLayout ).Returns( mkbLayout.Object );

            mkbCollection
                .Setup( e => e[It.IsAny<string>()] )
                .Callback<string>( z => Assert.That( z == keyboardName ) )
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
