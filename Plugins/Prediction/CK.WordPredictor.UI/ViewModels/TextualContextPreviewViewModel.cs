using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CK.Plugin;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.UI.ViewModels
{
    public class TextualContextPreviewViewModel : INotifyPropertyChanging, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        readonly IService<ITextualContextService> _textualContext;

        public TextualContextPreviewViewModel( IService<ITextualContextService> textualContext )
        {
            _textualContext = textualContext;
            _textualContext.Service.TextualContextChanged += TextualContext_PropertyChanged;
            _textualContext.ServiceStatusChanged += TextualContextService_ServiceStatusChanged;
        }

        private void TextualContext_PropertyChanged( object sender, EventArgs e )
        {
            OnPropertyChanged( "TextualContext" );
            OnPropertyChanged( "CurrentToken" );
            OnPropertyChanged( "CaretIndex" );
        }

        private void TextualContextService_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            OnPropertyChanged( "IsTextualContextServiceAvailable" );
        }

        public bool IsTextualContextServiceAvailable
        {
            get { return _textualContext.Service != null; }
        }

        public int CaretIndex
        {
            get
            {
                return IsTextualContextServiceAvailable && (_textualContext.Service.CaretOffset + _textualContext.Service.CurrentTokenIndex > 0) ? PreviousWordsLength().Sum() : 0;
            }
        }

        private IEnumerable<int> PreviousWordsLength()
        {
            return _textualContext.Service.Tokens.Take( _textualContext.Service.CurrentTokenIndex ).Select( t => t.Value.Length );
        }

        public string CurrentToken
        {
            get { return IsTextualContextServiceAvailable && _textualContext.Service.CurrentToken != null ? _textualContext.Service.CurrentToken.Value : String.Empty; }
        }

        public string TextualContext
        {
            get { return IsTextualContextServiceAvailable ? String.Join( " ", _textualContext.Service.Tokens.Select( x => x.Value ) ) : String.Empty; }
        }

        /// <summary>
        /// Raises this object's <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged( string propertyName )
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if( handler != null )
            {
                var e = new PropertyChangedEventArgs( propertyName );
                handler( this, e );
            }
        }

        public void Dispose()
        {
            _textualContext.Service.TextualContextChanged -= TextualContext_PropertyChanged;
            _textualContext.ServiceStatusChanged -= TextualContextService_ServiceStatusChanged;
        }

    }
}
