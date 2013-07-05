#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host.Services\Impl\Notifications\NotificationManager.cs) is part of CiviKey. 
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
using CK.Plugin;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace Host.Services
{
    public class NotificationManager : INotificationService, INotifierContext
    {
        public event EventHandler<NotificationArgs> Show;

        public event EventHandler Hide;

        void INotifierContext.FireHide()
        {
            if( Hide != null )
                Hide( this, EventArgs.Empty );
        }

        private UIElement GetDefaultContent( string title, string content )
        {
            UIElement e = null;
            Application.Current.Dispatcher.Invoke( ( (Action)( () => e = new DefaultContent( title, content ) ) ), null );
            return e;
        }

        public IDisposable ShowNotification( Guid pluginId, string title, string content, int duration )
        {
            return ShowCustomBaloon( pluginId, GetDefaultContent( title, content ), duration, null, NotificationTypes.None );
        }

        public IDisposable ShowNotification( Guid pluginId, string title, string content, int duration, Action<object, EventArgs> imageClickAction )
        {
            return ShowCustomBaloon( pluginId, GetDefaultContent( title, content ), duration, imageClickAction, NotificationTypes.None );
        }

        public IDisposable ShowNotification( Guid pluginId, string title, string content, int duration, NotificationTypes notificationType )
        {
            return ShowCustomBaloon( pluginId, GetDefaultContent( title, content ), duration, null, notificationType );
        }

        public IDisposable ShowNotification( Guid pluginId, string title, string content, int duration, NotificationTypes notificationType, Action<object, EventArgs> imageClickAction )
        {
            return ShowCustomBaloon( pluginId, GetDefaultContent( title, content ), duration, imageClickAction, notificationType );
        }

        /// <summary>
        /// Creates a notification by directly providing a UIElement. MAKE SURE THE UIELEMENT HAS BEEN CREATED BY THE APPLICATION'S MAIN THREAD.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="content"></param>
        /// <param name="duration"></param>
        /// <param name="notificationType"></param>
        /// <param name="imageClickAction"></param>
        /// <returns></returns>
        public IDisposable ShowNotification( Guid pluginId, UIElement content, int duration, NotificationTypes notificationType, Action<object, EventArgs> imageClickAction )
        {
            return ShowCustomBaloon( pluginId, content, duration, imageClickAction, notificationType );
        }

        public void ShowNotification( Guid pluginId, Exception ex )
        {
            ShowCustomBaloon( pluginId, new ExceptionContent( ex ), 0, null, NotificationTypes.Error );
        }

        NotificationHandle ShowCustomBaloon( Guid pluginId, UIElement content, int duration, Action<object, EventArgs> imageClickAction, NotificationTypes notificationType )
        {
            if( content != null )
            {

                string imagePath = null;
                if( pluginId != Guid.Empty ) imagePath = "../../../Images/defaultPluginIcon.png";
                else imagePath = "../../../Images/logo.png";
                NotificationWrapper notif = new NotificationWrapper( content, imagePath );
                notif.NotificationType = notificationType;
                notif.OnImageClickAction = imageClickAction;

                if( duration >= 500 )
                {
                    if( Show != null )
                        Show( this, new NotificationArgs( notif, PopupAnimation.Slide, duration ) );
                }
                else
                {
                    if( Show != null )
                    {
                        Show( this, new NotificationArgs( notif, PopupAnimation.Slide, null ) );
                        return new NotificationHandle( this );
                    }
                }

            }

            return null;
        }

    }
}
