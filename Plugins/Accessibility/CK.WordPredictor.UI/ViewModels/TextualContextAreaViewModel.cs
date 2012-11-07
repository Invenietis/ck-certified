using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WordPredictor.Model;
using CK.WPF.ViewModel;

namespace CK.WordPredictor.UI.ViewModels
{
    public class TextualContextAreaViewModel : VMBase
    {
        TextualContextSmartArea _textualContext;
        string _selectedText;

        public TextualContextAreaViewModel( TextualContextSmartArea textualContext )
        {
            _textualContext = textualContext;
        }

        public int CaretIndex
        {
            get { return _textualContext.CaretOffset; }
            set
            {
                _textualContext.SetCaretIndex( value );
            }
        }

        public string SelectedText
        {
            get
            {
                return _selectedText;
            }
            set
            {
                _selectedText = value;
                OnPropertyChanged( "SelectedText" );
            }
        }

        public int WordCount { get; set; }

        public int CharacterCount { get; set; }

        public string TextualContext
        {
            get { return String.Join( " ", _textualContext.Tokens.Select( e => e.Value ) ); }
            set
            {
                _textualContext.SetText( value );
            }
        }
    }
}
