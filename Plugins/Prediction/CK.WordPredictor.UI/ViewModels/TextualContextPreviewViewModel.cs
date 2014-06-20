#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor.UI\ViewModels\TextualContextPreviewViewModel.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
            if( _textualContext.Status.IsStartingOrStarted ) _textualContext.Service.TextualContextChanged += TextualContext_PropertyChanged;
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
            if( e.Current == InternalRunningStatus.Started ) _textualContext.Service.TextualContextChanged += TextualContext_PropertyChanged;
            else if( e.Current == InternalRunningStatus.Stopping ) _textualContext.Service.TextualContextChanged -= TextualContext_PropertyChanged;
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
            if( _textualContext.Status.IsStartingOrStarted ) _textualContext.Service.TextualContextChanged -= TextualContext_PropertyChanged;
            _textualContext.ServiceStatusChanged -= TextualContextService_ServiceStatusChanged;
        }

    }
}
