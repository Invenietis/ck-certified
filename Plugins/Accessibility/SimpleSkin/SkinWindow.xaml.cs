#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\SkinWindow.xaml.cs) is part of CiviKey. 
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
using System.Windows;
using CK.Windows;
using System.Windows.Media;
using SimpleSkin.Helpers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using CK.Interop;
using CK.Windows.Interop;
using System.Diagnostics;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using CK.Windows.Core;
using CK.WPF.StandardViews;

namespace SimpleSkin
{
    /// <summary>
    /// Logique d'interaction pour SkinWindow.xaml
    /// </summary>
    public partial class SkinWindow : CKNoFocusWindow
    {
        public SkinWindow( NoFocusManager noFocusManager )
            : base( noFocusManager )
        {
            InitializeComponent();
        }

        protected override bool IsDraggableVisual( DependencyObject visualElement )
        {
            FrameworkElement border = visualElement as FrameworkElement;
            //Allows drag and drop when the background is set
            if( border != null && border.Name == "InsideBorder" ) return true;
            if( DraggableVisualAttachedProperty.GetDraggableVisual( visualElement ) ) return true;
            var parent = VisualTreeHelper.GetParent( visualElement );
            return parent is SkinWindow || base.IsDraggableVisual( visualElement );
        }

        protected override bool EnableHitTestElementController( DependencyObject visualElement, Point p, int currentHTCode, out IHitTestElementController specialElement )
        {
            specialElement = visualElement as IHitTestElementController;
            return specialElement != null;
        }
    }
}
