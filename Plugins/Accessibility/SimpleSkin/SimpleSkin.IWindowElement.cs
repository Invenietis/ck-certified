using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.WindowManager.Model;

namespace SimpleSkin
{
    public partial class SimpleSkin
    {
        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        WindowManagerSubscriber _subscriber;

        partial void OnSuccessfulStart()
        {
            _subscriber = new WindowManagerSubscriber( WindowManager, WindowBinder );
            _skinDispatcher.BeginInvoke( new Action( () =>
            {
                _subscriber.Subscribe( "Skin", _skinWindow );
            } ) );
        }

        partial void OnSuccessfulStop()
        {
            _subscriber.Unsubscribe();
        }
    }
}
