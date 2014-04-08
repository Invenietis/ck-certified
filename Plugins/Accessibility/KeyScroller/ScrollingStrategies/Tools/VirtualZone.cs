﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using HighlightModel;

namespace Scroller
{
    public class VirtualZone : IHighlightableElement
    {
        public IHighlightableElement WrappedElement { get; private set; }

        public VirtualZone( ICKReadOnlyList<IHighlightableElement> elements, int start, int length )
        {
            List<IHighlightableElement> children = new List<IHighlightableElement>();

            for( int i = start; i < elements.Count && i < length + start; i++ )
            {
                children.Add( elements[i] );
            }

            Children = new CKReadOnlyListOnIList<IHighlightableElement>( children );
        }

        public VirtualZone( IHighlightableElement element )
        {
            WrappedElement = element;

            List<IHighlightableElement> children = new List<IHighlightableElement>();
            int childPerZone = element.Children.Count / HalfZoneScrollingStrategy.ZoneDivider;

            for( int i = 0; i < HalfZoneScrollingStrategy.ZoneDivider; i++ )
            {
                //On the last iteration, we add the remaining children.
                if( i == HalfZoneScrollingStrategy.ZoneDivider - 1 )
                    children.Add( new VirtualZone( element.Children, childPerZone * i, childPerZone + element.Children.Count % HalfZoneScrollingStrategy.ZoneDivider ) );
                else
                    children.Add( new VirtualZone( element.Children, childPerZone * i, childPerZone ) );
            }

            Children = new CKReadOnlyListOnIList<IHighlightableElement>( children );
            Skip = SkippingBehavior.None;
        }

        /// <summary>
        /// Update a child reference with a new one
        /// </summary>
        /// <param name="oldChild"></param>
        /// <param name="newChild"></param>
        public void UpdateChild( IHighlightableElement oldChild, IHighlightableElement newChild )
        {
            int idx = Children.IndexOf( oldChild );
            List<IHighlightableElement> list = Children.ToList();
            list[idx] = newChild;

            Children = new CKReadOnlyListOnIList<IHighlightableElement>( list );
        }

        #region IHighlightableElement Members

        public ICKReadOnlyList<IHighlightableElement> Children { get; private set; }

        public int X
        {
            get { return 0; }
        }

        public int Y
        {
            get { return 0; }
        }

        public int Width
        {
            get { return 0; }
        }

        public int Height
        {
            get { return 0; }
        }

        public SkippingBehavior Skip
        {
            get;
            internal set;
        }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            foreach( var child in Children )
            {
                child.BeginHighlight( beginScrollingInfo, scrollingDirective );
            }
            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            foreach( var child in Children )
            {
                child.EndHighlight( endScrollingInfo, scrollingDirective );
            }
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return false; }
        }

        #endregion
    }
}