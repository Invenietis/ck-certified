using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using Host.Services;
using System.Threading;
using CK.Context;
using System.Diagnostics;
using Caliburn.Micro;

namespace Sample
{
    [Plugin(Sample.Identifier, Version=Sample.Version, PublicName="Sample plugin")]
    public class Sample : IPlugin
    {
        const string Identifier = "{BF236A99-B2BF-412E-8E57-6F7ADECB9DA4}";
        const string Version = "1.0.0";

        IWindowManager _wnd;
        SampleViewModel _vm;

        public IPluginConfigAccessor Configuration { get; set; }

        [RequiredService]
        public INotificationService Notification { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            Notification.ShowNotification( new Guid( Identifier ), "Sample message", "Sample is running", 2000, NotificationTypes.Ok );

            Configuration[Context]["myKey"] = "myValue";
            Debug.Assert( Configuration.Context["myKey"].ToString() == "myValue" );

            _vm = new SampleViewModel();

            _wnd = new WindowManager();
            _wnd.Show( _vm );
        }

        public void Stop()
        {
            Notification.ShowNotification( new Guid( Identifier ), "Sample message", "Sample is stopping", 2000, NotificationTypes.Ok );
            _vm.TryClose();
        }

        public void Teardown()
        {
            Console.Out.WriteLine( "Sample : Teardown state" );
        }
    }
}
