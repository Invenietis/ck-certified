using System;
using System.Collections.Generic;
using System.Linq;
using CK.Plugin;
using CommonServices;

namespace InputTrigger
{
    [Plugin( InputTrigger.PluginIdString,
           PublicName = PluginPublicName,
           Version = InputTrigger.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class InputTrigger : IPlugin , ITriggerService
    {
        const string PluginIdString = "{14FE0383-2BE4-43A1-9627-A66C2CA775A6}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Input Trigger";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardDriver> KeyboardDriver { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> PointerDriver { get; set; }

        public IInputListener InputListener { get; private set; }

        public ITrigger DefaultTrigger { get { return Trigger.Default; } }
                                            
        private Dictionary<ITrigger, List<Action<ITrigger>>> _listeners;

        public bool Setup( IPluginSetupInfo info )
        {
            _listeners = new Dictionary<ITrigger, List<Action<ITrigger>>>();

            return true;
        }

        public void Start()
        {
            InputListener = new InputListener( KeyboardDriver, PointerDriver );
            InputListener.KeyDown += OnKeyDown;
        }

        public void Stop()
        {
            InputListener.KeyDown -= OnKeyDown;
            InputListener.Dispose();
        }

        public void Teardown()
        {

        }

        public void RegisterFor(ITrigger trigger, Action<ITrigger> action, bool preventDefault = true)
        {
            //call action when the given trigger is risen
            ITrigger key = GetKey( trigger.KeyCode, trigger.Source );
            if( preventDefault && trigger.Source == TriggerDevice.Keyboard ) KeyboardDriver.Service.RegisterCancellableKey( trigger.KeyCode );

            if( key != null )
            {
                _listeners[key].Add( action );
            }
            else
            {
                List<Action<ITrigger>> l = new List<Action<ITrigger>>();
                l.Add(action);
                _listeners.Add( trigger, l );
            }
        }

        public void Unregister(ITrigger trigger, Action<ITrigger> action )
        {
            if( trigger.Source == TriggerDevice.Keyboard ) KeyboardDriver.Service.UnregisterCancellableKey( trigger.KeyCode );
            trigger = GetKey( trigger.KeyCode, trigger.Source );
            
            if( trigger != null )
            {
                _listeners[trigger].Remove( action );
            }
        }

        void OnKeyDown( object sender, KeyDownEventArgs e )
        {
            ITrigger key = GetKey( e.KeyCode, e.Source );

            if( key != null ) InvokeActions( key );
        }

        ITrigger GetKey( int keyCode, TriggerDevice source )
        {
            return _listeners.Keys.Where( k => k.Source == source && k.KeyCode == keyCode )
                .FirstOrDefault();
        }

        void InvokeActions(ITrigger key)
        {
            if( !_listeners.ContainsKey( key ) ) throw new InvalidOperationException( "The given trigger must exist!" );

            foreach( var action in _listeners[key] ) action( key );
        }

        #region ITriggerService Members

        public event EventHandler<InputTriggerEventArgs> Triggered;

        #endregion
    }
}
