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
* Copyright © 2007-2012, 
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
    public class ExtensibleHighlightableElementProxy : IExtensibleHighlightableElement, IVisualizableHighlightableElement
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

        #region IVizualizableHighlightableElement Members


        public string VectorImagePath
        {
            get { return "F1M-2116.48,389.459L-2127.55,389.459C-2127.29,389.197 -2127.24,388.902 -2127.64,388.668 -2128.71,388.05 -2130.25,387.307 -2130.75,386.086 -2131.07,385.309 -2129.99,384.83 -2129.47,384.539 -2127.57,383.468 -2125.35,382.791 -2123.91,381.063 -2122.9,379.865 -2125.39,379.822 -2126,380.546 -2127.65,382.528 -2130.49,382.878 -2132.4,384.533 -2134.63,386.455 -2132.41,388.285 -2130.54,389.459L-2146.61,389.459C-2148.49,389.459,-2150.01,390.985,-2150.01,392.864L-2150.01,407.108C-2150.01,408.988,-2148.49,410.513,-2146.61,410.513L-2116.48,410.513C-2114.6,410.513,-2113.07,408.988,-2113.07,407.108L-2113.07,392.864C-2113.07,390.985,-2114.6,389.459,-2116.48,389.459z M-2129.98,393.488C-2129.98,393.147,-2129.71,392.872,-2129.36,392.872L-2127.19,392.872C-2126.85,392.872,-2126.57,393.147,-2126.57,393.488L-2126.57,395.665C-2126.57,396.005,-2126.85,396.282,-2127.19,396.282L-2129.36,396.282C-2129.71,396.282,-2129.98,396.005,-2129.98,395.665L-2129.98,393.488z M-2129.98,399.074C-2129.98,398.734,-2129.71,398.457,-2129.36,398.457L-2127.19,398.457C-2126.85,398.457,-2126.57,398.734,-2126.57,399.074L-2126.57,401.25C-2126.57,401.592,-2126.85,401.868,-2127.19,401.868L-2129.36,401.868C-2129.71,401.868,-2129.98,401.592,-2129.98,401.25L-2129.98,399.074z M-2135.52,393.488C-2135.52,393.147,-2135.25,392.872,-2134.9,392.872L-2132.73,392.872C-2132.39,392.872,-2132.11,393.147,-2132.11,393.488L-2132.11,395.665C-2132.11,396.005,-2132.39,396.282,-2132.73,396.282L-2134.9,396.282C-2135.25,396.282,-2135.52,396.005,-2135.52,395.665L-2135.52,393.488z M-2135.52,399.074C-2135.52,398.734,-2135.25,398.457,-2134.9,398.457L-2132.73,398.457C-2132.39,398.457,-2132.11,398.734,-2132.11,399.074L-2132.11,401.25C-2132.11,401.592,-2132.39,401.868,-2132.73,401.868L-2134.9,401.868C-2135.25,401.868,-2135.52,401.592,-2135.52,401.25L-2135.52,399.074z M-2141.06,393.488C-2141.06,393.147,-2140.78,392.872,-2140.44,392.872L-2138.27,392.872C-2137.93,392.872,-2137.65,393.147,-2137.65,393.488L-2137.65,395.665C-2137.65,396.005,-2137.93,396.282,-2138.27,396.282L-2140.44,396.282C-2140.78,396.282,-2141.06,396.005,-2141.06,395.665L-2141.06,393.488z M-2141.06,399.074C-2141.06,398.734,-2140.78,398.457,-2140.44,398.457L-2138.27,398.457C-2137.93,398.457,-2137.65,398.734,-2137.65,399.074L-2137.65,401.25C-2137.65,401.592,-2137.93,401.868,-2138.27,401.868L-2140.44,401.868C-2140.78,401.868,-2141.06,401.592,-2141.06,401.25L-2141.06,399.074z M-2143.19,406.897C-2143.19,407.236,-2143.47,407.514,-2143.81,407.514L-2145.98,407.514C-2146.32,407.514,-2146.6,407.236,-2146.6,406.897L-2146.6,404.72C-2146.6,404.378,-2146.32,404.102,-2145.98,404.102L-2143.81,404.102C-2143.47,404.102,-2143.19,404.378,-2143.19,404.72L-2143.19,406.897z M-2143.19,401.25C-2143.19,401.592,-2143.47,401.868,-2143.81,401.868L-2145.98,401.868C-2146.32,401.868,-2146.6,401.592,-2146.6,401.25L-2146.6,399.074C-2146.6,398.734,-2146.32,398.457,-2145.98,398.457L-2143.81,398.457C-2143.47,398.457,-2143.19,398.734,-2143.19,399.074L-2143.19,401.25z M-2143.19,395.665C-2143.19,396.005,-2143.47,396.282,-2143.81,396.282L-2145.98,396.282C-2146.32,396.282,-2146.6,396.005,-2146.6,395.665L-2146.6,393.488C-2146.6,393.147,-2146.32,392.872,-2145.98,392.872L-2143.81,392.872C-2143.47,392.872,-2143.19,393.147,-2143.19,393.488L-2143.19,395.665z M-2121.03,406.897C-2121.03,407.236,-2121.31,407.514,-2121.65,407.514L-2121.93,407.514 -2123.83,407.514 -2127.19,407.514 -2129.36,407.514 -2132.73,407.514 -2134.9,407.514 -2140.44,407.514C-2140.78,407.514,-2141.06,407.236,-2141.06,406.897L-2141.06,404.72C-2141.06,404.378,-2140.78,404.102,-2140.44,404.102L-2134.9,404.102 -2132.73,404.102 -2129.36,404.102 -2127.19,404.102 -2123.83,404.102 -2121.93,404.102 -2121.65,404.102C-2121.31,404.102,-2121.03,404.378,-2121.03,404.72L-2121.03,406.897z M-2121.03,401.25C-2121.03,401.592,-2121.31,401.868,-2121.65,401.868L-2123.83,401.868C-2124.17,401.868,-2124.44,401.592,-2124.44,401.25L-2124.44,399.074C-2124.44,398.734,-2124.17,398.457,-2123.83,398.457L-2121.65,398.457C-2121.31,398.457,-2121.03,398.734,-2121.03,399.074L-2121.03,401.25z M-2121.03,395.665C-2121.03,396.005,-2121.31,396.282,-2121.65,396.282L-2123.83,396.282C-2124.17,396.282,-2124.44,396.005,-2124.44,395.665L-2124.44,393.488C-2124.44,393.147,-2124.17,392.872,-2123.83,392.872L-2121.65,392.872C-2121.31,392.872,-2121.03,393.147,-2121.03,393.488L-2121.03,395.665z M-2115.49,406.897C-2115.49,407.236,-2115.77,407.514,-2116.11,407.514L-2118.29,407.514C-2118.63,407.514,-2118.9,407.236,-2118.9,406.897L-2118.9,404.72C-2118.9,404.378,-2118.63,404.102,-2118.29,404.102L-2116.11,404.102C-2115.77,404.102,-2115.49,404.378,-2115.49,404.72L-2115.49,406.897z M-2115.49,401.25C-2115.49,401.592,-2115.77,401.868,-2116.11,401.868L-2118.29,401.868C-2118.63,401.868,-2118.9,401.592,-2118.9,401.25L-2118.9,399.074C-2118.9,398.734,-2118.63,398.457,-2118.29,398.457L-2116.11,398.457C-2115.77,398.457,-2115.49,398.734,-2115.49,399.074L-2115.49,401.25z"; }
        }

        #endregion

        #region IVizualizableHighlightableElement Members

        public string ElementName
        {
            get { return Name; }
        }

        #endregion
    }
}
