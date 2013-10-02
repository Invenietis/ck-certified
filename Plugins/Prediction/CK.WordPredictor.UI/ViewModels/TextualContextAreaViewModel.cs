using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;
using CK.Plugins.SendInputDriver;
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

        public int CaretIndex
        {
            get { return _predictionTextArea.CaretIndex; }
            set 
            {
                //_predictionTextArea.Text = _text;
                _predictionTextArea.CaretIndex = value;
                PropertyChanged( this, new PropertyChangedEventArgs( "CaretIndex" ) );
            }
        }

        public string TextualContext
        {
            get { return _text; }
            set
            {
                _text = value;
                //_predictionTextArea.Text = value;
                PropertyChanged( this, new PropertyChangedEventArgs( "TextualContext" ) );
            }
        }
    }
}
