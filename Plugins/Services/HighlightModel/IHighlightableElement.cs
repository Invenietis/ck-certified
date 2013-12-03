﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public BeginScrollingInfo( TimeSpan tickInterval, IHighlightableElement previousElement )
        {
            TickInterval = tickInterval;
            PreviousElement = previousElement;
        }

        public TimeSpan TickInterval { get; private set; }
        public IHighlightableElement PreviousElement { get; private set; }
    }

    public class EndScrollingInfo
    {
        public EndScrollingInfo( TimeSpan tickInterval, IHighlightableElement previouslyHighlightedElement, IHighlightableElement elementToBeHighlighted )
        {
            TickInterval = tickInterval;
            PreviouslyHighlightedElement = previouslyHighlightedElement;
            ElementToBeHighlighted = elementToBeHighlighted;
        }

        public TimeSpan TickInterval { get; private set; }
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
        Immediate = 1
    }


    /// <summary>
    /// Enum describing the way the scroller is supposed ot move across its <see cref="IHighlightableElement" /> tree.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// No special behavior 
        /// </summary>
        Normal = 0,
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
        StayOnTheSameLocked = 4,
        /// <summary>
        /// Go up to the root of the tree containing the element, and start iterating on its first child
        /// </summary>
        RelativeRoot = 5,
        /// <summary>
        /// Go up to the very root of the keyscroller (to the parent of the RelativeRoot), and start iterating on its first child
        /// </summary>
        AbsoluteRoot = 6,
    }

}
