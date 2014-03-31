using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace BasicCommandHandlers
{
    public class RegionHelper
    {
        Region _region;
        List<Rectangle> _rectangles;

        public Region Region
        {
            get { return _region;  }
        }

        public RegionHelper()
        {
            _rectangles = new List<Rectangle>();
        }

        public RegionHelper( Rectangle rectangle )
        {
            Add( rectangle );
        }

        public RegionHelper( IEnumerable<Rectangle> rectangles )
            : this()
        {
            Add( rectangles );
        }

        public void Add( Rectangle rectangle )
        {
            if( _region == null )
            {
                _region = new Region( rectangle );
                _rectangles.Add( rectangle );
            }
            else
            {
                _region.Union( rectangle );
                _rectangles.Add( rectangle );
            }
        }

        public void Add( IEnumerable<Rectangle> rectangles )
        {
            foreach( var r in rectangles ) Add( r );
        }

        public void Remove( Rectangle rectangle )
        {
            if( _region != null )
            {
                _region.Exclude( rectangle );
                _rectangles.Remove( rectangle );
            }
        }

        public void Remove( IEnumerable<Rectangle> rectangles )
        {
            foreach( var r in rectangles ) Remove( r );
        }

        public bool Contains( Point point )
        {
            return _region != null ? _region.IsVisible( point ) : false;
        }

        public bool Contains( Rectangle rectangle )
        {
            return _region != null ? _region.IsVisible( rectangle ) : false;
        }

        public bool ContainsInXBounds( Point point )
        {
            return _rectangles.Any( r => r.Left < point.X && r.Right >= point.X );
        }

        public static bool ContainsInXBounds( Point point, Rectangle rectangleToTest )
        {
            return rectangleToTest.Left <= point.X && rectangleToTest.Right > point.X;
        }

        public bool ContainsInYBounds( Point point )
        {
            return _rectangles.Any( r => r.Top <= point.Y && r.Bottom > point.Y );
        }

        public static bool ContainsInYBounds( Point point, Rectangle rectangleToTest )
        {
            return rectangleToTest.Top <= point.Y && rectangleToTest.Bottom > point.Y;
        }

        public int GetMinYPosition( int X )
        {
            return GetMinPosition( r => r.Left < X && r.Right >= X, r => r.Top );
        }

        public int GetMinXPosition( int Y )
        {
            return GetMinPosition( r => r.Top <= Y && r.Bottom > Y, r => r.Left );
        }

        public int GetMaxYPosition( int X )
        {
            return GetMaxPosition( r => r.Left < X && r.Right >= X, r => r.Bottom );
        }

        public int GetMaxXPosition( int Y )
        {
            return GetMaxPosition( r => r.Top <= Y && r.Bottom > Y, r => r.Right );
        }

        #region Helper Methods

        int GetMinPosition( Func<Rectangle,bool> condition, Func<Rectangle,int> selector )
        {
            IEnumerable<Rectangle> e = _rectangles.Where( condition );
            if( e.FirstOrDefault() != Rectangle.Empty ) return e.Min( selector );
            return 0;
        }

        int GetMaxPosition( Func<Rectangle, bool> condition, Func<Rectangle, int> selector )
        {
            IEnumerable<Rectangle> e = _rectangles.Where( condition );
            if( e.FirstOrDefault() != Rectangle.Empty ) return e.Max( selector );
            return _rectangles.Max( selector );
        }

        #endregion Helper Methods
    }
}
