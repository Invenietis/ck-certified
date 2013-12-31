using System;
using System.Runtime.CompilerServices;
using System.Threading;
using CK.Plugin;
using CK.InputDriver;

namespace CK.Plugins.SendInputDriver
{
    [Plugin( "{4F82CC10-1115-4FF0-B483-E95EEEA21107}", Categories = new string[] { "Advanced" },
        PublicName = "Send string command handler", Version = "2.0.0" )]
    public class SendStringPlugin : IPlugin, ISendStringService, IDynamicService
    {
        public event EventHandler<StringSentEventArgs>  StringSent;
        public event EventHandler<StringSendingEventArgs>  StringSending;

        public event EventHandler<NativeKeySentEventArgs>  KeySent;
        public event EventHandler<NativeKeySendingEventArgs>  KeySending;

        private CommonServices.ICommandManagerService _cm;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public CommonServices.ICommandManagerService CommandManager
        {
            get { return this._cm; }
            set { this._cm = value; }
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Teardown()
        {
        }

        public virtual void Start()
        {
            if( _cm != null )
                _cm.CommandSent += new EventHandler<CommonServices.CommandSentEventArgs>( OnCommandSent );
        }

        public virtual void Stop()
        {
            if( _cm != null )
                _cm.CommandSent -= new EventHandler<CommonServices.CommandSentEventArgs>( OnCommandSent );
        }

        protected void OnCommandSent( object sender, CommonServices.CommandSentEventArgs e )
        {
            if( e.Command.StartsWith( "sendString:" ) )
            {
                SendString( e.Command.Substring( "sendString:".Length ) );
            }
            else if( e.Command.StartsWith( "sendKey:" ) )
            {
                var keyString = e.Command.Substring( "sendKey:".Length );
                Native.KeyboardKeys result;
                if( Enum.TryParse<Native.KeyboardKeys>( keyString, true, out result ) )
                {
                    SendKeyboardKey( result );
                }
            }
        }

        public void SendString( string s )
        {
            SendString( this, s );
        }

        public void SendString( object sender, string s )
        {
            DoSendString( sender, s );
        }

        public void SendKeyboardKey( Native.KeyboardKeys key )
        {
            NativeKeySendingEventArgs e = new NativeKeySendingEventArgs( key );
            if( this.KeySending != null )
            {
                this.KeySending( this, e );
            }
            if( !e.Cancel )
            {
                KeyboardProcessor.Process( key );
                if( this.KeySent != null )
                {
                    this.KeySent( this, new NativeKeySentEventArgs( key ) );
                }
            }
        }

        private void DoSendString( object sender, string s )
        {
            StringSendingEventArgs e = new StringSendingEventArgs( s );
            if( this.StringSending != null )
            {
                this.StringSending( this, e );
            }
            if( !e.Cancel )
            {
                KeyboardProcessor.ProcessString( s );
                if( this.StringSent != null )
                {
                    this.StringSent( this, new StringSentEventArgs( s ) );
                }
            }
        }

    }
}

