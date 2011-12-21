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
