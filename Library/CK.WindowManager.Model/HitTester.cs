#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WindowManager.Model\HitTester.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Windows;
using System.Linq;

namespace CK.WindowManager.Model
{
    public class HitTester
    {
        HitTestResult _lastResult;
        bool _lock;

        public IBinding LastResult
        {
            get { return _lastResult; }
        }

        public bool CanTest
        {
            get { return _lock == false; }
        }

        public void Block()
        {
            _lock = true;
        }

        public void Release()
        {
            _lock = false;
        }

        public IBinding Test( ISpatialBinding binding, IDictionary<IWindowElement, Rect> setToChallenge, double radius )
        {
            if( _lock == true ) throw new ApplicationException( "You must check the state before perform a hit test" );

            Rect bindingRect = setToChallenge[binding.Window];

            ICollection<IWindowElement> boundWindows = binding.AllDescendants().Select( x => x.Window ).ToArray();
            IWindowElement otherWindow = null;

            Rect r = Rect.Empty;
            foreach( var item in setToChallenge )
            {
                otherWindow = item.Key;
                // If in all registered windows a window intersect with the one that moved
                if( otherWindow != binding.Window && !boundWindows.Contains( otherWindow ) )
                {
                    r = setToChallenge[otherWindow];
                    if( !r.IntersectsWith( bindingRect ) )
                    {
                        //TOP
                        Rect rectTop = new Rect( r.X, r.Y - radius, r.Width, radius );
                        //BOTTOM
                        Rect rectBot = new Rect( r.X, r.Y + r.Height, r.Width, radius );
                        //LEFT
                        Rect rectLeft = new Rect( r.X - radius, r.Y, radius, r.Height );
                        //RIGHT
                        Rect rectRight = new Rect( r.X + r.Width, r.Y, radius, r.Height );
                        if( rectTop.IntersectsWith( bindingRect ) )
                        {
                            _lastResult = new HitTestResult( binding.Window, otherWindow, BindingPosition.Top );
                            return _lastResult;
                        }
                        else if( rectBot.IntersectsWith( bindingRect ) )
                        {
                            _lastResult = new HitTestResult( binding.Window, otherWindow, BindingPosition.Bottom );
                            return _lastResult;
                        }
                        else if( rectLeft.IntersectsWith( bindingRect ) )
                        {
                            _lastResult = new HitTestResult( binding.Window, otherWindow, BindingPosition.Left );
                            return _lastResult;
                        }
                        else if( rectRight.IntersectsWith( bindingRect ) )
                        {
                            _lastResult = new HitTestResult( binding.Window, otherWindow, BindingPosition.Right );
                            return _lastResult;
                        }
                    }
                }
            }
            //RegionHelper region = CreateRegion( rect, radius );

            //IWindowElement matchedWindow = null;
            //Rect intersectArea = GetIntersectionArea( binding, setToChallenge,  rect, ref enlargedArea, out matchedWindow );
            //if( intersectArea != Rect.Empty )
            //{
            //    Debug.Assert( matchedWindow != null );
            //    _lastResult = new HitTestResult( matchedWindow, binding.Window, enlargedArea, intersectArea );
            //    return _lastResult;
            //}
            return null;
        }

        static private Rect GetIntersectionArea( ISpatialBinding binding, IDictionary<IWindowElement, Rect> setToChallenge, Rect rect, ref Rect enlargedRectangle, out IWindowElement otherWindow )
        {
            ICollection<IWindowElement> boundWindows = binding.AllDescendants().Select( x => x.Window ).ToArray();
            otherWindow = null;
            Rect rectWindow = setToChallenge[binding.Window];
            foreach( var item in setToChallenge )
            {
                otherWindow = item.Key;
                // If in all registered windows a window intersect with the one that moved
                if( otherWindow != binding.Window && !boundWindows.Contains( otherWindow ) )
                {
                    rect = setToChallenge[otherWindow];
                    if( !rectWindow.IntersectsWith( rect ) && rect.IntersectsWith( enlargedRectangle ) ) return Rect.Intersect( enlargedRectangle, rect );
                }
            }
            return Rect.Empty;
        }

        class HitTestResult : IBinding
        {
            //public HitTestResult( IWindowElement windowElement, IWindowElement matchedWindow, Rect enlargedRectangle, Rect overlapRectangle )
            //{
            //    Target = windowElement;
            //    Origin = matchedWindow;

            //    if( overlapRectangle.Bottom == enlargedRectangle.Bottom && overlapRectangle.Height != enlargedRectangle.Height ) Position = BindingPosition.Top;
            //    else if( overlapRectangle.Top == enlargedRectangle.Top && overlapRectangle.Height != enlargedRectangle.Height ) Position = BindingPosition.Bottom;
            //    else if( overlapRectangle.Right == enlargedRectangle.Right && overlapRectangle.Width != enlargedRectangle.Width ) Position = BindingPosition.Left;
            //    else if( overlapRectangle.Left == enlargedRectangle.Left && overlapRectangle.Width != enlargedRectangle.Width ) Position = BindingPosition.Right;
            //    else Position = BindingPosition.None;
            //}

            public HitTestResult( IWindowElement origin, IWindowElement target, BindingPosition position )
            {
                Target = target;
                Origin = origin;
                Position = position;
            }

            public BindingPosition Position { get; set; }

            public IWindowElement Target { get; set; }

            public IWindowElement Origin { get; set; }
        }
    }
}
