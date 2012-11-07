using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CK.Plugins.SendInput;
using CK.WordPredictor.Model;
using CK.WPF.ViewModel;

namespace CK.WordPredictor.UI.ViewModels
{
    public class TextualContextAreaViewModel : VMBase
    {
        readonly ISendStringService _sendStringService;
        readonly TextualContextSmartArea _textualContext;
        string _selectedText;

        public TextualContextAreaViewModel( TextualContextSmartArea textualContext, ISendStringService sendStringService )
        {
            _textualContext = textualContext;
            _sendStringService = sendStringService;
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

        ICommand _sendTextCommand;

        public ICommand SendTextCommand
        {
            get
            {
                return _sendTextCommand ?? (_sendTextCommand = new VMCommand<string>( ( text ) =>
                    {
                        _sendStringService.SendString( this, text );
                    } ));
            }
        }
    }
}
