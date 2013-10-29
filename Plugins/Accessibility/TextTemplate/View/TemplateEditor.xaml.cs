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
        Template _template;

        public TemplateEditor(Template template)
        {
            _template = template;
            InitializeComponent();
            RenderTemplate();
        }

        void RenderTemplate()
        {
            StackPanel sp = (StackPanel)FindName("sheet");
            WrapPanel wp = new WrapPanel();

            foreach(IText text in _template.TextFragments)
            {
                TextBlock block;
                TextBox editable;

                if (text.IsEditable)
                {
                    editable = new TextBox();
                    editable.Text = text.Placeholder;
                    var b =  new Binding("Text");
                    b.Source = text;
                    editable.SetBinding(TextBox.TextProperty,b);
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
    }
}
