#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\HighlightModel\ExtensiblehighlightableElementProxy.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace HighlightModel
{
    //QUESTION JL : there should be comments here, to understand its purpose
    public class ExtensibleHighlightableElementProxy : IExtensibleHighlightableElement
    {
        IHighlightableElement _element;

        string _name;

        List<IHighlightableElement> _preChildren;
        List<IHighlightableElement> _postChildren;
        bool _highlightPrePostChildren;

        public IHighlightableElement HighlightableElement
        {
            get { return _element;  }
        }

        public ExtensibleHighlightableElementProxy( string name, IHighlightableElement element, bool highlightPrePostChildren = false )
        {
            if( element == null ) throw new ArgumentNullException( "element" );

            _preChildren = new List<IHighlightableElement>();
            _postChildren = new List<IHighlightableElement>();

            _highlightPrePostChildren = highlightPrePostChildren;
            _element = element;
            _name = name;
        }

        #region IExtensibleHighlightableElement Members

        /// <summary>
        /// Adds an element at the beginning or the end of the child list.
        /// An element can be added only once for a given position.
        /// </summary>
        /// <remarks>For ChildPosition.Pre, the element is added to the position 0 of the list, for ChildPosition.Post is added at the end</remarks>
        /// <param name="position"></param>
        /// <param name="child"></param>
        /// <returns>Returns true if the element could be added and did not exist yet. Otherwise false</returns>
        public bool RegisterElementAt( ChildPosition position, IHighlightableElement child )
        {
            if( child == null ) throw new ArgumentNullException( "child" );

            if( position == ChildPosition.Pre )
            {
                if( _preChildren.Contains( child ) ) return false;
                _preChildren.Insert( 0, child );
                return true;
            }
            else if( position == ChildPosition.Post )
            {
                if( _postChildren.Contains( child ) ) return false;
                _postChildren.Add( child );
                return true;
            }
            return false;
        }

        public bool UnregisterElement( ChildPosition positionToRemove, IHighlightableElement element )
        {
            if( positionToRemove == ChildPosition.Pre )
            {
                return _preChildren.Remove( element );
            }
            else if( positionToRemove == ChildPosition.Post )
            {
                return _postChildren.Remove( element );
            }
            return false;
        }

        public string Name
        {
            get { return _name; }
        }

        public IReadOnlyList<IHighlightableElement> PreChildren
        {
            get { return _preChildren.ToReadOnlyList(); }
        }

        public IReadOnlyList<IHighlightableElement> PostChildren
        {
            get { return _postChildren.ToReadOnlyList(); }
        }

        #endregion

        #region IHighlightableElement Members

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return _preChildren.Union( _element.Children ).Union( _postChildren ).ToReadOnlyList(); }
        }

        public int X
        {
            get { return _element.X; }
        }

        public int Y
        {
            get { return _element.Y; }
        }

        public int Width
        {
            get { return _element.Width; }
        }

        public int Height
        {
            get { return _element.Height; }
        }

        public SkippingBehavior Skip
        {
            get { return _element.Skip; }
        }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            if( _highlightPrePostChildren )
            {
                foreach( var p in _preChildren ) p.BeginHighlight( beginScrollingInfo, scrollingDirective );
                foreach( var p in _postChildren ) p.BeginHighlight( beginScrollingInfo, scrollingDirective );
            }
            return _element.BeginHighlight( beginScrollingInfo, scrollingDirective );
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            if( _highlightPrePostChildren )
            {
                foreach( var p in _preChildren ) p.EndHighlight( endScrollingInfo, scrollingDirective );
                foreach( var p in _postChildren ) p.EndHighlight( endScrollingInfo, scrollingDirective );
            }
            return _element.EndHighlight( endScrollingInfo, scrollingDirective );
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            return _element.SelectElement( scrollingDirective );
        }

        public bool IsHighlightableTreeRoot
        {
            get { return _element.IsHighlightableTreeRoot; }
        }

        #endregion
    }
}
