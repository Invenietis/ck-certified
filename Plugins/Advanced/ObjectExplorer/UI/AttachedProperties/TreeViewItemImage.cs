#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\UI\AttachedProperties\TreeViewItemImage.cs) is part of CiviKey. 
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

using System.Windows;
using System.Windows.Media;

namespace CK.Plugins.ObjectExplorer
{
    public class TreeViewItemImage
    {
        public static ImageSource GetImage( DependencyObject obj )
        {
            return (ImageSource)obj.GetValue( ImageProperty );
        }

        public static void SetImage( DependencyObject obj, ImageSource value )
        {
            obj.SetValue( ImageProperty, value );
        }

        public static readonly DependencyProperty ImageProperty =
        DependencyProperty.RegisterAttached( "Image", typeof( ImageSource ), typeof( TreeViewItemImage ) );
    }
}
