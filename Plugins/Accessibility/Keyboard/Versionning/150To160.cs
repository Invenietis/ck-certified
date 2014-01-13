using CK.Keyboard.Model;
using System;

namespace CK.Keyboard.Versionning
{
    internal static class V150To160
    {
        internal static void Key150To160( IKey k )
        {
            //Should be in CK.Keyboard
            foreach( var keyMode in k.KeyModes )
            {
                ProcessKeyProgram150To160( keyMode.OnKeyDownCommands );
                ProcessKeyProgram150To160( keyMode.OnKeyUpCommands );
                ProcessKeyProgram150To160( keyMode.OnKeyPressedCommands );
            }
        }

        private static void ProcessKeyProgram150To160( IKeyProgram keyProgram )
        {
            for( int i = 0; i < keyProgram.Commands.Count; i++ )
            {
                string command = String.Empty;
                string parameter = String.Empty;

                if( ProcessKeyCommand150To160( keyProgram.Commands[i], out command, out parameter ) )
                {
                    keyProgram.Commands.RemoveAt( i );
                    keyProgram.Commands.Insert( i, String.Format( "{0}:{1}", command, parameter ) );
                }
            }
        }

        private static bool ProcessKeyCommand150To160( string keyCommand, out string command, out string parameter )
        {
            command = String.Empty;
            parameter = String.Empty;

            if( keyCommand.StartsWith( "Mode." ) )
            {
                command = "mode";

                if( keyCommand.StartsWith( "Mode.Toggle" ) )
                {
                    parameter = "toggle,";
                }
                else if( keyCommand.StartsWith( "Mode.Remove" ) )
                {
                    parameter = "remove,";
                }
                else if( keyCommand.StartsWith( "Mode.Add" ) )
                {
                    parameter = "add,";
                }
                else if( keyCommand.StartsWith( "Mode.Set" ) )
                {
                    parameter = "set,";
                }
                else
                {
                    return false;
                }

                parameter += keyCommand.Substring( keyCommand.IndexOf( '"' ) + 1, ( keyCommand.Length - keyCommand.IndexOf( '"' ) ) - ( keyCommand.Length - keyCommand.LastIndexOf( '"' ) + 1 ) );
            }
            else if( keyCommand.StartsWith( "sendKeyOld" ) )
            {
                command = "sendkeyold";
                parameter = keyCommand.Substring( keyCommand.IndexOf( '"' ) + 1, ( keyCommand.Length - keyCommand.IndexOf( '"' ) ) - ( keyCommand.Length - keyCommand.LastIndexOf( '"' ) + 1 ) );
            }
            else if( keyCommand.StartsWith( "MonitorOnce" ) )
            {
                command = "monitoronce";
                parameter = "sendkey"; //Before 1.5.0, only sendkey was used
                string eventName = keyCommand.Split( '"' )[1];

                string innerCommand = String.Empty;
                string innerParameter = String.Empty;
                ProcessKeyCommand150To160( keyCommand.Substring( keyCommand.IndexOf( ":" ) + 1 ), out innerCommand, out innerParameter );

                parameter = String.Format( "{0},{1},{2}:{3}", parameter, eventName, innerCommand, innerParameter );
            }
            else if( keyCommand.StartsWith( "DynCommand" ) )
            {
                command = "dyncommand";
                parameter = keyCommand.Substring( keyCommand.IndexOf( '"' ) + 1, ( keyCommand.Length - keyCommand.IndexOf( '"' ) ) - ( keyCommand.Length - keyCommand.LastIndexOf( '"' ) + 1 ) );
                parameter = parameter.ToLowerInvariant();
            }
            else
            {
                return false;
            }

            return true;
        }

    }
}
