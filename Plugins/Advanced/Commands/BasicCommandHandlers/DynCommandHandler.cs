#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\DynCommandHandler.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using CommonServices;
using CK.Plugin;
using CK.Context;

namespace BasicCommandHandlers
{
    [Plugin( "{0628E093-A9C6-4EC6-83C8-F684352B5B37}", Categories = new string[] { "Advanced" },
        PublicName = "Dynamic command handler",
        Description = "Allows the system to execute simple actions for specific commands",
        Version = "1.0.0" )]
    public class DynCommandHandlerPlugin : BasicCommandHandler, IDynCommandHandlerService
    {
        Dictionary<string, Action> _actions;

        private const string CMD = "DynCommand";

        [RequiredService]
        public IContext Context { get; set; }

        //[DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        //public IService<ISkinService> SkinService { get; set; }

        void HideSkin()
        {
            //if( SkinService.Status.IsStartingOrStarted ) SkinService.Service.HideSkin();
        }
        void ToggleHostMinimized()
        {
            //if( SkinService.Status.IsStartingOrStarted ) SkinService.Service.ToggleHostMinimized();
        }

        public override bool Setup( IPluginSetupInfo info )
        {
            _actions = new Dictionary<string, Action>();
            _actions.Add( "HideSkin", () => HideSkin() );
            _actions.Add( "ShutDown", () => Context.RaiseExitApplication( true ) );
            _actions.Add( "ToggleHostMinimized", () => ToggleHostMinimized() );
            _actions.Add(
                "WindowsKey",
                () =>
                {
                    Keybd.Event( VKeyCode.VK_WIN, (byte)0, Keybd.KEYEVENTF.KEYDOWN, UIntPtr.Zero );
                    Keybd.Event( VKeyCode.VK_WIN, (byte)0, Keybd.KEYEVENTF.KEYUP, UIntPtr.Zero );
                }
            );
            _actions.Add( "ContextMenu",
                () =>
                {
                    Keybd.Event( VKeyCode.VK_APPS, (byte)0, Keybd.KEYEVENTF.KEYDOWN, UIntPtr.Zero );
                    Keybd.Event( VKeyCode.VK_APPS, (byte)0, Keybd.KEYEVENTF.KEYUP, UIntPtr.Zero );
                }
            );
            _actions.Add( "PressAltGr",
                () => Keybd.Event( VKeyCode.VK_ALTGR, (byte)VKeyCode.SC_ALTGR_FR, Keybd.KEYEVENTF.EXTENDEDKEY | 0, (UIntPtr)0 )
            );
            _actions.Add( "ReleaseAltGr",
                () => Keybd.Event( VKeyCode.VK_ALTGR, (byte)VKeyCode.SC_ALTGR_FR, Keybd.KEYEVENTF.EXTENDEDKEY | Keybd.KEYEVENTF.KEYUP, (UIntPtr)0 )
            );

            return base.Setup( info );
        }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            string cmd;
            string m;

            CommandParser p = new CommandParser( e.Command );

            if( !e.Canceled && p.IsIdentifier( out cmd ) && cmd == CMD )
            {
                if( p.Match( CommandParser.Token.OpenPar ) )
                    if( p.IsString( out m ) )
                        if( p.Match( CommandParser.Token.ClosePar ) )
                            if( cmd == CMD )
                                Exec( m );
            }
        }

        public void Exec( string actionKey )
        {
            if( _actions.ContainsKey( actionKey ) )
                _actions[actionKey]();
        }
    }
}
