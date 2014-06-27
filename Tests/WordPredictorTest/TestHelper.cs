#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\WordPredictorTest\TestHelper.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WordPredictor;
using CK.WordPredictor.Model;
using Moq;
using NUnit.Framework;

namespace WordPredictorTest
{
    public static class ServiceHelper
    {
        public static IService<TService> MockServiceWrapper<TService>() where TService : class, IDynamicService
        {
            Mock<TService> serviceMock = new Mock<TService>();
            return serviceMock.MockServiceWrapper();
        }

        public static IService<TService> MockServiceWrapper<TService>( this Mock<TService> serviceMock ) where TService : class, IDynamicService
        {
            Mock<IService<TService>> serviceWrapperMock = new Mock<IService<TService>>();
            serviceWrapperMock.SetupGet( e => e.Service ).Returns( serviceMock.Object );
            return serviceWrapperMock.Object;
        }

        public static IService<TService> MockServiceWrapper<TService>( this TService service ) where TService : class, IDynamicService
        {
            Mock<IService<TService>> serviceWrapperMock = new Mock<IService<TService>>();
            serviceWrapperMock.SetupGet( e => e.Service ).Returns( service );
            return serviceWrapperMock.Object;
        }
    }

    public class TestHelper
    {
        public static Func<string> SybilleResourceFullPath = () => @"F:\Users\Cedric\Documents\Dev\__Dev4\Civikey\ck-certified\Plugins\Prediction\CK.WordPredictor.Sybille\";

        public const string PredictionZoneName = "Prediction";

        public const string CompatibilityKeyboardName = "CompatibilityKeyboardName";

        public static Mock<IWordPredictorFeature> MockFeature( int maxSuggestedWords )
        {
            var feature = new Mock<IWordPredictorFeature>();
            feature.SetupGet( e => e.MaxSuggestedWords ).Returns( maxSuggestedWords );
            feature.SetupGet( e => e.InsertSpaceAfterPredictedWord ).Returns( true );
            feature.SetupGet( e => e.Engine ).Returns( "sybille" );
            return feature;
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
                mockKeyModeCurrent.Setup( e => e.OnKeyDownCommands ).Returns( mockKeyProgram.Object );

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

            mkbCollection.Setup( e => e.GetEnumerator() ).Returns( new List<IKeyboard>() { mkb.Object }.GetEnumerator() );

            mKbContext.Setup( e => e.CurrentKeyboard ).Returns( mkb.Object );

            mKbContext
                .Setup( e => e.Keyboards )
                .Returns( mkbCollection.Object )
                .Verifiable();

            return mKbContext;
        }

    }
}
