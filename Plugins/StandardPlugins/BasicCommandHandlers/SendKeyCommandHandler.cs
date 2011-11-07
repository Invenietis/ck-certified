using System;
using CommonServices;
using CK.Plugin;
using System.Windows.Forms;
using CK.Context;
using CK.Core;

namespace BasicCommandHandlers
{
    [Plugin("{3B38A879-4968-4fec-8AE8-81D9EBBB7D69}", Categories = new string[] { "Advanced" },
        PublicName = "Send key command handler", Version = "2.0.0" )]
    public class NewSendKeyCommandHandlerPlugin : BasicCommandHandler, ISendKeyCommandHandlerService
    {
        private const string CMDSendKey = "sendKey";

        public event EventHandler<KeySentEventArgs>  KeySent;

        public event EventHandler<KeySendingEventArgs>  KeySending;

        [RequiredService]
        public IContext Context { get; set; }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            CommandParser p = new CommandParser(e.Command);
            string str;
            if( p.IsIdentifier( out str ) && !e.Canceled && str == CMDSendKey )
            {
                if( str == CMDSendKey ) SendKey( p.StringValue );
            }
        }

        /// <summary>
        /// Send the key represented using the <see cref="SendKeys"/> Class.
        /// KeySending event is fired, then the key is sent and KeySent event is fired.
        /// </summary>
        /// <param name="key">Representation of a key, using <see cref="SendKeys"/> syntax</param>
        public void SendKey( string key )
        {
            DoSendKeys( this, key );
        }

        public void SendKey( object sender, string key )
        {
            DoSendKeys( sender, key );
        }

        private void DoSendKeys( object sender, string key )
        {
            KeySendingEventArgs e = new KeySendingEventArgs( key );

            if( KeySending != null ) KeySending( this, e );
            if( !e.Cancel )
            {
                switch( key )
                {
                    case "@":
                        Keybd.Event( VKeyCode.VK_ALTGR, (byte)VKeyCode.SC_ALTGR_FR, Keybd.KEYEVENTF.EXTENDEDKEY | 0, (UIntPtr)0 );
                        SendKeys.SendWait( key );
                        Keybd.Event( VKeyCode.VK_ALTGR, (byte)VKeyCode.SC_ALTGR_FR, Keybd.KEYEVENTF.EXTENDEDKEY | Keybd.KEYEVENTF.KEYUP, (UIntPtr)0 );
                        break;
                    case "#":
                        Keybd.Event( VKeyCode.VK_ALTGR, (byte)VKeyCode.SC_ALTGR_FR, Keybd.KEYEVENTF.EXTENDEDKEY | 0, (UIntPtr)0 );
                        SendKeys.SendWait( "#" );
                        Keybd.Event( VKeyCode.VK_ALTGR, (byte)VKeyCode.SC_ALTGR_FR, Keybd.KEYEVENTF.EXTENDEDKEY | Keybd.KEYEVENTF.KEYUP, (UIntPtr)0 );
                        break;
                    case "{BACKSLASH}":
                        Keybd.Event( VKeyCode.VK_ALTGR, (byte)VKeyCode.SC_ALTGR_FR, Keybd.KEYEVENTF.EXTENDEDKEY | 0, (UIntPtr)0 );
                        SendKeys.SendWait( "{\\}" );
                        Keybd.Event( VKeyCode.VK_ALTGR, (byte)VKeyCode.SC_ALTGR_FR, Keybd.KEYEVENTF.EXTENDEDKEY | Keybd.KEYEVENTF.KEYUP, (UIntPtr)0 );
                        break;
                    case "{dbquote}":
                        SendKeys.SendWait( "\"" );
                        break;
                    case "(":
                    case ")":
                    case "{":
                    case "}":
                    case "+":
                        SendKeys.SendWait( "{" + key + "}" );
                        break;
                    case "^":
                        Keybd.Event( VKeyCode.VK_CARET, (byte)0, Keybd.KEYEVENTF.KEYDOWN, UIntPtr.Zero );
                        Keybd.Event( VKeyCode.VK_CARET, (byte)0, Keybd.KEYEVENTF.KEYUP, UIntPtr.Zero );
                        SendKeys.SendWait( " " );
                        break;
                    case "%":
                        Keybd.Event( VKeyCode.VK_RSHIFT, (byte)0, Keybd.KEYEVENTF.EXTENDEDKEY, UIntPtr.Zero );
                        Keybd.Event( VKeyCode.VK_PERCENT, (byte)0, Keybd.KEYEVENTF.EXTENDEDKEY, UIntPtr.Zero );
                        Keybd.Event( VKeyCode.VK_PERCENT, (byte)0, Keybd.KEYEVENTF.EXTENDEDKEY | Keybd.KEYEVENTF.KEYUP, UIntPtr.Zero );
                        Keybd.Event( VKeyCode.VK_RSHIFT, (byte)0, Keybd.KEYEVENTF.EXTENDEDKEY | Keybd.KEYEVENTF.KEYUP, UIntPtr.Zero );
                        break;
                    case "~":
                    case "¨":
                    case "`":
                        SendKeys.SendWait( "{" + key + "}" );
                        SendKeys.SendWait( " " );
                        break;
                    default:
                        SendKeys.SendWait( key );
                        break;
                }    

                if( KeySent != null ) KeySent( this, new KeySentEventArgs( key ) );
            }
        }
    }
}
