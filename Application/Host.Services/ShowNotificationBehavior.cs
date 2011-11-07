using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace Host.Services
{
    public static class ShowNotificationBehavior
    {
        public static INotifierContext GetNotificationContext( DependencyObject obj )
        {
            return (INotifierContext)obj.GetValue( NotificationContextProperty );
        }

        public static void SetNotificationContext( DependencyObject obj, INotifierContext value )
        {
            obj.SetValue( NotificationContextProperty, value );
        }

        // Using a DependencyProperty as the backing store for NotificationContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotificationContextProperty =
        DependencyProperty.RegisterAttached( 
            "NotificationContext", 
            typeof( INotifierContext ), 
            typeof( ShowNotificationBehavior ),
            new FrameworkPropertyMetadata( OnContextChanged ) 
        );

        static void OnContextChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs e )
        {
            var notifier = GetNotificationContext( depObj );
            notifier.Show += ( o, args ) =>
            {
                var tb = depObj as TaskbarIcon;
                if( tb != null && !tb.IsDisposed )
                {
                    tb.ShowCustomBalloon( args.Content, args.Animation, args.Duration );
                }
            };
            notifier.Hide += ( o, args ) =>
            {
                var tb = depObj as TaskbarIcon;
                if( tb != null )
                {
                    tb.CloseBalloon();
                }
            };
        }
    }
}
