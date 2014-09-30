#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\KeyScrollerTests\KeyScrollerTests.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HighlightModel;
using Scroller;
using NUnit.Framework;

namespace KeyScrollerTests
{
    [TestFixture]
    public class KeyScrollerTests
    {
        [Test]
        public void KeyScrollerRegisterTest()
        {
            ScrollerPlugin scroller = new ScrollerPlugin();
            scroller.InputTrigger = TestHelper.MockIServiceTriggerService().Object;
            scroller.Configuration = TestHelper.MockPluginConfigAccessor().Object;
            scroller.Setup( null );
            scroller.Start();

            var mock = TestHelper.MockHighlightableFactory001();

            scroller.RegisterTree( "Keyboard", "Keyboard", mock );
            Assert.That( scroller.ScrollableElements.Contains( mock ), Is.True );
            Assert.That( scroller.ScrollableElements.Count, Is.EqualTo( 1 ) );

            scroller.RegisterTree( "Keyboard", "Keyboard", mock );
            Assert.That( scroller.ScrollableElements.Contains( mock ) );
            Assert.That( scroller.ScrollableElements.Count, Is.EqualTo( 1 ) );

            var preroot = TestHelper.CreateElement( "PreRoot" );
            Assert.That( scroller.RegisterInRegisteredElementAt( "Keyboard", "Root", ChildPosition.Pre, preroot ), Is.True );
            Assert.That( mock.PreChildren.Contains( preroot ), Is.True );
            Assert.That( mock.PreChildren[0], Is.EqualTo( preroot ) );
            Assert.That( mock.Children[0], Is.EqualTo( preroot ) );

            var preroot2 = TestHelper.CreateElement( "PreRoot2" );
            Assert.That( scroller.RegisterInRegisteredElementAt( "Keyboard", "Root", ChildPosition.Pre, preroot2 ), Is.True );
            Assert.That( mock.PreChildren.Contains( preroot2 ), Is.True );
            Assert.That( mock.PreChildren[0], Is.EqualTo( preroot2 ) );
            Assert.That( mock.Children[0], Is.EqualTo( preroot2 ) );

            var postroot = TestHelper.CreateElement( "PostRoot" );
            Assert.That( scroller.RegisterInRegisteredElementAt( "Keyboard", "Root", ChildPosition.Post, postroot ), Is.True );
            Assert.That( mock.PostChildren.Contains( postroot ), Is.True );
            Assert.That( mock.PostChildren[0], Is.EqualTo( postroot ) );
            Assert.That( mock.Children[mock.Children.Count - 1], Is.EqualTo( postroot ) );

            var postroot2 = TestHelper.CreateElement( "PostRoot2" );
            Assert.That( scroller.RegisterInRegisteredElementAt( "Keyboard", "Root", ChildPosition.Post, postroot2 ), Is.True );
            Assert.That( mock.PostChildren.Contains( postroot2 ), Is.True );
            Assert.That( mock.PostChildren[1], Is.EqualTo( postroot2 ) );
            Assert.That( mock.Children[mock.Children.Count - 1], Is.EqualTo( postroot2 ) );

            var children = (ExtensibleHighlightableElementProxy)mock.HighlightableElement.Children[0];
            var prechildren = TestHelper.CreateElement( "PreChildren" );
            Assert.That( scroller.RegisterInRegisteredElementAt( "Keyboard", "Children", ChildPosition.Pre, prechildren ), Is.True );
            Assert.That( children.PreChildren.Contains( prechildren ), Is.True );
            Assert.That( children.PreChildren[0], Is.EqualTo( prechildren ) );
            Assert.That( children.Children[0], Is.EqualTo( prechildren ) );

            var prechildren2 = TestHelper.CreateElement( "PreChildren2" );
            Assert.That( scroller.RegisterInRegisteredElementAt( "Keyboard", "Children", ChildPosition.Pre, prechildren2 ), Is.True );
            Assert.That( children.PreChildren.Contains( prechildren2 ), Is.True );
            Assert.That( children.PreChildren[0], Is.EqualTo( prechildren2 ) );
            Assert.That( children.Children[0], Is.EqualTo( prechildren2 ) );

            var postchildren = TestHelper.CreateElement( "PostChildren" );
            Assert.That( scroller.RegisterInRegisteredElementAt( "Keyboard", "Children", ChildPosition.Post, postchildren ), Is.True );
            Assert.That( children.PostChildren.Contains( postchildren ), Is.True );
            Assert.That( children.PostChildren[0], Is.EqualTo( postchildren ) );
            Assert.That( children.Children[children.Children.Count - 1], Is.EqualTo( postchildren ) );

            var postchildren2 = TestHelper.CreateElement( "PostChildren2" );
            Assert.That( scroller.RegisterInRegisteredElementAt( "Keyboard", "Children", ChildPosition.Post, postchildren2 ), Is.True );
            Assert.That( children.PostChildren.Contains( postchildren2 ), Is.True );
            Assert.That( children.PostChildren[1], Is.EqualTo( postchildren2 ) );
            Assert.That( children.Children[children.Children.Count - 1], Is.EqualTo( postchildren2 ) );

            Assert.That( scroller.UnregisterInRegisteredElement( "Keyboard", "Root", ChildPosition.Pre, preroot ), Is.True );
            Assert.That( mock.PreChildren.Contains( preroot ), Is.False );
            Assert.That( mock.PreChildren[0], Is.EqualTo( preroot2 ) );
            Assert.That( mock.Children[0], Is.EqualTo( preroot2 ) );

            Assert.That( scroller.UnregisterInRegisteredElement( "Keyboard", "Root", ChildPosition.Pre, preroot2 ), Is.True );
            Assert.That( mock.PreChildren.Contains( preroot2 ), Is.False );
            Assert.That( mock.PreChildren.Count, Is.EqualTo( 0 ) );
            Assert.That( mock.Children[0], Is.Not.EqualTo( preroot2 ) );

            Assert.That( scroller.UnregisterInRegisteredElement( "Keyboard", "Root", ChildPosition.Post, postroot ), Is.True );
            Assert.That( mock.PostChildren.Contains( postroot ), Is.False );
            Assert.That( mock.PostChildren[0], Is.EqualTo( postroot2 ) );
            Assert.That( mock.Children[mock.Children.Count - 1], Is.EqualTo( postroot2 ) );

            Assert.That( scroller.UnregisterInRegisteredElement( "Keyboard", "Root", ChildPosition.Post, postroot2 ), Is.True );
            Assert.That( mock.PostChildren.Contains( postroot2 ), Is.False );
            Assert.That( mock.PostChildren.Count, Is.EqualTo( 0 ) );
            Assert.That( mock.Children[mock.Children.Count - 1], Is.Not.EqualTo( postroot2 ) );

            Assert.That( scroller.UnregisterInRegisteredElement( "Keyboard", "Children", ChildPosition.Pre, prechildren ), Is.True );
            Assert.That( children.PreChildren.Contains( prechildren ), Is.False );
            Assert.That( children.PreChildren[0], Is.EqualTo( prechildren2 ) );
            Assert.That( children.Children[0], Is.EqualTo( prechildren2 ) );

            Assert.That( scroller.UnregisterInRegisteredElement( "Keyboard", "Children", ChildPosition.Pre, prechildren2 ), Is.True );
            Assert.That( children.PreChildren.Contains( prechildren2 ), Is.False );
            Assert.That( children.PreChildren.Count, Is.EqualTo( 0 ) );
            Assert.That( children.Children[0], Is.Not.EqualTo( prechildren2 ) );

            Assert.That( scroller.UnregisterInRegisteredElement( "Keyboard", "Children", ChildPosition.Post, postchildren ), Is.True );
            Assert.That( children.PostChildren.Contains( postchildren ), Is.False );
            Assert.That( children.PostChildren[0], Is.EqualTo( postchildren2 ) );
            Assert.That( children.Children[children.Children.Count - 1], Is.EqualTo( postchildren2 ) );

            Assert.That( scroller.UnregisterInRegisteredElement( "Keyboard", "Children", ChildPosition.Post, postchildren2 ), Is.True );
            Assert.That( children.PostChildren.Contains( postchildren2 ), Is.False );
            Assert.That( children.PostChildren.Count, Is.EqualTo( 0 ) );
        }
    }
}
