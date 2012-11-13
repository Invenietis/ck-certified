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
        readonly ICommandTextualContextService _commandTextualContextService;
        readonly ITextualContextService _textualContext;
        string _selectedText;
        string _text;

        public TextualContextAreaViewModel( ITextualContextService textualContext, ICommandTextualContextService commandTextualContextService )
        {
            _textualContext = textualContext;
            _commandTextualContextService = commandTextualContextService;
            _commandTextualContextService.PredictionAreaContentSent += OnPredictionAreaContentSent;
            //_textualContext.Tokens.CollectionChanged += Tokens_CollectionChanged;
        }

        void OnPredictionAreaContentSent( object sender, PredictionAreaContentEventArgs e )
        {
            _text = String.Empty;
            OnPropertyChanged( "TextualContext" );
        }

        //void Tokens_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        //{
        //    if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset )
        //    {
        //        OnPropertyChanged( "TextualContext" );
        //    }
        //}

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
                    _textualContext.SetRawText( _text );
                }
            }
        }

        public int CaretIndex
        {
            get { return _textualContext.CaretOffset; }
            set
            {
                _textualContext.SetCaretIndex( value );
            }
        }

        public string TextualContext
        {
            get { return _text; }
            set
            {
                _text = value;
                _textualContext.SetRawText( value );
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
    }
}
