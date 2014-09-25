#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\TextTemplate\Word.cs) is part of CiviKey. 
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

using System;
using System.ComponentModel;
using CK.Core;
using HighlightModel;

namespace TextTemplate
{
    /// <summary>
    /// An IText whithout new lines and whitespaces (except user input whitespaces)
    /// </summary>
    public class Word : IText, INotifyPropertyChanged
    {
        protected string _text;
        protected bool _isHighlighted;
        protected TextTemplate _textTemplate;
        protected bool _selected;

        public bool IsEditable { get; private set; }

        public string Text
        {
            get { return _text; }
            set
            {
                if (!IsEditable) throw new InvalidOperationException("Text is not editable");
                _text = value;
                NotifyPropertyChanged("Text");
            }
        }

        public string Placeholder { get; set; }

        public Word(bool editable, string text, TextTemplate tt)
        {
            IsEditable = editable;
            _text = text;
            Placeholder = "";
            _textTemplate = tt;
        }

        public Word()
        {
            IsEditable = false;
            _text = "";
            Placeholder = "";
        }

        public ActionType ActionType { get; set; }

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return CKReadOnlyListEmpty<IHighlightableElement>.Empty; }
        }

        public int X { get { return 0; } }

        public int Y { get { return 0; } }

        public int Width { get { return 0; } }

        public int Height { get { return 0; } }

        public SkippingBehavior Skip
        {
            get { return IsEditable ? SkippingBehavior.None : SkippingBehavior.Skip; }
        }

        public bool IsHighlighted 
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;
                NotifyPropertyChanged("IsHighlighted");
            }
        }

        public bool IsSelected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                NotifyPropertyChanged( "IsSelected" );
            }
        }

        void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ScrollingDirective BeginHighlight(BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective)
        {
            _selected = false;
            IsHighlighted = true;

            if(_textTemplate != null) _textTemplate.FocusOnElement(this);
            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight(EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective)
        {
            IsHighlighted = false;

            if( !_selected ) _textTemplate.RemoveFocus( this ); //Keep the focus if the text is selected
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement(ScrollingDirective scrollingDirective)
        {
            _selected = true;
            scrollingDirective.NextActionType = HighlightModel.ActionType.GoToAbsoluteRoot;
            _textTemplate.FocusOnElement( this );
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return false; }
        }
    }

    //New line !
    public class NewLine : Word
    {
        public NewLine() : base()
        {
            _text = "\n";
        }
    }

    //New line !
    public class WhiteSpace : Word
    {
        public WhiteSpace()
            : base()
        {
            _text = " ";
        }
    }
}
