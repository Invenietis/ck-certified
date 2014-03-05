using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace TextTemplate
{
    public class ContentControlDuFutur : ContentControl
    {
        IText _text;
        Dictionary<IText, TextBox> _bindings;

        public ContentControlDuFutur(IText text,  Dictionary<IText, TextBox> bindings)
        {
            _text = text;
            _bindings = bindings;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var tb = (ClickSelectTextBox) Template.FindName( "textbox", this );
            
            tb.GotFocus += ( o, e ) => { _text.IsSelected = true; };
            tb.LostFocus += ( o, e ) => { _text.IsSelected = false; };

            _bindings[_text] = tb;
        }
    }

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
            _model.Cancel.PropertyChanged += ( o, e ) => { if(_model.Cancel.IsHighlighted) ((Button)FindName( "cancel" )).Focus(); };
            _model.ValidateTemplate.PropertyChanged += ( o, e ) => { if( _model.ValidateTemplate.IsHighlighted ) ((Button)FindName( "ok" )).Focus(); };
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
                ContentControlDuFutur cc;

                if (text.IsEditable)
                {
                    cc = new ContentControlDuFutur( text, _bindings ) 
                    { 
                        DataContext = text,
                        Style = (Style) FindResource( "textcontrol" )
                    };
     
                    wp.Children.Add( cc );
                }
                else
                {
                    if (text is NewLine)
                    {
                        if( wp.Children.Count == 0 ) wp.MinHeight = LineHeight;
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

        /// <summary>
        /// Give the focus to the parent
        /// </summary>
        /// <param name="text"></param>
        public void RemoveFocus( IText text )
        {
            if( !_bindings.ContainsKey( text ) ) return;
            TextBox textBox = _bindings[text];
            text.IsSelected = false;

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
