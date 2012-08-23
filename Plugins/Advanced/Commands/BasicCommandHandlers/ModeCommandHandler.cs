#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\ModeCommandHandler.cs) is part of CiviKey. 
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
using CommonServices;
using CK.Plugin;
using CK.Keyboard.Model;
using CK.Core;

namespace BasicCommandHandlers
{
    [Plugin("{4A3F1565-E127-473c-B169-0022A3EDB58D}", Categories = new string[] { "Advanced" },        
        PublicName = "Mode command handler",
        Version = "1.0.0")]
    public class ModeCommandHandlerPlugin : BasicCommandHandler, IModeCommandHandlerService
    {
        private const string CMDChangeMode = "Mode";

        public event EventHandler<ModeChangedEventArgs>  ModeChangedByCommandHandler;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardContext KeyboardContext { get; set; }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            string cmd;
            string sub;
            string m;

            CommandParser p = new CommandParser( e.Command );

            if( !e.Canceled && p.IsIdentifier( out cmd ) && cmd == CMDChangeMode )
            {
                if( p.MatchIsolatedChar( '.' ) )
                {
                    p.IsIdentifier( out sub );
                    sub = sub.ToLower();

                    if( p.Match( CommandParser.Token.OpenPar ) )
                        if( p.IsString( out m ) )
                            if( p.Match( CommandParser.Token.ClosePar ) )
                            {
                                if( cmd == CMDChangeMode )
                                {
                                    switch( sub )
                                    {
                                        case "add": Add( m ); return;
                                        case "remove": Remove( m ); return;
                                        case "set": ChangeMode( m ); return;
                                        case "toggle": Toggle( m ); return;
                                    }
                                }
                            }
                }
            }
        }

        public void Add( string mode )
        {
            if( KeyboardContext.CurrentKeyboard != null )
            {
                IKeyboardMode kbMode = KeyboardContext.ObtainMode( mode );
                KeyboardContext.CurrentKeyboard.CurrentMode = KeyboardContext.CurrentKeyboard.CurrentMode.Add( kbMode );
                if( ModeChangedByCommandHandler != null ) ModeChangedByCommandHandler( this, new ModeChangedEventArgs( kbMode ) );
            }
        }

        public void Remove( string mode )
        {
            if( KeyboardContext.CurrentKeyboard != null )
            {
                IKeyboardMode kbMode = KeyboardContext.ObtainMode( mode );
                KeyboardContext.CurrentKeyboard.CurrentMode = KeyboardContext.CurrentKeyboard.CurrentMode.Remove( kbMode );
                if( ModeChangedByCommandHandler != null ) ModeChangedByCommandHandler( this, new ModeChangedEventArgs( kbMode ) );
            }
        }

        public void Toggle( string mode )
        {
            if( KeyboardContext.CurrentKeyboard != null )
            {
                IKeyboardMode kbMode = KeyboardContext.ObtainMode( mode );
                KeyboardContext.CurrentKeyboard.CurrentMode = KeyboardContext.CurrentKeyboard.CurrentMode.Toggle( kbMode );
                if( ModeChangedByCommandHandler != null ) ModeChangedByCommandHandler( this, new ModeChangedEventArgs( kbMode ) );
            }
        }

        public void ChangeMode( string mode )
        {
            if( KeyboardContext.CurrentKeyboard != null )
            {
                IKeyboardMode kbMode = KeyboardContext.ObtainMode( mode );
                KeyboardContext.CurrentKeyboard.CurrentMode = kbMode;
                if( ModeChangedByCommandHandler != null ) ModeChangedByCommandHandler( this, new ModeChangedEventArgs( kbMode ) );
            }
        }
    }
}
