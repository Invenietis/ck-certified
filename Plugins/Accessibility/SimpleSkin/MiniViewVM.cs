#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\MiniViewVM.cs) is part of CiviKey. 
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
using CK.Core;
using CK.Plugin.Config;
using CK.Windows.Helpers;
using CK.WPF.ViewModel;
using HighlightModel;

namespace SimpleSkin
{

    public class MiniViewVM : VMBase, IHighlightableElement, IDisposable
    {
        public WindowStateManager Parent { get; set; }
        IPluginConfigAccessor Config { get { return Parent.Config; } }

        bool _isHighlighted;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;
                OnPropertyChanged( "IsHighlighted" );
            }
        }

        public MiniViewVM( WindowStateManager parent )
        {
            _isHighlighted = false;
            Parent = parent;
        }

        public string Name { get { return "Minimized keyboard"; } }

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return CKReadOnlyListEmpty<IHighlightableElement>.Empty; }
        }

        public int X
        {
            get
            {
                var position = Config.Context["MiniViewPositionX"];
                if ( position == null )
                {
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
                    System.Drawing.Point p = ScreenHelper.GetCenterOfParentScreen( rect );

                    return p.X;
                }
                else
                    return ( Int32.Parse( position.ToString() ) );
            }
            set { Config.Context["MiniViewPositionX"] = value; }
        }

        public int Y
        {
            get
            {
                var position = Config.Context["MiniViewPositionY"];
                if ( position == null )
                    return 0;
                else
                    return ( Int32.Parse( position.ToString() ) );
            }
            set { Config.Context["MiniViewPositionY"] = value; }
        }

        int _width = 160;
        public int Width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged( "Width" ); }
        }

        int _height = 160;
        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged( "Height" );
            }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.None; }
        }

        public void Dispose()
        {
        }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            if ( Parent.IsViewHidden )
            {
                IsHighlighted = true;
            }

            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            if ( Parent.IsViewHidden )
            {
                IsHighlighted = false;
            }

            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            if ( Parent.IsViewHidden )
            {
                Parent.RestoreWindows();
            }

            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return true; }
        }
    }
}
