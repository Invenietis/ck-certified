using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

        bool _wasASpace = false;
        bool _stringSending = false;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardDriver> KeyboardDriver { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ISendStringService> SendStringService { get; set; }

        public IPluginConfigAccessor Configuration { get; set; }

        public event EventHandler Triggered;

        int _keyCode;

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            if( SendStringService.Service != null )
            {
                SendStringService.Service.StringSending += OnStringSending;
                SendStringService.Service.StringSent += OnStringSent;
            }
            SendStringService.ServiceStatusChanged += SendStringService_ServiceStatusChanged;

            KeyboardDriver.Service.KeyDown += OnKeyDown;

            _keyCode = Configuration.User.GetOrSet( "TriggerKeyCode", 0x20 );
            KeyboardDriver.Service.RegisterCancellableKey( _keyCode );

            Configuration.ConfigChanged += OnConfigChanged;
        }

        void OnStringSending( object sender, StringSendingEventArgs e )
        {
            _stringSending = true;
        }

        void OnStringSent( object sender, StringSentEventArgs e )
        {
            _stringSending = false;
        }


        void SendStringService_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
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
            if( _stringSending == false && !_wasASpace && e.KeyCode == _keyCode ) // on spacebar pressed
            {
                _wasASpace = true;
                if( Triggered != null ) Triggered( this, EventArgs.Empty );
                _wasASpace = false;
            }
        }

        public void Stop()
        {
            if( SendStringService.Service != null )
            {
                SendStringService.Service.StringSending -= OnStringSending;
                SendStringService.Service.StringSent -= OnStringSent;
            }
            SendStringService.ServiceStatusChanged -= SendStringService_ServiceStatusChanged;
            KeyboardDriver.Service.KeyDown -= OnKeyDown;
            KeyboardDriver.Service.UnregisterCancellableKey( _keyCode );
        }

        public void Teardown()
        {
            
        }
    }
}
