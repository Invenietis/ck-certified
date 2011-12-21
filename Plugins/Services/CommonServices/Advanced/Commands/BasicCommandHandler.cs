using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CommonServices
{
    /// <summary>
    /// Provides a basic implementation for a CommandHandler. It manages the event registering
    /// in Start & Stop and the link to the Command Manager.
    /// </summary>
    public class BasicCommandHandler : IPlugin
    {
        bool _cLink;

        ICommandManagerService _cm;
        [DynamicService(Requires = RunningRequirement.OptionalTryStart )]
        public ICommandManagerService CommandManager
        {
            get { return _cm; }
            set
            {
                if( _cm != null && value == null )
                    StopListenCommandManager();

                _cm = value;

                if( _cm != null )
                    StartListenCommandManager();
            }
        }

        /// <summary>
        /// Method reacting from Command Sending event. Here, you can parse the command you want to listen
        /// before the command is really launches and do some stuff about it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"><see cref="CommandSendingEventArgs"/> containing data about the command which
        /// is about to be send.</param>
        protected virtual void OnCommandSending( object sender, CommandSendingEventArgs e ) { }

        /// <summary>
        /// Method reacting from the Command Sent event. Here, you can parse the command you want to listen
        /// and do some stuff about it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"><see cref="CommandSentEventArgs"/> containing data about the sent command.</param>
        protected virtual void OnCommandSent( object sender, CommandSentEventArgs e ) { }

        /// <summary>
        /// When the Optional CommandManager is set we listen its CommandSent event.
        /// </summary>
        void StartListenCommandManager()
        {
            _cm.CommandSent += new EventHandler<CommandSentEventArgs>( OnCommandSent );
            _cm.CommandSending += new EventHandler<CommandSendingEventArgs>( OnCommandSending );
            _cLink = true;
        }

        /// <summary>
        /// When the Optional CommandManager is unset we stop to listen its CommandSent event.
        /// </summary>
        void StopListenCommandManager()
        {
            _cm.CommandSent -= new EventHandler<CommandSentEventArgs>( OnCommandSent );
            _cm.CommandSending -= new EventHandler<CommandSendingEventArgs>( OnCommandSending );
            _cLink = false;
        }

        public virtual bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public virtual void Start()
        {
            if( _cm != null && !_cLink ) StartListenCommandManager();
        }

        public virtual void Stop()
        {
            if( _cm != null && _cLink ) StopListenCommandManager();
        }

        public virtual void Teardown()
        {
        }
    }
}
