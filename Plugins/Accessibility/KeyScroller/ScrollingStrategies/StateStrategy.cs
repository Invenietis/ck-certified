using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin.Config;
using CommonServices.Accessibility;
using HighlightModel;

namespace KeyScroller
{
    [StrategyAttribute( StateStrategy.StrategyName )]
    public class StateStrategy : ScrollingStrategy
    {
        const string StrategyName = "StateStrategy";
        public override string Name
        {
            get { return StrategyName; }
        }

        public StateStrategy( DispatcherTimer timer, List<IHighlightableElement> elements, IPluginConfigAccessor configuration )
            : base( timer , elements, configuration )
        {
        }

        public override void OnExternalEvent()
        {
            if( _currentElement != null )
            {
                FireSelectElement( this, new HighlightEventArgs( _currentElement ) );
                _actionType = ActionType.EnterChild;
            }
            
        }


        protected override IHighlightableElement GetNextElement( ActionType actionType )
        {
            ICKReadOnlyList<IHighlightableElement> elements = null;
            // get the sibling of the current element
            if( _currentElementParents.Count > 0 ) elements = _currentElementParents.Peek().Children;
            else elements = RegisteredElements;

            if( _currentId > -1 && elements[_currentId] is IActionableElement )
            {
                var e = (IActionableElement)elements[_currentId];
                actionType = e.ActionType;
            }

            _actionType = ActionType.Normal;

            IHighlightableElement nextElement = null;

            if( actionType == ActionType.UpToParent )
            {
                nextElement = GetUpToParent();
            }
            else
            {
                if( actionType == ActionType.StayOnTheSame )
                {
                    nextElement = GetStayOnTheSame( elements );
                }
                else if( _currentId < 0 || actionType != ActionType.EnterChild )
                {
                    // if it's the first iteration, or if we just have to go to the next sibbling

                    if( _currentId < elements.Count - 1 ) _currentId++;
                    // if we are at the end of this elements set and if there is a parent in the stack, move to parent
                    else if( _currentElementParents.Count > 0 ) return GetNextElement( ActionType.UpToParent );
                    // otherwise we go back to the first element
                    else _currentId = 0;

                    nextElement = elements[_currentId];
                }
                else
                {
                    nextElement = GetEnterChild( elements );
                }
            }
            return GetSkipBehavior( nextElement );
        }
    }

    public interface IActionableElement : IHighlightableElement
    {
        ActionType ActionType { get; set; }
    }
}
