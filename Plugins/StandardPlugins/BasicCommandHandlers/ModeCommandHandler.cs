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
