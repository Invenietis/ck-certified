using System;
using CK.Plugin;
using CK.Plugins.SendInputDriver;
using CK.WordPredictor.Model;
using CommonServices;

namespace CK.WordPredictor
{
    [Plugin( "{55C2A080-30EB-4CC6-B602-FCBBF97C8BA5}", PublicName = "WordPrediction - TextArea Bus", Categories = new string[] { "Prediction", "Advcanced" } )]
    public class PredictionTextAreaBus : BasicCommandHandler, IPredictionTextAreaService
    {
        string _text;
        int _caretIndex;

        public const string CMDSendPredictionAreaContent = "sendPredictionAreaContent";

        public event EventHandler<PredictionAreaContentEventArgs> PredictionAreaContentChanged;
        
        public event EventHandler<PredictionAreaContentEventArgs> PredictionAreaTextSent;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ISendStringService> SendStringService { get; set; }

        public override void Start()
        {
            base.Start();
            PredictionAreaContentChanged += OnPredictionAreaContentChanged;

        }

        public override void Stop()
        {
            PredictionAreaContentChanged -= OnPredictionAreaContentChanged;
            base.Stop();
        }

        protected virtual void OnPredictionAreaContentChanged( object sender, PredictionAreaContentEventArgs e )
        {
            _text = e.Text;
            _caretIndex = e.CaretIndex;
        }

        void IPredictionTextAreaService.ChangePredictionAreaContent( string text, int caretIndex )
        {
            if( _text != text || _caretIndex != caretIndex )
            {
                if( PredictionAreaContentChanged != null )
                    PredictionAreaContentChanged( this, new PredictionAreaContentEventArgs( text, caretIndex ) );
            }
        }

        void IPredictionTextAreaService.SendText()
        {
            if( PredictionAreaTextSent != null )
                PredictionAreaTextSent( this, new PredictionAreaContentEventArgs( _text, _caretIndex ) );
        }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( e.Command != null && e.Command.Contains( CMDSendPredictionAreaContent ) )
                ((IPredictionTextAreaService)this).SendText();
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
