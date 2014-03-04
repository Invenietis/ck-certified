using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TextTemplate
{
    /// <summary>
    /// Interaction logic for Template.xaml
    /// </summary>
    public partial class TemplateEditor : Window
    {
        static readonly int LineHeight = 25;
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
            wp.Height = LineHeight;
            _bindings = new Dictionary<IText, TextBox>();

            foreach(IText text in _model.Template.TextFragments)
            {
                Label block;
                ClickSelectTextBox editable;
                Grid grid;
                Line line;

                if (text.IsEditable)
                {
                    grid = new Grid();
                    grid.ClipToBounds = false;
                    line = new Line();

                    editable = new ClickSelectTextBox(text.Text);
                    line.Stroke = (SolidColorBrush)FindResource( "GrayColor" );
                    line.StrokeThickness = 1.5;

                    grid.Children.Add( editable );
                    grid.Children.Add( line );

                    editable.DataContext = text;
                    editable.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                    var b =  new Binding("Text");

                    b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    b.Source = text;
                    editable.SetBinding(TextBox.TextProperty, b);

                    line.StrokeDashArray.Add( 0 );
                    line.StrokeDashArray.Add( 2.0 );
                    line.StrokeDashArray.Add( 0 );
                    line.Height = 1;
                    line.Width = editable.Width;
                    line.X1 = 0;
                    line.X2 = 1000;
                    line.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                    var b2 =new Binding("ActualWidth");
                    b2.Source = line;
                    line.SetBinding( Line.X2Property,  b2);

                    _bindings[text] = editable;
                    wp.Children.Add( grid );
                }
                else
                {
                    if (text is NewLine)
                    {
                        if( wp.Children.Count == 0 ) wp.Height = LineHeight;
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

        public void RemoveFocus( IText text )
        {
            if( !_bindings.ContainsKey( text ) ) return;
            TextBox textBox = _bindings[text];

            FrameworkElement parent = (FrameworkElement) textBox.Parent;
            while( parent != null && parent is IInputElement && !((IInputElement)parent).Focusable )
            {
                parent = (FrameworkElement)parent.Parent;
            }

            DependencyObject scope = FocusManager.GetFocusScope( textBox );
            FocusManager.SetFocusedElement( scope, parent as IInputElement );
        }
    }
}
