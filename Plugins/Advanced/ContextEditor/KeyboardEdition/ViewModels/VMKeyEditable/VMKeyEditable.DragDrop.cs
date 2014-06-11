#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\EditableSkin\ViewModels\VMKeyEditable.cs) is part of CiviKey. 
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
using CK.WPF.ViewModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using CommonServices;

namespace KeyboardEditor.ViewModels
{
    public partial class VMKeyEditable : VMContextElementEditable
    {
        const int thumbWidth = 10;
        const int thumbHeight = 10;

        private bool _isDown;
        private Point _startPoint;
        private bool _isDragging;

        int _originalLeft;
        int _originalTop;

        #region Commands

        public VMCommand<DragDeltaEventArgs> HandleBottomRightCommand
        {
            get
            {
                return new VMCommand<DragDeltaEventArgs>( ( args ) =>
                {
                    HandleBottomRight( args );
                } );
            }
        }

        public VMCommand<DragDeltaEventArgs> HandleBottomLeftCommand
        {
            get
            {
                return new VMCommand<DragDeltaEventArgs>( ( args ) =>
                {
                    HandleBottomLeft( args );
                } );
            }
        }

        public VMCommand<DragDeltaEventArgs> HandleTopRightCommand
        {
            get
            {
                return new VMCommand<DragDeltaEventArgs>( ( args ) =>
                {
                    HandleTopRight( args );
                } );
            }
        }

        public VMCommand<DragDeltaEventArgs> HandleTopLeftCommand
        {
            get
            {
                return new VMCommand<DragDeltaEventArgs>( ( args ) =>
                {
                    HandleTopLeft( args );
                } );
            }
        }

        public VMCommand<MouseEventArgs> MouseLeftButtonDownCommand
        {
            get
            {
                return new VMCommand<MouseEventArgs>( ( args ) =>
                {
                    _isDown = true;

                    _startPoint = new Point( _context.PointerDeviceDriver.Service.CurrentPointerXLocation, _context.PointerDeviceDriver.Service.CurrentPointerYLocation );
                    _originalLeft = X;
                    _originalTop = Y;

                    IsSelected = true;
                    //Context.SelectedElement = this;
                } );
            }
        }

        public VMCommand<MouseEventArgs> MouseLeftButtonUpCommand
        {
            get
            {
                return new VMCommand<MouseEventArgs>( ( args ) =>
                {
                    //Console.Out.WriteLine( "ButtonUp from control" );
                    StopDragging();
                } );
            }
        }

        #endregion

        #region OnXXX

        public void OnPointerButtonUp( PointerDeviceEventArgs args )
        {
            //Console.Out.WriteLine( "ButtonUp from Context" );
            StopDragging();
        }

        void OnMouseMove( PointerDeviceEventArgs e )
        {
            if( _isDown && _context.SelectedElement == this )
            {
                Point position = new Point( _context.PointerDeviceDriver.Service.CurrentPointerXLocation, _context.PointerDeviceDriver.Service.CurrentPointerYLocation );

                if( (_isDragging == false) &&
                    ((Math.Abs( position.X - _startPoint.X ) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs( position.Y - _startPoint.Y ) > SystemParameters.MinimumVerticalDragDistance)) )
                    _isDragging = true;

                if( _isDragging )
                {
                    X = (int)(position.X - (_startPoint.X - _originalLeft));
                    Y = (int)(position.Y - (_startPoint.Y - _originalTop));
                }
            }
        }

        #endregion

        #region Methods

        private void StopDragging()
        {
            if( _isDown )
            {
                _isDown = false;
                _isDragging = false;
            }
        }

        // Handler for resizing from the bottom-right.
        void HandleBottomRight( DragDeltaEventArgs args )
        {
            // Change the size by the amount the user drags the mouse, as long as it's larger 
            // than the width or height of an adorner, respectively.
            Width = (int)Math.Max( Width + args.HorizontalChange, thumbWidth );
            Height = (int)Math.Max( args.VerticalChange + Height, thumbHeight );
        }

        // Handler for resizing from the top-right.
        void HandleTopRight( DragDeltaEventArgs args )
        {

            // Change the size by the amount the user drags the mouse, as long as it's larger 
            // than the width or height of an adorner, respectively.
            Width = (int)Math.Max( Width + args.HorizontalChange, thumbWidth );
            //adornedElement.Height = Math.Max(adornedElement.Height - args.VerticalChange, thumbHeight);

            int height_old = Height;
            int height_new = (int)Math.Max( Height - args.VerticalChange, thumbHeight );

            Height = (int)height_new;
            int top_old = Y;
            Y = (int)(top_old - (height_new - height_old));
        }

        // Handler for resizing from the top-left.
        void HandleTopLeft( DragDeltaEventArgs args )
        {
            int width_old = Width;
            int width_new = (int)Math.Max( Width - args.HorizontalChange, thumbWidth );

            //adornedElement.Width = width_new;
            Width = (int)width_new;

            int left_old = X;
            X = (int)(left_old - (width_new - width_old));

            int height_old = Height;
            int height_new = (int)Math.Max( Height - args.VerticalChange, thumbHeight );
            Height = (int)height_new;

            int top_old = Y;
            Y = (int)(top_old - (height_new - height_old));
        }

        // Handler for resizing from the bottom-left.
        void HandleBottomLeft( DragDeltaEventArgs args )
        {

            // Change the size by the amount the user drags the mouse, as long as it's larger 
            // than the width or height of an adorner, respectively.
            //adornedElement.Width = Math.Max(adornedElement.Width - args.HorizontalChange, thumbWidth);
            Height = (int)Math.Max( args.VerticalChange + Height, thumbHeight );

            int width_old = Width;
            int width_new = (int)Math.Max( Width - args.HorizontalChange, thumbWidth );
            Width = (int)width_new;

            int left_old = X;
            X = (int)(left_old - (width_new - width_old));
        }

        #endregion
    }
}
