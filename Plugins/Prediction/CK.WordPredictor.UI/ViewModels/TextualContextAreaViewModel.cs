#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor.UI\ViewModels\TextualContextAreaViewModel.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.ComponentModel;
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
            _commandTextualContextService = commandTextualContextService;
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
                    _predictionTextArea.ChangePredictionAreaContent( _text, _caretIndex );
                }

                PropertyChanged( this, new PropertyChangedEventArgs( "IsFocused" ) );
            }
        }

        int _caretIndex;

        public int CaretIndex
        {
            get { return _caretIndex; }
            set
            {
                _caretIndex = value;
                PropertyChanged( this, new PropertyChangedEventArgs( "CaretIndex" ) );
            }
        }

        public string TextualContext
        {
            get { return _text; }
            set
            {
                _text = value;
                PropertyChanged( this, new PropertyChangedEventArgs( "TextualContext" ) );
            }
        }
    }
}
