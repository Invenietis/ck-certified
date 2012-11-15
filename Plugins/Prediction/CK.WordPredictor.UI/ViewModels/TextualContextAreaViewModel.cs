using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CK.Plugins.SendInput;
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

        public bool IsFocused
        {
            set
            {
                if( value == false )
                {
                    _commandTextualContextService.ClearTextualContext();
                }
                if( value == true )
                {
                    _commandTextualContextService.ClearTextualContext();
                    _predictionTextArea.Text = _text;
                }
            }
        }

        public int CaretIndex
        {
            get { return _predictionTextArea.CaretIndex; }
            set { _predictionTextArea.CaretIndex = value; }
        }

        public string TextualContext
        {
            get { return _predictionTextArea.Text; }
            set { _text = _predictionTextArea.Text = value; }
        }
    }
}
