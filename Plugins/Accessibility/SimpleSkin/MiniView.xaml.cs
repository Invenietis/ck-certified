#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\MiniView.xaml.cs) is part of CiviKey. 
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
using System.Windows.Interop;
using CK.Interop;
using CK.Windows.Interop;

namespace SimpleSkin
{
    /// <summary>
    /// Interaction logic for MiniView.xaml
    /// </summary>
    public partial class MiniView : Window
    {
        Action _showSkin;
        WindowInteropHelper _interopHelper;
        HwndSourceHook _wndHook;
        IntPtr _lastFocused;
        bool _ncbuttondown;

        public MiniView( Action showSkin )
        {
            InitializeComponent();
            _showSkin = showSkin;

            move.MouseLeftButtonDown += new MouseButtonEventHandler( OnMoveDown );
        }

        void OnMoveDown( object sender, MouseButtonEventArgs e )
        {
            GetFocus();
            DragMove();
            ReleaseFocus();
        }

        void GetFocus()
        {
            _lastFocused = Win.Functions.GetForegroundWindow();
            Win.Functions.SetForegroundWindow( _interopHelper.Handle );
        }

        void ReleaseFocus()
        {
            Win.Functions.SetForegroundWindow( _lastFocused );
        }

        protected override void OnSourceInitialized( EventArgs e )
        {
            _interopHelper = new WindowInteropHelper( this );

            Win.Functions.SetWindowLong( _interopHelper.Handle, Win.WindowLongIndex.GWL_EXSTYLE, (uint)CK.Windows.Interop.Win.WS_EX_NOACTIVATE );

            HwndSource mainWindowSrc = HwndSource.FromHwnd( _interopHelper.Handle );

            _wndHook = new HwndSourceHook( WndProc );
            mainWindowSrc.AddHook( _wndHook );

            base.OnSourceInitialized( e );
        }

        IntPtr WndProc( IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        {
            switch( msg )
            {
                case Win.WM_NCLBUTTONDOWN:
                    _ncbuttondown = true;
                    GetFocus();
                    break;
                case Win.WM_NCMOUSEMOVE:
                    if( _ncbuttondown )
                    {
                        ReleaseFocus();
                        _ncbuttondown = false;
                    }
                    break;
            }
            return hWnd;
        }

        protected override void OnPreviewMouseDoubleClick( MouseButtonEventArgs e )
        {
            e.Handled = true;
            _showSkin.Invoke();
            base.OnPreviewMouseDoubleClick( e );
        }
    }
}
