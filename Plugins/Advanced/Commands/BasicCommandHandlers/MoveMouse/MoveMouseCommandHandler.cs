﻿#region LGPL License
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
using CommonServices;
using CommonServices.Accessibility;
using CK.Plugin;
using System.Windows.Threading;
using ProtocolManagerModel;
using BasicCommandHandlers.Resources;

namespace BasicCommandHandlers
{
    [Plugin( "{B2EC4D13-7A4F-4F9E-A713-D5F8DDD161EF}", Categories = new string[] { "Advanced" },
        PublicName = "Move mouse command handler",
        Version = "1.0.0" )]
    public class MoveMouseCommandHandler : BasicCommandHandler, IMoveMouseCommandHandlerService
    {
        const string PROTOCOL_BASE = "movemouse";
        const string PROTOCOL = PROTOCOL_BASE + ":";
        const int STEP = 5;

        DispatcherTimer _timer;
        Action _currentMotionFunction;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> PointerDriver { get; set; }

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> HighlighterService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IProtocolEditorsManager> ProtocolManagerService { get; set; }

        public override bool Setup( IPluginSetupInfo info )
        {
            _timer = new DispatcherTimer();
            _timer.Tick += OnInternalBeat;

            return base.Setup( info );
        }

        void OnInternalBeat( object sender, EventArgs e )
        {
            if ( _currentMotionFunction != null )
                _currentMotionFunction();
        }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if ( e.Command.StartsWith( PROTOCOL ) )
            {
                if ( !_timer.IsEnabled )
                {
                    if ( HighlighterService.Status.IsStartingOrStarted )
                        HighlighterService.Service.Pause();

                    string[] parameters = e.Command.Substring( PROTOCOL.Length ).Trim().Split( ',' );
                    string direction = parameters[0];
                    int speed = int.Parse( parameters[1] );

                    BeginMouseMotion( direction, speed );
                }
                else
                {
                    if ( HighlighterService.Status.IsStartingOrStarted )
                        HighlighterService.Service.Resume();
                    EndMouseMotion();
                }
            }
        }

        public void BeginMouseMotion( string direction, int speed )
        {
            // timer should not be enabled. If it is enabled it could be due to 
            if ( _timer.IsEnabled ) _timer.Stop();

            // setup speed
            _timer.Interval = new TimeSpan( 0, 0, 0, 0, speed );

            Action motion = null;
            #region create motion function based on the direction
            switch ( direction )
            {
                case "U":
                    motion = () =>
                    {
                        CK.InputDriver.MouseProcessor.MoveMouseToAbsolutePosition( 
                            PointerDriver.Service.CurrentPointerXLocation,
                            PointerDriver.Service.CurrentPointerYLocation - STEP );

                    };
                    break;
                case "R":
                    motion = () =>
                    {
                        CK.InputDriver.MouseProcessor.MoveMouseToAbsolutePosition(
                            PointerDriver.Service.CurrentPointerXLocation + STEP,
                            PointerDriver.Service.CurrentPointerYLocation );
                    };
                    break;
                case "B":
                    motion = () =>
                    {
                        CK.InputDriver.MouseProcessor.MoveMouseToAbsolutePosition(
                            PointerDriver.Service.CurrentPointerXLocation,
                            PointerDriver.Service.CurrentPointerYLocation + STEP );
                    };
                    break;
                case "L":
                    motion = () =>
                    {
                        CK.InputDriver.MouseProcessor.MoveMouseToAbsolutePosition(
                            PointerDriver.Service.CurrentPointerXLocation - STEP,
                            PointerDriver.Service.CurrentPointerYLocation );
                    };
                    break;
            }

            if ( motion == null )
            {

                double angle = 0.0;
                switch ( direction )
                {
                    case "UL":
                        angle = -( ( 3 * Math.PI ) / 4 );
                        break;
                    case "BL":
                        angle = ( 3 * Math.PI ) / 4;
                        break;
                    case "BR":
                        angle = Math.PI / 4;
                        break;
                    case "UR":
                        angle = -( Math.PI / 4 );
                        break;
                }

                motion = () =>
                {
                    int x = (int)( PointerDriver.Service.CurrentPointerXLocation + ( STEP * Math.Cos( angle ) ) );
                    int y = (int)( PointerDriver.Service.CurrentPointerYLocation + ( STEP * Math.Sin( angle ) ) );
                    CK.InputDriver.MouseProcessor.MoveMouseToAbsolutePosition( x, y );
                };
            }
            #endregion

            _currentMotionFunction = motion;
            _timer.Start();
        }

        public void EndMouseMotion()
        {
            _timer.Stop();
        }

        public override void Start()
        {
            base.Start();
            ProtocolManagerService.Service.Register(
                    new VMProtocolEditorWrapper( PROTOCOL_BASE,
                                                 R.MoveMouseProtocolTitle,
                                                 R.MoveMouseProtocolDescription,
                                                 typeof( MoveMouseCommandParameterManager ) ),
                                                 typeof( IMoveMouseCommandHandlerService ) );
        }

        public override void Stop()
        {
            if ( ProtocolManagerService.Status.IsStartingOrStarted )
                ProtocolManagerService.Service.Unregister( PROTOCOL_BASE );
            base.Stop();
        }
    }
}
