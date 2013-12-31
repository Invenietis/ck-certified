using System;
using CK.Plugin;
using CK.InputDriver;

namespace CK.Plugins.SendInputDriver
{
    public interface ISendStringService : IDynamicService
    {
        /// <summary>
        /// Send a String
        /// </summary>
        /// <param name="key">A classic string</param>
        void SendString( string key );

        /// <summary>
        /// Send a String
        /// </summary>
        /// <param name="key">A classic string</param>
        void SendString( object sender, string key );

        /// <summary>
        /// Send a specific keyboard key
        /// </summary>
        /// <param name="key"></param>
        void SendKeyboardKey( Native.KeyboardKeys key );

        /// <summary>
        /// Raised when a String is sent.
        /// </summary>
        event EventHandler<StringSentEventArgs> StringSent;

        /// <summary>
        /// Raised when a String is sending.
        /// </summary>
        event EventHandler<StringSendingEventArgs> StringSending;


        /// <summary>
        /// Raised when a Key is sent.
        /// </summary>
        event EventHandler<NativeKeySentEventArgs>  KeySent;

        /// <summary>
        /// Raised when a Key is sending.
        /// </summary>
        event EventHandler<NativeKeySendingEventArgs>  KeySending;
    }

    public class StringSentEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        public readonly string StringVal;

        public StringSentEventArgs( string s )
        {
            StringVal = s;
        }
    }

    /// <summary>
    /// </summary>
    public class StringSendingEventArgs : StringSentEventArgs
    {
        private bool _canceled;

        public bool Cancel
        {
            get { return _canceled; }
            set { _canceled = value; }
        }

        public StringSendingEventArgs( string s )
            : base( s )
        {
        }

    }


    public class NativeKeySentEventArgs : EventArgs
    {
        /// <summary>
        /// Store the keyword which represent the key which has been sent
        /// </summary>
        public readonly Native.KeyboardKeys Key;

        public NativeKeySentEventArgs(Native.KeyboardKeys key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// Contain the key which will be send to system, it's can be cancel
    /// Sample: the F1 key launch the help of focused application. we can block that
    /// </summary>
    public class NativeKeySendingEventArgs : NativeKeySentEventArgs
    {
        private bool _canceled;

        public bool Cancel
        {
            get { return _canceled; }
            set { _canceled = value; }
        }

        public NativeKeySendingEventArgs(Native.KeyboardKeys key)
            : base( key )
        {
        }

    }
}

