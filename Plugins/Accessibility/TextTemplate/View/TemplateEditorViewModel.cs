using CK.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace TextTemplate
{
    public class TemplateEditorViewModel
    {
        ICommand _ValidateTemplate;

        public event EventHandler TemplateValidated;

        public Template Template { get; set; }

        public Color HighlightColor { get { return Color.FromRgb(200, 12, 89); }  }

        public ICommand ValidateTemplate
        {
            get
            {
                if (_ValidateTemplate == null) _ValidateTemplate = new VMCommand(OnValidateTemplate);

                return _ValidateTemplate;
            }
        }

        void OnValidateTemplate()
        {
            FireTemplateValidated();
        }

        void FireTemplateValidated()
        {
            if (TemplateValidated != null) TemplateValidated(this, new EventArgs());
        }
    }
}
