using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using KeyScroller;
using CK.Core;
using CommonServices.Accessibility;
using HighlightModel;
using CK.Plugin.Config;

namespace KeyScroller
{
    /// <summary>
    /// Scrolling on each key one after the other, without taking zones into account
    /// </summary>
    [StrategyAttribute( SimpleScrollingStrategy.StrategyName )]
    public class SimpleScrollingStrategy : ScrollingStrategy
    {
        const string StrategyName = "SimpleScrollingStrategy";
        public SimpleScrollingStrategy( DispatcherTimer timer, List<IHighlightableElement> elements, IPluginConfigAccessor configuration )
            : base( timer, elements, configuration )
        {
        }

        public override string Name
        {
            get { return StrategyName; }
        }

        protected override IHighlightableElement GetUpToParent()
        {
            IHighlightableElement nextElement = null;
            // if there is no parent, go to normal next element
            if( _currentElementParents.Count == 0 ) return GetNextElement( ActionType.Normal );

            IHighlightableElement parent = _currentElementParents.Pop();
            ICKReadOnlyList<IHighlightableElement> parentSibblings = null;
            if( _currentElementParents.Count > 0 ) parentSibblings = _currentElementParents.Peek().Children;
            else parentSibblings = RegisteredElements;

            int parentId = parentSibblings.IndexOf( parent );

            //Range test
            if( parentId == parentSibblings.Count - 1 ) parentId = 0;
            else ++parentId;

            nextElement = parentSibblings[parentId];

            _currentElement = nextElement;
            _currentId = parentId;

            return GetNextElement( ActionType.EnterChild );
        }

        protected override IHighlightableElement GetSkipBehavior( IHighlightableElement element )
        {
            switch( element.Skip )
            {
                case SkippingBehavior.Skip:
                    return GetNextElement( ActionType.Normal );
                default:
                    if( element != null && element.Children.Count > 0 ) return GetNextElement( ActionType.EnterChild );
                    return element;
            }
        }

        internal override void OnExternalEvent()
        {
            if( _currentElement != null )
            {
                FireSelectElement( this, new HighlightEventArgs( _currentElement ) );
                _actionType = ActionType.UpToParent; //We directly go back to the relative root
                //TODOJL : RelativeRoot
            }
        }
    }
}
