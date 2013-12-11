using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ScreenScroller
{
    /// <summary>
    /// This class overrides the Arrange method of the UniformGrid in order to arrange its children in a "track" way.
    /// instead of arranging them in the following way :  1 2 3
    ///                                                   4 5 6
    ///                                                   7 8 9
    ///                                                  
    ///                 this Grid arranges the that way : 1 2 3
    ///                                                   6 5 4
    ///                                                   7 8 9
    /// It works for any number of children.
    /// </summary>
    public class TrackUniformGrid : UniformGrid
    {
        protected override Size ArrangeOverride( Size arrangeSize )
        {
            UpdateComputedValues();

            Rect childBounds = new Rect( 0, 0, arrangeSize.Width / _columns, arrangeSize.Height / _rows );
            double xStep = childBounds.Width;
            double xBound = arrangeSize.Width - 1.0;
            int rowIndex = 1;

            childBounds.X += childBounds.Width * FirstColumn;


            // Arrange and Position each child to the same cell size
            foreach( UIElement child in InternalChildren )
            {
                child.Arrange( childBounds );

                // only advance to the next grid cell if the child was not collapsed
                bool isAnEvenRowIndex = ( rowIndex % 2 == 0 );
                if( isAnEvenRowIndex )
                {
                    childBounds.X -= xStep; //we are going backwards
                    if( childBounds.X < -0.5 ) //handle -0.00000000001 values. pretty ugly fix, i have to admit it
                    {
                        childBounds.Y += childBounds.Height;
                        childBounds.X = 0;//The next row will be a classic one, so we go back to the left side of the grid
                        rowIndex++;
                    }
                }
                else
                {
                    childBounds.X += xStep; //we are going forward
                    if( childBounds.X >= xBound )
                    {
                        childBounds.Y += childBounds.Height;
                        childBounds.X -= xStep; //The next row will be a reverse one, we go back to where we were before incrementing the X.
                        rowIndex++;
                    }
                }
            }

            return arrangeSize;
        }

        #region Microsoft Code

        /// <summary>
        /// Directly taken from the underlying UniformGrid.
        /// Compute the default values for Column & Rows if they are not set.
        /// </summary>
        private void UpdateComputedValues()
        {
            _columns = Columns;
            _rows = Rows;

            //parameter checking.
            if( FirstColumn >= _columns )
            {
                //NOTE: maybe we shall throw here. But this is somewhat out of
                //the MCC itself. We need a whole new panel spec.
                FirstColumn = 0;
            }

            if( ( _rows == 0 ) || ( _columns == 0 ) )
            {
                int nonCollapsedCount = 0;

                // First compute the actual # of non-collapsed children to be laid out
                for( int i = 0, count = InternalChildren.Count; i < count; ++i )
                {
                    UIElement child = InternalChildren[i];
                    if( child.Visibility != Visibility.Collapsed )
                    {
                        nonCollapsedCount++;
                    }
                }

                // to ensure that we have at leat one row & column, make sure
                // that nonCollapsedCount is at least 1
                if( nonCollapsedCount == 0 )
                {
                    nonCollapsedCount = 1;
                }

                if( _rows == 0 )
                {
                    if( _columns > 0 )
                    {
                        // take FirstColumn into account, because it should really affect the result
                        _rows = ( nonCollapsedCount + FirstColumn + ( _columns - 1 ) ) / _columns;
                    }
                    else
                    {
                        // both rows and columns are unset -- lay out in a square
                        _rows = (int)Math.Sqrt( nonCollapsedCount );
                        if( ( _rows * _rows ) < nonCollapsedCount )
                        {
                            _rows++;
                        }
                        _columns = _rows;
                    }
                }
                else if( _columns == 0 )
                {
                    // guaranteed that _rows is not 0, because we're in the else clause of the check for _rows == 0
                    _columns = ( nonCollapsedCount + ( _rows - 1 ) ) / _rows;
                }
            }
        }
        private int _rows;
        private int _columns;

        #endregion

    }
}
