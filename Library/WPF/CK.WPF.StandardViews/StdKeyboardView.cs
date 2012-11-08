#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.StandardViews\StdKeyboardView.cs) is part of CiviKey. 
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
using System.Windows.Controls;
using System.Collections;
using System.Windows.Media;

namespace CK.WPF.StandardViews
{
    public class StdKeyboardView : Control
    {
        static StdKeyboardView()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( StdKeyboardView ), new FrameworkPropertyMetadata( typeof( StdKeyboardView ) ) );
        }
        public StdKeyboardView()
        {
            Focusable = false;
        }

        #region Dependency properties

        public IEnumerable Keys
        {
            get { return (IEnumerable)GetValue( KeysProperty ); }
            set { SetValue( KeysProperty, value ); }
        }
        public static readonly DependencyProperty KeysProperty = 
            DependencyProperty.Register( "Keys", typeof( IEnumerable ), typeof( StdKeyboardView ) );

        public object BackgroundImagePath
        {
            get { return (Brush)GetValue( BackgroundImagePathProperty ); }
            set { SetValue( BackgroundImagePathProperty, value ); }
        }
        public static readonly DependencyProperty BackgroundImagePathProperty = 
            DependencyProperty.Register( "BackgroundImagePath", typeof( object ), typeof( StdKeyboardView ) );

        public Brush OutsideBorderColor
        {
            get { return (Brush)GetValue( OutsideBorderColorProperty ); }
            set { SetValue( OutsideBorderColorProperty, value ); }
        }
        public static readonly DependencyProperty OutsideBorderColorProperty = 
            DependencyProperty.Register( "OutsideBorderColor", typeof( Brush ), typeof( StdKeyboardView ) );

        public Thickness OutsideBorderThickness
        {
            get { return (Thickness)GetValue( OutsideBorderThicknessProperty ); }
            set { SetValue( OutsideBorderThicknessProperty, value ); }
        }
        public static readonly DependencyProperty OutsideBorderThicknessProperty = 
            DependencyProperty.Register( "OutsideBorderThickness", typeof( Thickness ), typeof( StdKeyboardView ) );

        public Brush InsideBorderColor
        {
            get { return (Brush)GetValue( InsideBorderColorProperty ); }
            set { SetValue( InsideBorderColorProperty, value ); }
        }
        public static readonly DependencyProperty InsideBorderColorProperty = 
            DependencyProperty.Register( "InsideBorderColor", typeof( Brush ), typeof( StdKeyboardView ) );

        public Thickness InsideBorderThickness
        {
            get { return (Thickness)GetValue( InsideBorderThicknessProperty ); }
            set { SetValue( InsideBorderThicknessProperty, value ); }
        }
        public static readonly DependencyProperty InsideBorderThicknessProperty = 
            DependencyProperty.Register( "InsideBorderThickness", typeof( Thickness ), typeof( StdKeyboardView ) );

        #endregion
    }
}
