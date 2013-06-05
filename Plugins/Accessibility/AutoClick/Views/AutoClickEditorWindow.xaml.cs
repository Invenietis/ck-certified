#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\Views\AutoClickEditorWindow.xaml.cs) is part of CiviKey. 
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;
using CK.Core;
using CK.Interop;
using CK.Windows.Interop;

namespace CK.Plugins.AutoClick.Views
{
    /// <summary>
    /// Interaction logic for AutoClickEditorWindow.xaml
    /// </summary>
    public partial class AutoClickEditorWindow : Window
    {
        WindowInteropHelper _interopHelper;

        public AutoClickEditorWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += new MouseButtonEventHandler( WPFStdClickTypeWindow_MouseLeftButtonDown );
        }

        void WPFStdClickTypeWindow_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
            DragMove();
        }

        protected override void OnSourceInitialized( EventArgs e )
        {
            _interopHelper = new WindowInteropHelper( this );            

            HwndSource mainWindowSrc = HwndSource.FromHwnd( _interopHelper.Handle );

            mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb( 0, 0, 0, 0 );
            mainWindowSrc.CompositionTarget.RenderMode = RenderMode.Default;

            if( CK.Core.OSVersionInfo.OSLevel >= CK.Core.OSVersionInfo.SimpleOSLevel.WindowsVista && Dwm.Functions.IsCompositionEnabled() )
            {
                Win.Margins m = new Win.Margins() { LeftWidth = -1, RightWidth = -1, TopHeight = -1, BottomHeight = -1 };
                Dwm.Functions.ExtendFrameIntoClientArea( _interopHelper.Handle, ref m );
            }
            else
            {
                this.Background = new SolidColorBrush( Colors.WhiteSmoke );
            }


            base.OnSourceInitialized( e );
        }
    }
}
