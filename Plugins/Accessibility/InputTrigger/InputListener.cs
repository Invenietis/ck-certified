using System;
using CK.Plugin;
using CommonServices;

namespace InputTrigger
{
    public class InputListener : IInputListener
    {
        IService<IKeyboardDriver> _keboardDriver;
        IService<IPointerDeviceDriver> _mouseDriver;

        private  Action<ITrigger> _recordCallback;

        public bool IsRecording { get; private set; }

        public InputListener(IService<IKeyboardDriver> kb, IService<IPointerDeviceDriver> pd)
        {
            _keboardDriver = kb;
            _mouseDriver = pd;

            _keboardDriver.Service.KeyDown += OnKeyDown;
            _mouseDriver.Service.PointerButtonDown += OnMouseDown;
        }

        public event EventHandler<KeyDownEventArgs> KeyDown;

        public void Record( Action<ITrigger> callback )
        {
            IsRecording = true;
            _recordCallback = callback;
        }

        void OnKeyDown( object sender, KeyboardDriverEventArg e )
        {
            if( IsRecording )
            {
                _recordCallback( new Trigger( e.KeyCode, e.InputSource == InputSource.CiviKey ? TriggerDevice.Civikey : TriggerDevice.Keyboard ) );
                IsRecording = false;
            }
            else
            {
                FireKeyDown( e.KeyCode, e.InputSource == InputSource.CiviKey ? TriggerDevice.Civikey : TriggerDevice.Keyboard );
            }
        }

        void OnMouseDown( object sender, PointerDeviceEventArgs e )
        {
            if( IsRecording )
            {
                _recordCallback(new Trigger( MouseCodeFromButtonInfo( e.ButtonInfo, e.ExtraInfo ), TriggerDevice.Pointer ));
                IsRecording = false;
            }
            else
            {
                FireKeyDown( MouseCodeFromButtonInfo( e.ButtonInfo, e.ExtraInfo ), TriggerDevice.Pointer );
            }
        }

        void FireKeyDown(int keyCode, TriggerDevice device)
        {
            if( KeyDown != null ) KeyDown( this, new KeyDownEventArgs( keyCode, device ) );
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

            return 0;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _keboardDriver.Service.KeyDown -= OnKeyDown;
            _mouseDriver.Service.PointerButtonDown -= OnMouseDown;
        }

        #endregion
    }
}
