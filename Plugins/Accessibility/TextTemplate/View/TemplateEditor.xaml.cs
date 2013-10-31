using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextTemplate
{
    /// <summary>
    /// Interaction logic for Template.xaml
    /// </summary>
    public partial class TemplateEditor : Window
    {
        Dictionary<IText, UIElement> _bindings;
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
            WrapPanel wp = new WrapPanel();
            _bindings = new Dictionary<IText, UIElement>();

            foreach(IText text in _model.Template.TextFragments)
            {
                TextBlock block;
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
                        block = new TextBlock();
                        block.Text = text.Text;
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
