using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CommonServices;
using System.Windows;
using CK.Core;

namespace HostNotificationManager
{
    [Plugin("{0F5C1D60-FEEB-43F3-B384-FC8462EEC79C}",Categories=new string[]{"Advanced"},
        PublicName="Host notification manager", Version="1.0.0"
        )]
    public class HostNotificationManager : IPlugin, INotificationService
    {
        const string PluginIdString = "{0F5C1D60-FEEB-43F3-B384-FC8462EEC79C}";
        const string PluginIdVersion = "1.0.0";
        public static readonly IVersionedUniqueId PluginId = new SimpleVersionedUniqueId( PluginIdString, PluginIdVersion );

        bool _isEnabled;

        public void Start()
        {
            
        }

        public void Stop()
        {
            _isEnabled = false;
        }

        public event EventHandler<NotificationEventArgs>  NotificationRequest;

        void FireShowNotification( NotificationEventArgs notificationParameters, IPlugin plugin )
        {
            if( _isEnabled && notificationParameters != null && NotificationRequest != null )
                NotificationRequest( plugin, notificationParameters );
        }

        #region IPlugin Members

        public bool CanStart( out string lastError )
        {
            lastError = "";
            return true;
        }

        public bool Setup( IPluginSetupInfo info )
        {
            _isEnabled = true;
            return true;
        }

        public void Teardown()
        {
            
        }

        #endregion

        void INotificationService.ShowNotification( IPlugin plugin, string title, string content, int duration )
        {
            FireShowNotification( new NotificationEventArgs( title, content, duration ), plugin );
        }

        void INotificationService.ShowNotification( IPlugin plugin, string title, string content, int duration, Action<object, EventArgs> imageClickAction )
        {
            FireShowNotification( new NotificationEventArgs( title, content, duration, imageClickAction ), plugin );
        }

        void INotificationService.ShowNotification( IPlugin plugin, string title, string content, int duration, NotificationTypes notifType )
        {
            FireShowNotification( new NotificationEventArgs( title, content, duration, notifType ), plugin );
        }

        void INotificationService.ShowNotification( IPlugin plugin, string title, string content, int duration, Action<object, EventArgs> imageClickAction, NotificationTypes notifType )
        {
            FireShowNotification( new NotificationEventArgs( title, content, duration, imageClickAction, notifType ), plugin );
        }

        void INotificationService.ShowNotification( IPlugin plugin, Exception ex )
        {
            FireShowNotification( new NotificationEventArgs( ex ), plugin );
        }

        void INotificationService.ShowNotification( IPlugin plugin, UIElement content, int duration, Action<object, EventArgs> imageClickAction, NotificationTypes notifType )
        {
            FireShowNotification( new NotificationEventArgs( content, duration, imageClickAction, notifType ), plugin );
        }

        void INotificationService.ShowNotification( IPlugin plugin, NotificationEventArgs args )
        {
            FireShowNotification( args, plugin );
        }
    }
}
