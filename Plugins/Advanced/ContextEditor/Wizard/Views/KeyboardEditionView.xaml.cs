using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CK.WPF.StandardViews;
using ContextEditor.ViewModels;

namespace ContextEditor.s
{
    /// <summary>
    /// Interaction logic for KeyboardEditionView.xaml
    /// </summary>
    public partial class KeyboardEditionView : UserControl
    {
        public KeyboardEditionView()
        {
            InitializeComponent();
            
            Loaded += KeyboardEditionView_Loaded;
        }

        void KeyboardEditionView_Loaded( object sender, RoutedEventArgs e )
        {
            foreach( StdKeyView key in FindVisualChildren<StdKeyView>( KeyboardContainer ) )
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer( key );
                layer.Add( new ResizingAdorner( key ) );
            }

            //this.MouseLeftButtonDown += new MouseButtonEventHandler( Window1_MouseLeftButtonDown );
            //this.MouseLeftButtonUp += new MouseButtonEventHandler( DragFinishedMouseHandler );
            //this.MouseMove += new MouseEventHandler( Window1_MouseMove );
            //this.MouseLeave += new MouseEventHandler( Window1_MouseLeave );

            //KeyboardContainer.PreviewMouseLeftButtonDown += new MouseButtonEventHandler( myCanvas_PreviewMouseLeftButtonDown );
            //KeyboardContainer.PreviewMouseLeftButtonUp += new MouseButtonEventHandler( DragFinishedMouseHandler );
        }

        public static IEnumerable<T> FindVisualChildren<T>( DependencyObject depObj ) where T : DependencyObject
        {
            if( depObj != null )
            {
                for( int i = 0; i < VisualTreeHelper.GetChildrenCount( depObj ); i++ )
                {
                    DependencyObject child = VisualTreeHelper.GetChild( depObj, i );
                    if( child != null && child is T )
                    {
                        yield return (T)child;
                    }

                    foreach( T childOfChild in FindVisualChildren<T>( child ) )
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        // AdornerLayer aLayer;

        //bool _isDown;
        //bool _isDragging;
        //bool selected = false;
        //UIElement selectedElement = null;

        //Point _startPoint;
        //private double _originalLeft;
        //private double _originalTop;

        //// Handler for drag stopping on leaving the window
        //void Window1_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    StopDragging();
        //    e.Handled = true;
        //}

        //// Handler for drag stopping on user choise
        //void DragFinishedMouseHandler(object sender, MouseButtonEventArgs e)
        //{
        //    StopDragging();
        //    e.Handled = true;
        //}

        //// Method for stopping dragging
        //private void StopDragging()
        //{
        //    if (_isDown)
        //    {
        //        _isDown = false;
        //        _isDragging = false;
        //    }
        //}
        
        //// Hanler for providing drag operation with selected element
        //void Window1_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (_isDown)
        //    {
        //        if ((_isDragging == false) &&
        //            ( ( Math.Abs( e.GetPosition( KeyboardContainer ).X - _startPoint.X ) > SystemParameters.MinimumHorizontalDragDistance ) ||
        //            ( Math.Abs( e.GetPosition( KeyboardContainer ).Y - _startPoint.Y ) > SystemParameters.MinimumVerticalDragDistance ) ) )
        //            _isDragging = true;

        //        if (_isDragging)
        //        {
        //            Point position = Mouse.GetPosition( KeyboardContainer );
        //            Canvas.SetTop(selectedElement, position.Y - (_startPoint.Y - _originalTop));
        //            Canvas.SetLeft(selectedElement, position.X - (_startPoint.X - _originalLeft));
        //        }
        //    }
        //}        
                        
        //// Handler for clearing element selection, adorner removal
        //void Window1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{            
        //    if (selected)
        //    {
        //        selected = false;
        //        if (selectedElement != null)
        //        {
        //            aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
        //            selectedElement = null;
        //        }                
        //    }            
        //}

        //// Handler for element selection on the canvas providing resizing adorner
        //void myCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    // Remove selection on clicking anywhere the window
        //    if (selected)
        //    {
        //        selected = false;
        //        if (selectedElement != null)
        //        {
        //            // Remove the adorner from the selected element
        //            aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);                    
        //            selectedElement = null;
        //        }
        //    }

        //    // If any element except canvas is clicked, 
        //    // assign the selected element and add the adorner
        //    if( e.Source != KeyboardContainer )
        //    {
        //        _isDown = true;
        //        _startPoint = e.GetPosition( KeyboardContainer );

        //        selectedElement = e.Source as UIElement;

        //        _originalLeft = Canvas.GetLeft(selectedElement);
        //        _originalTop = Canvas.GetTop(selectedElement);

        //        aLayer = AdornerLayer.GetAdornerLayer(selectedElement);
        //        aLayer.Add(new ResizingAdorner(selectedElement));
        //        selected = true;
        //        e.Handled = true;
        //    }
        //}
    }
}
