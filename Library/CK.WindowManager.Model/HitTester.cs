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

            Rect rect = setToChallenge[binding.Window];
            Rect enlargedArea = Rect.Inflate( rect, radius, radius );

            IWindowElement matchedWindow = null;
            Rect intersectArea = GetIntersectionArea( binding, setToChallenge,  rect, ref enlargedArea, out matchedWindow );
            if( intersectArea != Rect.Empty )
            {
                Debug.Assert( matchedWindow != null );
                _lastResult = new HitTestResult( matchedWindow, binding.Window, enlargedArea, intersectArea );
                return _lastResult;
            }
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
            public HitTestResult( IWindowElement windowElement, IWindowElement matchedWindow, Rect enlargedRectangle, Rect overlapRectangle )
            {
                Target = windowElement;
                Origin = matchedWindow;

                if( overlapRectangle.Bottom == enlargedRectangle.Bottom && overlapRectangle.Height != enlargedRectangle.Height ) Position = BindingPosition.Top;
                else if( overlapRectangle.Top == enlargedRectangle.Top && overlapRectangle.Height != enlargedRectangle.Height ) Position = BindingPosition.Bottom;
                else if( overlapRectangle.Right == enlargedRectangle.Right && overlapRectangle.Width != enlargedRectangle.Width ) Position = BindingPosition.Left;
                else if( overlapRectangle.Left == enlargedRectangle.Left && overlapRectangle.Width != enlargedRectangle.Width ) Position = BindingPosition.Right;
                else Position = BindingPosition.None;
            }

            public BindingPosition Position { get; set; }

            public IWindowElement Target { get; set; }

            public IWindowElement Origin { get; set; }
        }
    }
}
