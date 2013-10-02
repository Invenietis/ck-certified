using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Plugin;
using CK.Plugins.SendInputDriver;
using CK.WordPredictor.Model;
using CommonServices;

namespace CK.WordPredictor
{
    [Plugin( "{55C2A080-30EB-4CC6-B602-FCBBF97C8BA5}", PublicName = "WordPrediction - TextArea Bus", Categories = new string[] { "Prediction", "Advcanced" } )]
    public class PredictionTextAreaBus : BasicCommandHandler, IPredictionTextAreaService
    {
        public const string CMDSendPredictionAreaContent = "sendPredictionAreaContent";

        public event PropertyChangedEventHandler PropertyChanged;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ISendStringService> SendStringService { get; set; }

        public override void Start()
        {
            base.Start();
            SendStringService.Service.StringSending += Service_StringSending;
            SendStringService.Service.StringSent += Service_StringSent;
        }

        public override void Stop()
        {
            SendStringService.Service.StringSending -= Service_StringSending;
            SendStringService.Service.StringSent -= Service_StringSent;
            base.Stop();
        }

        bool _sending;

        void Service_StringSending( object sender, StringSendingEventArgs e )
        {
            _sending = true;
        }

        void Service_StringSent( object sender, StringSentEventArgs e )
        {
            _sending = false;
        }

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
                if( _sending == false && PropertyChanged != null )
                {
                    PropertyChanged( this, new PropertyChangedEventArgs( "Text" ) );
                }
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

        public event EventHandler<IsDrivenChangedEventArgs> IsDrivenChanged;
        bool _isDriven;
        public bool IsDriven
        {
            get
            {
                return _isDriven;
            }
            set
            {
                _isDriven = value;
                if( IsDrivenChanged != null ) IsDrivenChanged( this, new IsDrivenChangedEventArgs( value ) );
            }
        }
    }
}
