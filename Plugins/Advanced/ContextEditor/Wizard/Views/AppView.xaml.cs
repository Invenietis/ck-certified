#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\Views\AppView.xaml.cs) is part of CiviKey. 
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
using KeyboardEditor.Tools;

namespace KeyboardEditor.s
{
    /// <summary>
    /// Interaction logic for AppView.xaml
    /// </summary>
    public partial class AppView : Window
    {
        public event HookInvokedEventHandler HookInvoqued;
        public delegate void HookInvokedEventHandler( object sender, HookInvokedEventArgs e );


        //These are global hooks, which are not what we need for this type of interface.
        //protected override void OnSourceInitialized( EventArgs e )
        //{
        //    base.OnSourceInitialized( e );
        //    HwndSource source = PresentationSource.FromVisual( this ) as HwndSource;
        //    source.AddHook( WndProc );
        //}

        //private IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        //{
        //    if( msg == Constants.WM_HOTKEY_MSG_ID )
        //    {
        //        if( HookInvoqued != null )
        //        {
        //            HookInvoqued( this, new HookInvokedEventArgs( msg, lParam, wParam ) );
        //        }
        //    }
        //    return IntPtr.Zero;
        //}

        public AppView()
        {
            InitializeComponent();
        }
    }
}
