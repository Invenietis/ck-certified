using System;
using System.Windows;
using CK.Plugin;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls.Primitives;

namespace Host.Services
{
    public class NotificationHandle : IDisposable
    {
        INotifierContext _ctx;

        public NotificationHandle( INotifierContext ctx )
        {
            _ctx = ctx;
        }

        public void CloseNotification()
        {
            _ctx.FireHide();
        }

        public void Dispose()
        {
            CloseNotification();
        }
    }
}
