#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\Mode\ModeCommandHandler.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Diagnostics;
using BasicCommandHandlers.Resources;
using CK.Keyboard.Model;
using CK.Plugin;
using CommonServices;
using ProtocolManagerModel;

namespace BasicCommandHandlers
{
    [Plugin( "{4A3F1565-E127-473c-B169-0022A3EDB58D}", Categories = new string[] { "Advanced" },
        PublicName = "Mode command handler",
        Version = "1.6.0" )]
    public class ModeCommandHandlerPlugin : BasicCommandHandler, IModeCommandHandlerService
    {
        private const string PROTOCOL = "mode";

        public event EventHandler<ModeChangedEventArgs> ModeChangedByCommandHandler;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardContext KeyboardContext { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IProtocolEditorsManager> ProtocolManagerService { get; set; }

        public override void Start()
        {
            base.Start();
            ProtocolManagerService.Service.Register(
                                        new VMProtocolEditorMetaData(
                                        "mode",
                                        R.ModeProtocolTitle,
                                        R.ModeProtocolDescription,
                                        typeof( ModeCommandParameterManager ) ),
                                        typeof( IModeCommandHandlerService ) );
        }


        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( !e.Canceled && e.Command.StartsWith( PROTOCOL ) )
            {
                string parameter = e.Command.Substring( e.Command.IndexOf( ':' ) + 1 );
                string[] splittedParameter = parameter.Split( ',' );
                Debug.Assert( splittedParameter.Length == 2 );

                string action = splittedParameter[0];
                string targetMode = splittedParameter[1];

                switch( action )
                {
                    case "add": Add( targetMode ); return;
                    case "remove": Remove( targetMode ); return;
                    case "set": ChangeMode( targetMode ); return;
                    case "toggle": Toggle( targetMode ); return;
                }
            }

            //string cmd;
            //string sub;
            //string m;

            //CommandParser p = new CommandParser( e.Command );

            //if( !e.Canceled && p.IsIdentifier( out cmd ) && cmd == CMDChangeMode )
            //{
            //    if( p.MatchIsolatedChar( '.' ) )
            //    {
            //        p.IsIdentifier( out sub );
            //        sub = sub.ToLower();

            //        if( p.Match( CommandParser.Token.OpenPar ) )
            //            if( p.IsString( out m ) )
            //                if( p.Match( CommandParser.Token.ClosePar ) )
            //                {
            //                    if( cmd == CMDChangeMode )
            //                    {
            //                        switch( sub )
            //                        {
            //                            case "add": Add( m ); return;
            //                            case "remove": Remove( m ); return;
            //                            case "set": ChangeMode( m ); return;
            //                            case "toggle": Toggle( m ); return;
            //                        }
            //                    }
            //                }
            //    }
            //}
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
