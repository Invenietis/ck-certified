using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.WordPredictor.Model;
using CommonServices;

namespace CK.WordPredictor
{
    [Plugin( "{55C2A080-30EB-4CC6-B602-FCBBF97C8BA5}", PublicName = "WordPrediction - TextArea Bus", Categories = new string[] { "Prediction", "Advcanced" } )]
    public class PredictionTextAreaBus : BasicCommandHandler, IPredictionTextAreaService
    {
        public const string CMDSendPredictionAreaContent = "sendPredictionAreaContent";

        public event PropertyChangedEventHandler PropertyChanged;

        string _text;
        int _caretIndex;

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                if( PropertyChanged != null )
                    PropertyChanged( this, new PropertyChangedEventArgs( "Text" ) );
            }
        }

        public int CaretIndex
        {
            get
            {
                return _caretIndex;
            }
            set
            {
                _caretIndex = value;

                if( PropertyChanged != null )
                    PropertyChanged( this, new PropertyChangedEventArgs( "CaretIndex" ) );
            }
        }

        public event EventHandler<PredictionAreaContentEventArgs> TextSent;

        public void SendText()
        {
            if( TextSent != null && Text != null )
                TextSent( this, new PredictionAreaContentEventArgs( Text ) );
        }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( e.Command != null && e.Command.Contains( CMDSendPredictionAreaContent ) )
                SendText();
        }
    }
}
