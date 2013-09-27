using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CK.Plugins.SendInputDriver;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.UI.ViewModels
{
    public class TextualContextAreaViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        readonly ITextualContextService _textualContext;
        readonly IPredictionTextAreaService _predictionTextArea;
        readonly ICommandTextualContextService _commandTextualContextService;
        string _text;

        public TextualContextAreaViewModel( ITextualContextService textualContext, IPredictionTextAreaService predictionTextArea, ICommandTextualContextService commandTextualContextService )
        {
            _textualContext = textualContext;
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
            set
            {
                _isFocused = value;
                if( _isFocused )
                {
                    _commandTextualContextService.ClearTextualContext();
                    _predictionTextArea.Text = _text;
                }
                else
                {
                    _commandTextualContextService.ClearTextualContext();
                }

                PropertyChanged( this, new PropertyChangedEventArgs( "IsFocused" ) );
            }
            get { return _isFocused; }
        }

        public int CaretIndex
        {
            get { return _predictionTextArea.CaretIndex; }
            set { _predictionTextArea.CaretIndex = value; }
        }

        public string TextualContext
        {
            get { return _predictionTextArea.Text; }
            set 
            {
                _text = _predictionTextArea.Text = value; 
            }
        }
    }
}
