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
        const string PluginPublicName = "SpacebarTrigger";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        bool _wasASpace = false;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardDriver> KeyboardDriver { get; set; }

        public IPluginConfigAccessor Configuration { get; set; }

        public event EventHandler Triggered;

        int _keyCode;

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            KeyboardDriver.Service.KeyDown += OnKeyDown;

            _keyCode = Configuration.User.GetOrSet( "TriggerKeyCode", 0x20 );
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
            if( !_wasASpace && e.KeyCode == _keyCode ) // on spacebar pressed
            {
                _wasASpace = true;
                if( Triggered != null ) Triggered( this, EventArgs.Empty );
                _wasASpace = false;
            }
        }

        public void Stop()
        {
            KeyboardDriver.Service.KeyDown -= OnKeyDown;
            KeyboardDriver.Service.UnregisterCancellableKey( _keyCode );
        }

        public void Teardown()
        {
            
        }
    }
}
