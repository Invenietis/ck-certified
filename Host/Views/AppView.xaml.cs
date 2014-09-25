#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\Views\AppView.xaml.cs) is part of CiviKey. 
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
using System.Windows;

namespace Host
{
    /// <summary>
    /// Interaction logic for AppView.xaml
    /// </summary>
    public partial class AppView : Window
    {
        public AppView()
        {
            InitializeComponent();
        }

        protected override void OnClosing( System.ComponentModel.CancelEventArgs e )
        {
            //if( taskbarIcon != null )
            //{
            //    if(taskbarIcon.Visibility == System.Windows.Visibility.Visible)
            //        taskbarIcon.Visibility = System.Windows.Visibility.Collapsed;
            //    taskbarIcon.Dispose();
            //}
            base.OnClosing( e );
        }

        /// <summary>
        /// Used to launch WPF UI Thread when the window goes from the minimized state to the normal state, on XP.
        /// Without that, half the window remains black until it is resized.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated( EventArgs e )
        {
            this.UpdateLayout();
            base.OnActivated( e );
        }
    }
}
