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
using KeyboardEditor.Model;

namespace KeyboardEditor.ViewModels
{

    public partial class VMKeyEditable : VMContextElementEditable, IHandleDragDrop
    {
        internal int GetMaximumMovementAmplitude( MoveDirection direction, int pixels )
        {
            switch( direction )
            {
                case MoveDirection.Left:
                    return (X - pixels) > 0 ? pixels : X;
                case MoveDirection.Right:
                    return X + Width + pixels < Context.KeyboardVM.W ? pixels : Context.KeyboardVM.W - X - Width;
                case MoveDirection.Top:
                    return (Y - pixels) > 0 ? pixels : Y;
                case MoveDirection.Bottom:
                    return Y + Height + pixels < Context.KeyboardVM.H ? pixels : Context.KeyboardVM.H - Y - Height;
            }

            return -1;
        }

        internal override void OnMove( MoveDirection direction, int pixels, bool arrangeMovementAmplitude = true )
        {
            if( arrangeMovementAmplitude ) pixels = GetMaximumMovementAmplitude( direction, pixels );

            switch( direction )
            {
                case MoveDirection.Left:
                    X -= pixels;
                    break;
                case MoveDirection.Right:
                    X += pixels;
                    break;
                case MoveDirection.Top:
                    Y -= pixels;
                    break;
                case MoveDirection.Bottom:
                    Y += pixels;
                    break;
                default:
                    break;
            }
        }

        internal override void OnSuppr()
        {
            DeleteKey();
        }

        #region IHandleDragDrop Members

        bool _isDragDropEnabled = true;
        public bool IsDragDropEnabled
        {
            get
            {
                return _isDragDropEnabled;
            }
            set
            {
                _isDragDropEnabled = value;
            }
        }

        public bool CanBeDropTarget( IHandleDragDrop draggedItem )
        {
            return draggedItem is VMKeyEditable && draggedItem != this;
        }

        public bool CanBeDropSource( IHandleDragDrop target )
        {
            return target is VMKeyEditable || target is VMZoneEditable;
        }

        public void ExecuteDropAction( IHandleDragDrop droppedItem )
        {

            if( droppedItem is VMKeyEditable )
            {
                VMKeyEditable actualDroppedItem = (VMKeyEditable)droppedItem;
                if( actualDroppedItem.Model.Zone == Model.Zone )
                {
                    actualDroppedItem.Index = Index;
                }
                else
                {
                    ((VMZoneEditable)Parent).InsertKeyCommand.Execute( actualDroppedItem );
                }
            }
        }

        #endregion
    }
}
