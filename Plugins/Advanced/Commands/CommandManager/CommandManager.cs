#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\CommandManager\CommandManager.cs) is part of CiviKey. 
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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Threading;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CommonServices;

namespace CK.StandardPlugins.CommandManager
{
    [Plugin( "{7CCD8104-FB8A-4495-8855-3E7B56EE5100}",
        Categories = new string[] { "Advanced" },
        Version = "1.0.1",
        PublicName = "CommandManager" )]
    public class CommandManager : IPlugin, ICommandManagerService
    {
        const int TIMER_MILLI_SPAN = 20; //in milliseconds

        Queue<DictionaryEntry> _runningCommands;

        DispatcherTimer _timer;
        TimeSpan _timerSpan = new TimeSpan( 0, 0, 0, 0, TIMER_MILLI_SPAN );
        bool _intervalAltered = false;

        public event EventHandler<CommandSendingEventArgs>  CommandSending;
        public event EventHandler<CommandSentEventArgs>  CommandSent;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardContext KeyboardContext { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            KeyboardContext.Keyboards.KeyboardActivated += KeyboardActivated;
            KeyboardContext.Keyboards.KeyboardDeactivated += KeyboardDeactivated;

            foreach( var k in KeyboardContext.Keyboards.Actives ) RegistersOnKeyboard( k );

            EventHandler d = new EventHandler( TimerMethod );
            _timer = new DispatcherTimer( _timerSpan, DispatcherPriority.Normal, d, Dispatcher.CurrentDispatcher );
            _timer.Stop();
        }
        public void Stop()
        {
            foreach( var k in KeyboardContext.Keyboards.Actives ) UnRegistersOnKeyboard( k );

            KeyboardContext.Keyboards.KeyboardActivated -= KeyboardActivated;
            KeyboardContext.Keyboards.KeyboardDeactivated -= KeyboardDeactivated;

            _timer.Stop();
        }

        public void Teardown() { }

        public bool IsRunningCommands
        {
            get { throw new NotImplementedException(); }
        }

        public void SendCommand( object sender, string command )
        {
            SendCommands( sender, new CKReadOnlyListMono<string>( command ) );
        }

        public void SendCommands( object sender, ICKReadOnlyList<string> commands )
        {
            if( _runningCommands == null ) _runningCommands = new Queue<DictionaryEntry>();

            if( !_timer.IsEnabled ) _timer.Start();

            foreach( string cmd in commands )
                _runningCommands.Enqueue( new DictionaryEntry( sender, cmd ) );
        }

        private void ProcessCommands()
        {
            while( _runningCommands.Count > 0 )
            {
                DictionaryEntry e = _runningCommands.Dequeue();
                string value = e.Value.ToString().TrimStart();

                if( value.StartsWith( "pause", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    int milliseconds = 0;
                    if( Int32.TryParse( value.Split( ':' )[1], out milliseconds ) )
                    {
                        _timer.Interval = new TimeSpan( 0, 0, 0, 0, milliseconds );
                        _intervalAltered = true;
                        return;
                    }
                    else
                    {
                        throw new ArgumentException( "A \"pause\" command was called. Its value was not a valid integer : " + e.Value );
                    }
                }

                DoSendCommand( e.Key, (string)e.Value );
            }

            _timer.Stop();
        }


        private void TimerMethod( object sender, EventArgs e )
        {
            if( _intervalAltered )
            {
                _timer.Interval = _timerSpan;
                _intervalAltered = false;
            }

            ProcessCommands();
        }

        /// <summary>
        /// Called when the current keyboard changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void KeyboardActivated( object sender, KeyboardEventArgs e )
        {
            RegistersOnKeyboard( e.Keyboard );
        }

        private void KeyboardDeactivated( object sender, KeyboardEventArgs e )
        {
            UnRegistersOnKeyboard( e.Keyboard );
        }

        /// <summary>
        /// Registers on the Keyboard given in parameters.
        /// </summary>
        /// <param name="keyboard"><see cref="VKeyboard"/> to register on.</param>
        void RegistersOnKeyboard( IKeyboard keyboard )
        {
            keyboard.KeyDown += new EventHandler<KeyInteractionEventArgs>( SendKeyCommand );
            keyboard.KeyPressed += new EventHandler<KeyPressedEventArgs>( SendKeyCommand );
            keyboard.KeyUp += new EventHandler<KeyInteractionEventArgs>( SendKeyCommand );
        }

        /// <summary>
        /// Unregisters on the Keyboard given in parameters.
        /// </summary>
        /// <param name="keyboard"><see cref="VKeyboard"/> to unregister on.</param>
        void UnRegistersOnKeyboard( IKeyboard keyboard )
        {
            keyboard.KeyDown -= new EventHandler<KeyInteractionEventArgs>( SendKeyCommand );
            keyboard.KeyPressed -= new EventHandler<KeyPressedEventArgs>( SendKeyCommand );
            keyboard.KeyUp -= new EventHandler<KeyInteractionEventArgs>( SendKeyCommand );
        }

        /// <summary>
        /// Redirects the command to the method which will process it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SendKeyCommand( object sender, KeyInteractionEventArgs e )
        {
            SendCommands( e.Key, e.Commands );
        }

        // Private method which realy Send commands, we don't allow client to use it directly.
        void DoSendCommand( object sender, string cmd )
        {
            CommandSendingEventArgs e = new CommandSendingEventArgs( cmd );
            if( CommandSending != null ) CommandSending( sender, e );
            if( CommandSent != null ) CommandSent( sender, new CommandSentEventArgs( e ) );
        }
    }
}
