using CK.Core;
using HighlightModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace TextTemplate
{
    /// <summary>
    /// An IText whithout new lines and whitespaces (except user input whitespaces)
    /// </summary>
    public class Word : IText, INotifyPropertyChanged
    {
        protected string _text;
        protected bool _isHighlighted;

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

        public Word(bool editable, string text)
        {
            IsEditable = editable;
            _text = text;
            Placeholder = "";
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

        void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    //New line !
    public class NewLine : Word
    {
        public NewLine() : base()
        {
            _text = Environment.NewLine;
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
