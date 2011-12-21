using System;
using System.Windows;
using CK.Plugin;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls.Primitives;

namespace Host.Services
{
    public class NotificationManager : INotificationService, INotifierContext
    {
        public event EventHandler<NotificationArgs> Show;

        public event EventHandler Hide;

        void INotifierContext.FireHide()
        {
            if( Hide != null ) Hide( this, EventArgs.Empty );
        }

        public IDisposable ShowNotification( Guid pluginId, string title, string content, int duration )
        {
            return ShowCustomBaloon( pluginId, new DefaultContent( title, content ), duration, null, NotificationTypes.None );
        }

        public IDisposable ShowNotification( Guid pluginId, string title, string content, int duration, Action<object, EventArgs> imageClickAction )
        {
            return ShowCustomBaloon( pluginId, new DefaultContent( title, content ), duration, imageClickAction, NotificationTypes.None );
        }

        public IDisposable ShowNotification( Guid pluginId, string title, string content, int duration, NotificationTypes notificationType )
        {
            return ShowCustomBaloon( pluginId, new DefaultContent( title, content ), duration, null, notificationType );
        }

        public IDisposable ShowNotification( Guid pluginId, string title, string content, int duration, NotificationTypes notificationType, Action<object, EventArgs> imageClickAction )
        {
            return ShowCustomBaloon( pluginId, new DefaultContent( title, content ), duration, imageClickAction, notificationType );
        }

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
