using System;
using System.ComponentModel;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.UI.ViewModels
{
    public class TextualContextAreaViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        readonly IPredictionTextAreaService _predictionTextArea;
        readonly ICommandTextualContextService _commandTextualContextService;
        string _text;

        public TextualContextAreaViewModel( IPredictionTextAreaService predictionTextArea, ICommandTextualContextService commandTextualContextService )
        {
            _predictionTextArea = predictionTextArea;
            _predictionTextArea.TextSent += OnPredictionAreaContentSent;
            _commandTextualContextService = commandTextualContextService;
        }

        void OnPredictionAreaContentSent( object sender, PredictionAreaContentEventArgs e )
        {
            _text = _predictionTextArea.Text = String.Empty;
            if( PropertyChanged != null )
                PropertyChanged( this, new PropertyChangedEventArgs( "TextualContext" ) );
        }

        bool _isFocused;
        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                _isFocused = value;
                _commandTextualContextService.ClearTextualContext();

                if( _isFocused )
                {
                    _predictionTextArea.Text = _text;
                }

                PropertyChanged( this, new PropertyChangedEventArgs( "IsFocused" ) );
            }
        }

        int _caretIndex;

        public int CaretIndex
        {
            get { return _caretIndex; }
            set
            {
                _caretIndex = value;
                PropertyChanged( this, new PropertyChangedEventArgs( "CaretIndex" ) );
            }
        }

        public string TextualContext
        {
            get { return _text; }
            set
            {
                _text = value;
                PropertyChanged( this, new PropertyChangedEventArgs( "TextualContext" ) );
            }
        }
    }
}
