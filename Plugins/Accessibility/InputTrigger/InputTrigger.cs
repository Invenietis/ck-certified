using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Plugin;
using CommonServices;

namespace InputTrigger
{
    public class InputTrigger : IPlugin
    {
        const string PluginIdString = "{14FE0383-2BE4-43A1-9627-A66C2CA775A6}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Input Trigger";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardDriver> KeyboardDriver { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> PointerDriver { get; set; }

        private Dictionary<ITrigger, List<Action<ITrigger>>> _listeners;

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

        TriggerDevice _currentDevice = TriggerDevice.None;

        public bool Setup( IPluginSetupInfo info )
        {
            _listeners = new Dictionary<ITrigger, List<Action<ITrigger>>>();
            return true;
        }

        public void Start()
        {
            KeyboardDriver.Service.KeyDown += OnKeyDown;
            PointerDriver.Service.PointerButtonDown += OnPointerButtonDown;
        }

        public void Stop()
        {
            ListenToKeyDown = false;
            //KeyboardDriver.Service.UnregisterCancellableKey( _keyCode );
        }

        public void Teardown()
        {

        }

        public void RegisterFor(ITrigger trigger, Action<ITrigger> action)
        {
            //TODO call action when ITrigger is rised
        }

        public void Unregister( Action<ITrigger> action )
        {
            //TODO unregister action
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

        void OnKeyDown( object sender, KeyboardDriverEventArg e )
        {

            //if( _currentDevice == TriggerDevice.Keyboard && e.KeyCode == _keyCode ) // when the right keycode is pressed
            //{

            //}
        }

        void OnPointerButtonDown( object sender, PointerDeviceEventArgs e )
        {
            if( _currentDevice == TriggerDevice.Pointer )
            {
                //if( MouseCodeFromButtonInfo( e.ButtonInfo, e.ExtraInfo ) == _keyCode ) // when the right mouse button is pressed
                //{
                //}
            }
        }
    }
}
