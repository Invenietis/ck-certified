using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WordPredictor.Model;
using CK.WPF.ViewModel;

namespace CK.WordPredictor.UI.ViewModels
{
    public class TextualContextPreviewViewModel : VMBase
    {
        ITextualContextService _textualContext;

        public TextualContextPreviewViewModel( ITextualContextService textualContext )
        {
            _textualContext = textualContext;
            _textualContext.PropertyChanged += TextualContext_PropertyChanged;
        }

        private void TextualContext_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            OnPropertyChanged( "TextualContext" );
            OnPropertyChanged( "CurrentToken" );
            OnPropertyChanged( "CaretIndex" );
        }

        public int CaretIndex
        {
            get { return _textualContext.CaretOffset + _textualContext.CurrentTokenIndex > 0 ? PreviousWordsLength().Sum() : 0; }
        }

        private IEnumerable<int> PreviousWordsLength()
        {
            return _textualContext.Tokens.Take( _textualContext.CurrentTokenIndex ).Select( t => t.Value.Length );
        }

        public string CurrentToken
        {
            get { return _textualContext.CurrentToken != null ? _textualContext.CurrentToken.Value : String.Empty; }
        }

        public string TextualContext
        {
            get { return String.Join( " ", _textualContext.Tokens.Select( x => x.Value ) ); }
        }
    }
}
