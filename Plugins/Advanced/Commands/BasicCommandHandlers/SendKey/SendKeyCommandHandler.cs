#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\SendKey\SendKeyCommandHandler.cs) is part of CiviKey. 
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
using System.Windows.Forms;
using BasicCommandHandlers.Resources;
using CK.Context;
using CK.Plugin;
using CommonServices;
using ProtocolManagerModel;

namespace BasicCommandHandlers
{
    [Plugin( "{3B38A879-4968-4fec-8AE8-81D9EBBB7D69}", Categories = new string[] { "Advanced" },
        PublicName = "Send key command handler", Version = "2.0.0" )]
    public class NewSendKeyCommandHandlerPlugin : BasicCommandHandler, ISendKeyCommandHandlerService
    {
        private const string CMDSendKey = "sendKeyOld";
        private const string PROTOCOL = "sendkeyold";

        public event EventHandler<KeySentEventArgs> KeySent;

        public event EventHandler<KeySendingEventArgs> KeySending;

        [RequiredService]
        public IContext Context { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IProtocolEditorsManager> ProtocolManagerService { get; set; }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( !e.Canceled && e.Command.StartsWith( PROTOCOL ) )
            {
                string parameter = e.Command.Substring( e.Command.IndexOf( ':' ) + 1 );
                SendKey( parameter );
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

        public override void Start()
        {
            base.Start();
            ProtocolManagerService.Service.Register(
                                        new VMProtocolEditorWrapper(
                                        "sendKey",
                                        R.SendKeyProtocolTitle,
                                        R.SendKeyProtocolDescription,
                                        typeof( SendKeyCommandParameterManager ) ),
                                        typeof( ISendKeyCommandHandlerService ) );//is it the right service ?
        }

        public override void Stop()
        {
            ProtocolManagerService.Service.Unregister( "sendKey" );
            base.Stop();
        }
    }
}
