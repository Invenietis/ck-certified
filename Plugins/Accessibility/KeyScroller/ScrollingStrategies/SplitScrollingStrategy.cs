using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin.Config;
using CommonServices.Accessibility;
using HighlightModel;
using SimpleSkin.ViewModels;

namespace KeyScroller
{
    [Strategy( SplitScrollingStrategy.StrategyName )]
    internal class SplitScrollingStrategy : BasicScrollingStrategy
    {
        const string StrategyName = "SplitScrollingStrategy";
        const int childrenLimitBeforeSplit = 6;

        public override string Name
        {
            get { return StrategyName; }
        }

        IHighlightableElement _currentElement = null;

        public SplitScrollingStrategy( DispatcherTimer timer, List<IHighlightableElement> elements, IPluginConfigAccessor configuration )
            : base( timer, elements, configuration )
        {

        }

        protected override IHighlightableElement GetNextElement( ActionType actionType )
        {
            // reset the action type to normal
            _actionType = ActionType.Normal;

            if( actionType == ActionType.UpToParent )
            {
                _currentElement = GetUpToParent();
            }
            else
            {
                ICKReadOnlyList<IHighlightableElement> elements = null;
                // get the sibling of the current element
                if( _currentElementParents.Count > 0 ) elements = _currentElementParents.Peek().Children;
                else elements = RegisteredElements;

                if( actionType == ActionType.StayOnTheSame )
                {
                    _currentElement = GetStayOnTheSame( elements );
                }
                else if( _currentId < 0 || actionType != ActionType.EnterChild )
                {
                    // if it's the first iteration, or if we just have to go to the next sibbling
                    VMSplitZone splitZone = null;

                    if( _currentElement != null )
                    {
                        splitZone = _currentElement as VMSplitZone;
                    }

                    if( splitZone != null && splitZone.Next != null )
                    {
                        _currentElement = splitZone.Next;
                    }
                    else
                    {
                        if( _currentId < elements.Count - 1 ) _currentId++;
                        // if we are at the end of this elements set and if there is a parent in the stack, move to parent
                        else if( _currentElementParents.Count > 0 ) return GetNextElement( ActionType.UpToParent );
                        // otherwise we go back to the first element
                        else _currentId = 0;

                        var nextElementToSplit = elements[_currentId];
                        if( !(nextElementToSplit is VMZoneSimple) )
                        {
                            _currentElement = nextElementToSplit;
                        }
                        else
                        {
                            var vmz = nextElementToSplit as VMZoneSimple;
                            if( vmz != null && vmz.Children.Count > childrenLimitBeforeSplit )
                            {
                                _currentElement = new VMSplitZone( vmz.Context, vmz.Children.Skip( 0 ).Take( vmz.Children.Count / 2 ), vmz.Children.Skip( vmz.Children.Count / 2 ) );
                            }
                            else
                            {
                                _currentElement = nextElementToSplit;
                            }
                        }
                    }
                }
                else
                {
                    _currentElement = GetEnterChild( elements );
                }
            }

            return GetSkipBehavior( _currentElement );
        }

        public override void OnExternalEvent()
        {
            if( _currentElement != null )
            {
                if( _currentElement.Children.Count > 0 ) _actionType = ActionType.EnterChild;
                else
                {
                    FireSelectElement( this, new HighlightEventArgs( _currentElement ) );
                    _actionType = ActionType.StayOnTheSame;
                }
            }
        }
    }
}
