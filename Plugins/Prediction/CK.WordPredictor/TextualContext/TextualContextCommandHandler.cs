using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicCommandHandlers;
using CK.Plugin;
using CK.Plugins.SendInputDriver;
using CK.WordPredictor.Model;
using CommonServices;

namespace CK.WordPredictor
{
    [Plugin( "{B2A76BF2-E9D2-4B0B-ABD4-270958E17DA0}", PublicName = "TextualContext - Command Handler", Categories = new string[] { "Prediction" } )]
    public class TextualContextCommandHandler : BasicCommandHandler, ICommandTextualContextService
    {
        public const string CMDClearTextualContext = "clearTextualContext";

        public event EventHandler TextualContextClear;

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( e.Command != null && e.Command.Contains( CMDClearTextualContext ) )
            {
                ClearTextualContext();
            }
        }

        public void ClearTextualContext()
        {
            if( TextualContextClear != null )
                TextualContextClear( this, EventArgs.Empty );
        }
    }
}
