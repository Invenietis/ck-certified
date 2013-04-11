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
    [Plugin( InputTrigger.PluginIdString,
           PublicName = PluginPublicName,
           Version = InputTrigger.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class InputTrigger : IPlugin, ITriggerService
    {
        const string PluginIdString = "{4E3A3B25-7FD0-406F-A958-ECB50AC6A597}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Keyboard Trigger";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardDriver> KeyboardDriver { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> PointerDriver { get; set; }

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
                    {
                        KeyboardDriver.Service.KeyDown += OnKeyDown;
                        PointerDriver.Service.PointerButtonDown += OnPointerButtonDown;
                    }
                    else
                    {
                        KeyboardDriver.Service.KeyDown -= OnKeyDown;
                        PointerDriver.Service.PointerButtonDown -= OnPointerButtonDown;
                    }
                }
            }
        }

        public event EventHandler InternalTriggered;

        int _keyCode;
        TriggerDevice _currentDevice = TriggerDevice.None;

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            ListenToKeyDown = true;

            _keyCode = Configuration.User.GetOrSet( "TriggerCode", 122 );
            _currentDevice = Configuration.User.GetOrSet( "TriggerDevice", TriggerDevice.Keyboard );
            if( _currentDevice == TriggerDevice.Keyboard )
                KeyboardDriver.Service.RegisterCancellableKey( _keyCode );

            Configuration.ConfigChanged += OnConfigChanged;
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( u => u.UniqueId == InputTrigger.PluginId.UniqueId ) && ( e.Key == "TriggerCode" || e.Key == "TriggerDevice" ) )
            {
                if( _currentDevice == TriggerDevice.Keyboard ) //If we were listening to a keyboard key, unregister it.
                    KeyboardDriver.Service.UnregisterCancellableKey( _keyCode );

                if( e.Key == "TriggerCode" ) //Retrieving the new code.
                    _keyCode = (int)e.Value;
                else if( e.Key == "TriggerDevice" ) //Retrieving the new TriggerDevice
                {
                    TriggerDevice temp = TriggerDevice.None;
                    if( Enum.TryParse<TriggerDevice>( e.Value.ToString(), out temp ) )
                        _currentDevice = temp;
                }

                if( _currentDevice == TriggerDevice.Keyboard ) //If we are now listening to a keyboard key, register it
                    KeyboardDriver.Service.RegisterCancellableKey( _keyCode );
            }
        }

        void OnKeyDown( object sender, KeyboardDriverEventArg e )
        {
            if( _currentDevice == TriggerDevice.Keyboard && e.KeyCode == _keyCode ) // when the right keycode is pressed
            {
                if( InternalTriggered != null ) InternalTriggered( this, EventArgs.Empty );
            }
        }

        void OnPointerButtonDown( object sender, PointerDeviceEventArgs e )
        {
            if( _currentDevice == TriggerDevice.Pointer )
            {
                if( MouseCodeFromButtonInfo( e.ButtonInfo, e.ExtraInfo ) == _keyCode ) // when the right mouse button is pressed
                {
                    if( InternalTriggered != null )
                    {
                        e.Cancel = true;
                        InternalTriggered( this, EventArgs.Empty );
                    }
                }
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

        //JL : duplicate with MouseCodeFromButtonInfo in BasicScroll/Editor/EditorViewModel.cs. Where can it be put to avoid duplication ?
        public int MouseCodeFromButtonInfo( ButtonInfo buttonInfo, string extraInfo )
        {
            if( buttonInfo == ButtonInfo.DefaultButton )
            {
                return 1;
            }

            if( buttonInfo == ButtonInfo.XButton )
            {
                if( extraInfo == "Right" )
                    return 2;

                if( extraInfo == "Middle" )
                    return 3;
            }

            throw new Exception( String.Format( "The specified buttonInfo is incorrect. (ButtonInfo : {0}, ExtraInfo : {1}) ", buttonInfo.ToString(), extraInfo ) );
        }
    }

    public enum TriggerDevice
    {
        None = 0,
        Keyboard = 1,
        Pointer = 2
    }

}
