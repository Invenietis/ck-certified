#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\TextTemplate\View\TemplateEditor.xaml.cs) is part of CiviKey. 
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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            KeyDown += _model.Window_KeyDown;

            this.ContentRendered += ( o, e ) => FocusOnElement( _bindings.Keys.FirstOrDefault() );
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

            if (text == null || !_bindings.ContainsKey(text)) return;
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
