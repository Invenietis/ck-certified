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
