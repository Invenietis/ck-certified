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
        readonly ITextualContextService _textualContext;
        string _selectedText;

        public TextualContextAreaViewModel( ITextualContextService textualContext )
        {
            _textualContext = textualContext;
            _textualContext.Tokens.CollectionChanged += Tokens_CollectionChanged;
        }

        void Tokens_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset )
            {
                OnPropertyChanged( "TextualContext" );
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
                _textualContext.SetRawText( value );
            }
        }
    }
}
