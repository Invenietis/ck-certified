using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextTemplate
{
    public class TextFragment : IText
    {
        string _text;

        public bool IsEditable { get; private set; }

        public string Text
        {
            get { return _text; }
            set
            {
                if (!IsEditable) throw new InvalidOperationException("Text is not editable");
                _text = value;
            }
        }

        public string Placeholder { get; set; }

        public TextFragment(bool editable, string text)
        {
            IsEditable = editable;
            _text = text;
            Placeholder = "";
        }
    }

    //New line !
    public class NewLine : IText
    {
        public bool IsEditable { get { return false; } }

        public string Text { get { return Environment.NewLine; } set { } }

        public string Placeholder { get { return ""; } set { } }
    }
}
