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
        const int ChildrenLimitBeforeSplit = 6;

        public override string Name
        {
            get { return StrategyName; }
        }

        IHighlightableElement _nextElement = null;

        public SplitScrollingStrategy( DispatcherTimer timer, List<IHighlightableElement> elements, IPluginConfigAccessor configuration )
            : base( timer, elements, configuration )
        {
        }

        protected override IHighlightableElement GetUpToParent()
        {
            IHighlightableElement nextElement = null;
            // if there is no parent, go to normal next element
            if( _currentElementParents.Count == 0 ) return GetNextElement( ActionType.Normal );

            IHighlightableElement parent = _currentElementParents.Pop();
            IHighlightableElement parentObj = null;
            ICKReadOnlyList<IHighlightableElement> parentSibblings = null;
            if( _currentElementParents.Count > 0 )
            {
                parentObj = _currentElementParents.Peek();
                parentSibblings = parentObj.Children;
            }
            else parentSibblings = RegisteredElements;

            // if it's a splitted zone, "insert" it in the model logically
            if( parent is VMSplitZone )
            {
                var parentSplitZone = parent as VMSplitZone;
                if( parentObj is VMZoneSimple && !( parentObj is VMSplitZone ) )
                {
                    if( _currentElementParents.Count >= 1 )
                    {
                        parentObj = _currentElementParents.Skip( 1 ).Take( 1 ).SingleOrDefault();
                        if( parentObj != null ) parentSibblings = parentObj.Children;
                    }
                }

                _currentId = parentSibblings.IndexOf( parentSplitZone.Original );
                nextElement = parentSplitZone.Parent ?? parentSplitZone;
            }
            else
            {
                _currentId = parentSibblings.IndexOf( parent );
                nextElement = parent;
            }

            if( _currentId < 0 ) _currentId = 0;

            // if the parent skipping behavior is enter children, we skip it
            if( parent.Skip == SkippingBehavior.EnterChildren )
            {
                _currentElement = nextElement;
                return GetNextElement( ActionType.Normal );
            }

            return nextElement;
        }

        protected override IHighlightableElement GetEnterChild( ICKReadOnlyList<IHighlightableElement> elements )
        {
            // if the current element does not have any children ... go to the normal next element
            if( _nextElement == null || ( _nextElement != null && _nextElement.Children.Count == 0 ) ) return GetNextElement( ActionType.Normal );
            // otherwise we just push the element as a parent and set the the first child as the current element
            _currentId = 0;

            var vmz = _nextElement as VMZoneSimple;
            if( vmz != null && !( vmz is VMSplitZone ) && vmz.Children.Count > ChildrenLimitBeforeSplit )
            {
                _nextElement = new VMSplitZone( vmz, vmz.Children.Skip( 0 ).Take( vmz.Children.Count / 2 ), vmz.Children.Skip( vmz.Children.Count / 2 ) );
                // push the original zone in the history
                _currentElementParents.Push( vmz );
                return _nextElement;
            }
            else
            {
                _currentElementParents.Push( _nextElement );
            }

            return _nextElement.Children[0];
        }

        protected override IHighlightableElement GetNextElement( ActionType actionType )
        {
            // reset the action type to normal
            if( actionType != ActionType.StayOnTheSameLocked )
                _lastDirective.NextActionType = ActionType.Normal;

            if( actionType == ActionType.AbsoluteRoot )
            {
                _nextElement = GetUpToAbsoluteRoot();
            }
            else if( actionType == ActionType.RelativeRoot )
            {
                _nextElement = GetUpToRelativeRoot();
            }
            else if( actionType == ActionType.UpToParent )
            {
                _nextElement = GetUpToParent();
            }
            else
            {
                ICKReadOnlyList<IHighlightableElement> elements = null;
                // get the sibling of the current element
                if( _currentElementParents.Count > 0 ) elements = _currentElementParents.Peek().Children;
                else elements = RegisteredElements;

                if( actionType == ActionType.StayOnTheSameOnce || actionType == ActionType.StayOnTheSameLocked )
                {
                    _nextElement = GetStayOnTheSame( elements );
                }
                else if( _currentId < 0 || actionType != ActionType.EnterChild )
                {
                    // if it's the first iteration, or if we just have to go to the next sibbling
                    VMSplitZone splitZone = null;

                    if( _nextElement != null )
                    {
                        splitZone = _nextElement as VMSplitZone;
                    }

                    if( splitZone != null && splitZone.Next != null )
                    {
                        _nextElement = splitZone.Next;
                    }
                    else
                    {
                        if( _currentId < elements.Count - 1 ) _currentId++;
                        // if we are at the end of this elements set and if there is a parent in the stack, move to parent
                        else if( _currentElementParents.Count > 0 ) return GetNextElement( ActionType.UpToParent );
                        // otherwise we go back to the first element
                        else _currentId = 0;

                        var nextElementToSplit = elements[_currentId];
                        if( ( _nextElement is VMSplitZone ) )
                        {
                            _nextElement = GetUpToParent();
                        }
                        else if( !( nextElementToSplit is VMZoneSimple ) )
                        {
                            _nextElement = nextElementToSplit;
                        }
                        else
                        {
                            var vmz = nextElementToSplit as VMZoneSimple;
                            _nextElement = vmz;
                        }
                    }
                }
                else
                {
                    _nextElement = GetEnterChild( _nextElement.Children );
                }
            }

            return GetSkipBehavior( _nextElement );
        }
    }
}
