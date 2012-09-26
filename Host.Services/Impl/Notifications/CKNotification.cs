#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host.Services\Impl\Notifications\CKNotification.cs) is part of CiviKey. 
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;

namespace Host.Services
{
    public class CKNotification : UserControl
    {
        internal CKNotification() { }

        public CKNotification( UIElement content, string imagePath )
        {
            NotificationContent = content;
            DataContext = this;
            if( imagePath == null )
                ImageSource = new BitmapImage( new Uri( "../../Resources/Images/logo.png", UriKind.RelativeOrAbsolute ) );
            else
                ImageSource = new BitmapImage( new Uri( imagePath, UriKind.RelativeOrAbsolute ) );
        }

        public NotificationTypes NotificationType { get; set; }

        public UIElement NotificationContent { get; set; }

        public ImageSource ImageSource { get; private set; }

        public Action<object, EventArgs> OnImageClickAction { get; set; }

        protected void CloseNotification( object sender, MouseButtonEventArgs e )
        {
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon( this );
            taskbarIcon.CloseBalloon();
        }

        protected void OnImageClick( object sender, MouseButtonEventArgs e )
        {
            if( OnImageClickAction != null ) OnImageClickAction( sender, e );
        }
    }
}
