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
using CommonServices;

namespace BasicScroll
{
    [Plugin( SpacebarTrigger.PluginIdString,
           PublicName = PluginPublicName,
           Version = SpacebarTrigger.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class SpacebarTrigger : IPlugin, ITriggerService
    {
        const string PluginIdString = "{4E3A3B25-7FD0-406F-A958-ECB50AC6A597}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "SpacebarTrigger";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        bool _wasASpace = false;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardDriver> KeyboardDriver { get; set; }

        public event EventHandler Triggered;

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            KeyboardDriver.Service.KeyDown += OnKeyDown;
        }

        void OnKeyDown( object sender, KeyboardDriverEventArg e )
        {
            if( !_wasASpace && e.KeyCode == 0x20 ) // on spacebar pressed
            {
                _wasASpace = true;
                if( Triggered != null ) Triggered( this, EventArgs.Empty );
                e.Cancel = true;
                _wasASpace = false;
            }
        }

        public void Stop()
        {
            KeyboardDriver.Service.KeyDown -= OnKeyDown;
        }

        public void Teardown()
        {
            
        }
    }
}
