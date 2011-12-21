using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CommonServices
{
    public interface ISendKeyCommandHandlerService : IDynamicService
    {
        /// <summary>
        /// Send the key
        /// </summary>
        /// <param name="key">Keyword which represent the key</param>
        void SendKey( string key );

        /// <summary>
        /// Send the key and throw the sender to event
        /// </summary>
        /// <param name="key">Keyword which represent the key</param>
        void SendKey( object sender, string key );

        /// <summary>
        /// Raised when a Key is sent.
        /// </summary>
        event EventHandler<KeySentEventArgs> KeySent;

        /// <summary>
        /// Raised when a Key is sending
        /// </summary>
        event EventHandler<KeySendingEventArgs> KeySending;
    }

    public class KeySentEventArgs : EventArgs
    {
        /// <summary>
        /// Store the keyword which represent the key which has been sent
        /// </summary>
        public readonly string Key;

        public KeySentEventArgs( string key )
        {
            Key = key;
        }
    }

    /// <summary>
    /// Contain the key which will be send to system, it's can be cancel
    /// Sample: the F1 key launch the help of focused application. we can block that
    /// </summary>
    public class KeySendingEventArgs : KeySentEventArgs
    {
        private bool _canceled;

        public bool Cancel
        {
            get { return _canceled; }
            set { _canceled = value; }
        }

        public KeySendingEventArgs( string key )
            : base( key )
        {
        }
        
    }
}
