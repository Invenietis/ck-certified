using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace TextTemplate
{
    /// <summary>
    /// Interaction logic for Template.xaml
    /// </summary>
    public partial class TemplateEditor : Window
    {
        Dictionary<IText, TextBox> _bindings;
        TemplateEditorViewModel _model;

        public TemplateEditor(TemplateEditorViewModel model)
        {
            _model = model;
            InitializeComponent();
            RenderTemplate();
            DataContext = model;
            this.MouseDown += (o, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                    this.DragMove();
            };
        }

        void RenderTemplate()
        {
            StackPanel sp = (StackPanel)FindName("sheet");
            sp.MouseDown += (o, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                    this.DragMove();
            };

            WrapPanel wp = new WrapPanel();
            _bindings = new Dictionary<IText, TextBox>();

            foreach(IText text in _model.Template.TextFragments)
            {
                Label block;
                TextBox editable;

                if (text.IsEditable)
                {
                    editable = new TextBox();
                    editable.DataContext = text;
                    editable.Text = text.Placeholder;

                    var b =  new Binding("Text");
                    b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    b.Source = text;
                    editable.SetBinding(TextBox.TextProperty, b);
                    editable.GotFocus += (o, e) =>
                    {
                        editable.SelectAll();
                    };
                    editable.MouseUp += (o, e) =>
                    {
                        editable.SelectAll();
                    };
                    _bindings[text] = editable;
                    wp.Children.Add(editable);
                }
                else
                {
                    if (text is NewLine)
                    {
                        sp.Children.Add(wp);
                        wp = new WrapPanel();
                    }
                    else
                    {
                        block = new Label();
                        block.Content = text.Text;
                        wp.Children.Add(block);
                    }
                }
            }

            if( wp.Children.Count > 0 ) sp.Children.Add(wp);
        }

        public void FocusOnElement(IText text)
        {
            if (!_bindings.ContainsKey(text)) return;
            _bindings[text].Focus();
        }
    }
}
