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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;

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

        TaskbarIcon tbi;

        protected override void OnClosing( System.ComponentModel.CancelEventArgs e )
        {
            tbi.Dispose();
            base.OnClosing( e );
        }

        protected override void OnContentRendered( EventArgs e )
        {
            //The Hardcodet taskbar icon crashes on Windows 8.
            //We need ot check the OS version before using it, so we have (for now) to rely on code behind
            //if( CK.Core.OSVersionInfo.OSLevel <= CK.Core.OSVersionInfo.SimpleOSLevel.WindowsVista )
            //{
            //    AppViewModel app = (AppViewModel)DataContext;
            //    tbi = new TaskbarIcon();
            //    ImageSourceConverter imsc = new ImageSourceConverter();

            //    tbi.IconSource = ( (ImageSource)imsc.ConvertFromString( "pack://application:,,,/CiviKey;component/Resources/Icons/logo_16x16.ico" ) );
            //    tbi.ToolTipText = "CiviKey";
            //    tbi.MenuActivation = PopupActivationMode.LeftOrRightClick;

            //    ContextMenu ctm = new System.Windows.Controls.ContextMenu();
            //    Binding visibilityBinding = new Binding( "ShowSystrayIcon" );

            //    visibilityBinding.Converter = new BooleanToVisibilityConverter();
            //    visibilityBinding.Mode = BindingMode.OneWay;
            //    ctm.SetBinding( UIElement.VisibilityProperty, visibilityBinding );

            //    MenuItem mi = new MenuItem();
            //    mi.Header = "Exit";
            //    mi.Command = app.ExitHostCommand;
            //    ctm.Items.Add( mi );
            //    tbi.ContextMenu = ctm;
            //    grid.Children.Add( tbi );
            //}
            //  <tb:TaskbarIcon 
            //    x:Name="taskbarIcon"
            //    IconSource="pack://application:,,,/CiviKey;component/Resources/Icons/logo_16x16.ico"
            //    ToolTipText="CiviKey" 
            //    MenuActivation="LeftOrRightClick"
            //    DoubleClickCommand="{Binding EnsureMainWindowVisibleCommand}"
            //    Visibility="{Binding ShowSystrayIcon, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
            //    ser:ShowNotificationBehavior.NotificationContext="{Binding NotificationCtx}">
            //    <tb:TaskbarIcon.ContextMenu>
            //        <ContextMenu>
            //            <MenuItem Header="Exit" Command="{Binding ExitHostCommand}" />
            //        </ContextMenu>
            //    </tb:TaskbarIcon.ContextMenu>
            //</tb:TaskbarIcon>

            base.OnInitialized( e );
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
