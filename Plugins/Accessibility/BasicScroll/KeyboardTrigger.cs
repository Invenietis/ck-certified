using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Plugins.SendInput;
using CommonServices;

namespace BasicScroll
{
    [Plugin( KeyboardTrigger.PluginIdString,
           PublicName = PluginPublicName,
           Version = KeyboardTrigger.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class KeyboardTrigger : IPlugin, ITriggerService
    {
        const string PluginIdString = "{4E3A3B25-7FD0-406F-A958-ECB50AC6A597}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Keyboard Trigger";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardDriver> KeyboardDriver { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ISendStringService> SendStringService { get; set; }

        public IPluginConfigAccessor Configuration { get; set; }

        public event EventHandler Triggered
        {
            add { InternalTriggered += value; ListenToKeyDown = true; }
            remove
            {
                InternalTriggered -= value;
                if( InternalTriggered == null ) ListenToKeyDown = false;
            }
        }

        private bool _listenToKeyDown;
        private bool ListenToKeyDown
        {
            set
            {
                if( _listenToKeyDown != value )
                {
                    _listenToKeyDown = value;
                    if( _listenToKeyDown )
                        KeyboardDriver.Service.KeyDown += OnKeyDown;
                    else
                        KeyboardDriver.Service.KeyDown -= OnKeyDown;
                }
            }
        }

        public event EventHandler InternalTriggered;

        int _keyCode;

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            ListenToKeyDown = true;

            _keyCode = Configuration.User.GetOrSet( "TriggerKeyCode", 222 );
            KeyboardDriver.Service.RegisterCancellableKey( _keyCode );

            Configuration.ConfigChanged += OnConfigChanged;
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( u => u.UniqueId == KeyboardTrigger.PluginId.UniqueId ) && e.Key == "TriggerKeyCode" )
            {
                KeyboardDriver.Service.UnregisterCancellableKey( _keyCode );
                _keyCode = (int)e.Value;
                KeyboardDriver.Service.RegisterCancellableKey( _keyCode );
            }
        }

        void OnKeyDown( object sender, KeyboardDriverEventArg e )
        {
            if( e.KeyCode == _keyCode ) // on spacebar pressed
            {
                if( InternalTriggered != null ) InternalTriggered( this, EventArgs.Empty );
            }
        }

        public void Stop()
        {
            ListenToKeyDown = false;
            KeyboardDriver.Service.UnregisterCancellableKey( _keyCode );
        }

        public void Teardown()
        {

        }
    }

}
