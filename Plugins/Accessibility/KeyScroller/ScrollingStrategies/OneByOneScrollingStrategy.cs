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
using System.Diagnostics;

namespace KeyScroller
{
    /// <summary>
    /// Scrolling on each key one after the other, without taking zones into account
    /// </summary>
    [StrategyAttribute( OneByOneScrollingStrategy.StrategyName )]
    public class OneByOneScrollingStrategy : ScrollingStrategyBase
    {
        const string StrategyName = "OneByOneScrollingStrategy";
        public OneByOneScrollingStrategy( DispatcherTimer timer, List<IHighlightableElement> elements, IPluginConfigAccessor configuration )
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
            // if there is no parent, we are at the root level, we'll start iterating on the current tree's next sibling
            if( _currentElementParents.Count == 0 ) return GetNextElement( ActionType.Normal );

            //We get the parent and fetch its siblings
            IHighlightableElement parent = _currentElementParents.Pop();
            ICKReadOnlyList<IHighlightableElement> parentSiblings = null;
            if( _currentElementParents.Count > 0 )
            {
                parentSiblings = _currentElementParents.Peek().Children;
            }
            else
            {
                //there, we actually are at the root level
                Debug.Assert( parent.IsHighlightableTreeRoot );
                parentSiblings = RegisteredElements;

                //If this tree is the only tree at the root level, we directly start iterating on its children
                if( parentSiblings.Count == 1 ) return GetNextElement( ActionType.EnterChild );
            }

            //We get the current element's parent. The idea it to directly enter the next sibling's children to bypass the zones.
            int parentId = parentSiblings.IndexOf( parent );

            //When the parent is the last of its level, we go up to the next upper level. 
            if( parentId == parentSiblings.Count - 1 )
            {
                _currentId = parentId = 0;
                return GetNextElement( ActionType.UpToParent );
            }
            else ++parentId; //otherwise we get to the next parent to start iterating on its children

            nextElement = parentSiblings[parentId];

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
                    if( element != null && element.Children.Count > 0 && !element.IsHighlightableTreeRoot )
                    {
                        return GetNextElement( ActionType.EnterChild );
                    }
                    return element;
            }
        }

        public override void OnExternalEvent()
        {
            if( _currentElement != null )
            {
                FireSelectElement();
            }
        }
    }
}
