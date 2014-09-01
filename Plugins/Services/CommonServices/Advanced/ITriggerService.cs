#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\CommonServices\Advanced\ITriggerService.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using CK.Plugin;

namespace CommonServices
{
    /// <summary>
    /// This service abstract external inputs, from a keyboard input to an event sent by the Mars rover by email.
    /// </summary>
    public interface ITriggerService : IDynamicService
    {
        /// <summary>
        /// Get the default ITrigger
        /// </summary>
        ITrigger DefaultTrigger { get; }

        /// <summary>
        /// The listener that listen all input kind
        /// </summary>
        IInputListener InputListener { get; }

        /// <summary>
        /// Register an Action to the given trigger
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="action"></param>
        /// <param name="preventDefault">if true, the event fired by the ITrigger is not propagated to the system</param>
        void RegisterFor( ITrigger trigger, Action<ITrigger> action, bool preventDefault = true );

        /// <summary>
        /// Unregister the given action to the given trigger
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="action"></param>
        void Unregister(ITrigger trigger, Action<ITrigger> action );
    }

    /// <summary>
    /// Describe a class wich is able to catch inputs from the supported devices
    /// </summary>
    public interface IInputListener : IDisposable
    {
        /// <summary>
        /// Listen to inputs and invoke the callback method when a trigger is rised
        /// </summary>
        /// <returns></returns>
        void Record( Action<ITrigger> callback );
        
        /// <summary>
        /// True when a record is ongoing
        /// </summary>
        bool IsRecording { get; }

        /// <summary>
        /// Fired when any input is pressed
        /// </summary>
        event EventHandler<KeyDownEventArgs> KeyDown;
    }

    /// <summary>
    /// A captured input
    /// </summary>
    public interface ITrigger
    {
        int KeyCode { get; }

        /// <summary>
        /// Source device
        /// </summary>
        TriggerDevice Source { get; }


        string DisplayName { get; }
    }

    public class KeyDownEventArgs : EventArgs
    {
        public int KeyCode { get; private set; }
        public TriggerDevice Source { get; private set; }

        public KeyDownEventArgs( int keyCode, TriggerDevice device )
            : base()
        {
            KeyCode = keyCode;
            Source = device;
        }
    }

    public enum TriggerDevice
    {
        None = 0,
        Keyboard = 1 ,
        Civikey = 2,
        Pointer = 3
    }

    public class InputTriggerEventArgs : EventArgs
    {
        public InputSource Source { get; set; }
        public InputTriggerEventArgs( InputSource source )
        {
            Source = source;
        }

        public InputTriggerEventArgs()
        {
            Source = InputSource.Unknown;
        }
    }
}
