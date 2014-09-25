#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\HighlightModel\IHighlightableElement.cs) is part of CiviKey. 
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

using CK.Core;

namespace HighlightModel
{
    public interface IHighlightableElement
    {
        /// <summary>
        /// Gets the collection of inner children of this element.
        /// </summary>
        ICKReadOnlyList<IHighlightableElement> Children { get; }

        /// <summary>
        /// Gets the X position of this element. This property can be used by scroll algorythms to determine the scroll scenario.
        /// <remarks>X, Y, Width and Height can be "virtual" values, not necessary pixels positions on the screen</remarks>
        /// </summary>
        int X { get; }

        /// <summary>
        /// Gets the Y position of this element. This property can be used by scroll algorythms to determine the scroll scenario.
        /// <remarks>X, Y, Width and Height can be "virtual" values, not necessary pixels positions on the screen</remarks>
        int Y { get; }

        /// <summary>
        /// Gets the width of this element. This property can be used by scroll algorythms to determine the scroll scenario.
        /// <remarks>X, Y, Width and Height can be "virtual" values, not necessary pixels positions on the screen</remarks>
        int Width { get; }

        /// <summary>
        /// Gets the height of this element. This property can be used by scroll algorythms to determine the scroll scenario.
        /// <remarks>X, Y, Width and Height can be "virtual" values, not necessary pixels positions on the screen</remarks>
        int Height { get; }

        /// <summary>
        /// Gets if the element has to be skipped by the scoll algorythm.
        /// </summary>
        SkippingBehavior Skip { get; }

        /// <summary>
        /// Called by the <see cref="KeyScroller"/> when the element is being scrolled on
        /// </summary>
        /// <param name="beginScrollingInfo">Gets information about the scroller configuration & state</param>
        /// <returns>Directives used to choose the next element (return null if you don't want to change the default behavior)</returns>
        ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective );

        /// <summary>
        /// Called by the <see cref="KeyScroller"/> when the element is not being scrolled on anymore
        /// </summary>
        /// <param name="endScrollingInfo">Gets information about the scroller configuration & state</param>
        /// <returns>Directives used to choose the next element (return null if you don't want to change the default behavior)</returns>
        ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective );

        /// <summary>
        /// Called by the <see cref="KeyScroller"/> when the element was being scrolled on when the triggering input was pressed
        /// </summary>
        /// <returns>Directives used to choose the next element (return null if you don't want to change the default behavior)</returns>
        ScrollingDirective SelectElement( ScrollingDirective scrollingDirective );

        /// <summary>
        /// Gets whether this element is the root of its tree
        /// </summary>
        bool IsHighlightableTreeRoot { get; }
    }

    /// <summary>
    /// Object that defines the way the KeyScroller should travel through its elements tree.
    /// </summary>
    public class ScrollingDirective
    {
        public ScrollingDirective( ActionType nextActionType, ActionTime actionTime = HighlightModel.ActionTime.NextTick )
        {
            NextActionType = nextActionType;
            ActionTime = actionTime;
        }

        public ActionTime ActionTime { get; set; }
        public ActionType NextActionType { get; set; }
    }

    /// <summary>
    /// Information about the scrolling device's configuration & state.
    /// Contains the time interval between two scrolling beats and the element that was previously scrolled on
    /// </summary> 
    public class BeginScrollingInfo
    {
        public BeginScrollingInfo( double tickInterval, IHighlightableElement previousElement )
        {
            TickInterval = tickInterval;
            PreviousElement = previousElement;
        }

        public double TickInterval { get; private set; }
        public IHighlightableElement PreviousElement { get; private set; }
    }

    public class EndScrollingInfo
    {
        public EndScrollingInfo( double tickInterval, IHighlightableElement previouslyHighlightedElement, IHighlightableElement elementToBeHighlighted )
        {
            TickInterval = tickInterval;
            PreviouslyHighlightedElement = previouslyHighlightedElement;
            ElementToBeHighlighted = elementToBeHighlighted;
        }

        public double TickInterval { get; private set; }
        public IHighlightableElement PreviouslyHighlightedElement { get; private set; }
        public IHighlightableElement ElementToBeHighlighted { get; private set; }
    }


    /// <summary>
    /// Describes when the KeyScroller should start highlighting the next element in the scrolling tree.
    /// </summary>
    public enum ActionTime
    {
        /// <summary>
        /// Default behavior : the next element will be highlighted when the next tick comes.
        /// </summary>
        NextTick = 0,
        /// <summary>
        /// The KeyScroller immediately hightlights the next element, without waiting for the next tick.
        /// </summary>
        Immediate = 1,
        /// <summary>
        /// The KeyScroller wait one tick before execute the action.
        /// </summary>
        Delayed = 2,
    }


    /// <summary>
    /// Enum describing the way the scroller is supposed ot move across its <see cref="IHighlightableElement" /> tree.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// No special behavior 
        /// </summary>
        MoveNext = 0,
        /// <summary>
        /// Try to scroll on the current element's children
        /// </summary>
        EnterChild = 1,
        /// <summary>
        /// Go up to the parent
        /// </summary>
        UpToParent = 2,
        /// <summary>
        /// Stay on the current element until the next tick
        /// </summary>
        StayOnTheSameOnce = 3,
        /// <summary>
        /// Stay on the current element until the element says otherwise
        /// </summary>
        StayOnTheSame = 4,
        /// <summary>
        /// Go up to the first sibling
        /// </summary>
        MoveToFirst = 5,
        /// <summary>
        /// Go up to the last sibling
        /// </summary>
        MoveToLast = 6,
        /// <summary>
        /// Go up to the root of the tree containing the element, and start iterating on its first child
        /// </summary>
        GoToRelativeRoot = 7,
        /// <summary>
        /// Go up to the very root of the keyscroller (to the parent of the RelativeRoot), and start iterating on its first child
        /// </summary>
        GoToAbsoluteRoot = 8,

    }
}
