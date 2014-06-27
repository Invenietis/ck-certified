#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\KeyScrollerTests\TestHelper.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;
using HighlightModel;
using Moq;

namespace KeyScrollerTests
{
    public class TestHelper
    {
        public static ExtensibleHighlightableElementProxy MockHighlightableFactory001()
        {
            var root = new MockHighlightableElement();
            root.AddChildren();
            return new ExtensibleHighlightableElementProxy( "Root", root );
        }

        public static ExtensibleHighlightableElementProxy CreateElement( string name )
        {
            return new ExtensibleHighlightableElementProxy( name, new MockHighlightableElement() );
        }

        public static Mock<IPluginConfigAccessor> MockPluginConfigAccessor()
        {
            var feature = new Mock<IPluginConfigAccessor>();
            feature.SetupGet( e => e.User ).Returns( MockUser().Object );
            return feature;
        }

        public static Mock<IObjectPluginConfig> MockUser()
        {
            var feature = new Mock<IObjectPluginConfig>();
            return feature;
        }

        public static Mock<IService<ITriggerService>> MockIServiceTriggerService()
        {
            var feature = new Mock<IService<ITriggerService>>();
            feature.SetupGet( e => e.Service ).Returns( MockTriggerService().Object );
            return feature;
        }

        public static Mock<ITriggerService> MockTriggerService()
        {
            var feature = new Mock<ITriggerService>();
            feature.SetupGet( e => e.DefaultTrigger ).Returns( MockTrigger().Object );
            return feature;
        }

        public static Mock<ITrigger> MockTrigger()
        {
            var feature = new Mock<ITrigger>();
            return feature;
        }
    }

    public class MockHighlightableElement : IHighlightableElement
    {
        public List<IHighlightableElement> _children;

        public MockHighlightableElement()
        {
            _children = new List<IHighlightableElement>();
        }

        public void AddChildren( string name = "Children" )
        {
            _children.Add( new ExtensibleHighlightableElementProxy( name, new MockHighlightableElement() ) );
        }

        #region IHighlightableElement Members

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return _children.ToReadOnlyList(); }
        }

        public int X
        {
            get { return 42; }
        }

        public int Y
        {
            get { return 42; }
        }

        public int Width
        {
            get { return 42; }
        }

        public int Height
        {
            get { return 42; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.None; }
        }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return true; }
        }

        #endregion
    }
}
