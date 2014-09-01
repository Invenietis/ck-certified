using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KeyboardEditor.Model;
using KeyboardEditor.ViewModels;

namespace KeyboardEditor.s
{
    /// <summary>
    /// Interaction logic for KeyboardEditionView.xaml
    /// </summary>
    public partial class KeyboardEditionView : UserControl
    {
        public KeyboardEditionView()
        {
            InitializeComponent();
        }

        Point _lastMouseDown;

        FrameworkElement DragDropRoot { get { return EditorTreeView; } }

        IHandleDragDrop _draggedItem;
        IHandleDragDrop _target;

        private void DnDMouseDown( object sender, MouseButtonEventArgs e )
        {
            if( e.ChangedButton == MouseButton.Left )
            {
                _lastMouseDown = e.GetPosition( this );
            }
        }

        private void DnDMouseMove( object sender, MouseEventArgs e )
        {
            if( e.LeftButton == MouseButtonState.Pressed )
            {
                Point position = e.GetPosition( DragDropRoot );

                if( Math.Abs( position.X - _lastMouseDown.X ) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs( position.Y - _lastMouseDown.Y ) > SystemParameters.MinimumVerticalDragDistance )
                {
                    _draggedItem = EditorTreeView.SelectedItem as IHandleDragDrop;
                    if( _draggedItem != null && _draggedItem.IsDragDropEnabled )
                    {
                        DragDropEffects finalDropEffect = DragDrop.DoDragDrop( DragDropRoot, _draggedItem,
                            DragDropEffects.Move );

                        // _target is set right after a Drop.
                        if( (finalDropEffect == DragDropEffects.Move) && (_target != null) )
                        {
                            if( !_draggedItem.Equals( _target ) )
                            {
                                DropAction( _draggedItem, _target );
                            }

                            _target = null;
                            _draggedItem = null;
                        }
                    }
                }
            }
        }
        private void DnDDragOver( object sender, DragEventArgs e )
        {
            Point currentPosition = e.GetPosition( EditorTreeView );

            if( (Math.Abs( currentPosition.X - _lastMouseDown.X ) > SystemParameters.MinimumHorizontalDragDistance) ||
                (Math.Abs( currentPosition.Y - _lastMouseDown.Y ) > SystemParameters.MinimumVerticalDragDistance) )
            {
                IHandleDragDrop target = GetNearestContainer( e.OriginalSource as FrameworkElement );

                if( target != null && target.IsDragDropEnabled && CheckDropTarget( _draggedItem, target ) )
                {
                    e.Effects = DragDropEffects.Move;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            e.Handled = true;
        }
        private void DnDDrop( object sender, DragEventArgs e )
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            IHandleDragDrop target = GetNearestContainer( e.OriginalSource as FrameworkElement );
            if( target != null && _draggedItem != null )
            {
                _target = target;
                e.Effects = DragDropEffects.Move;
            }
        }

        private bool CheckDropTarget( IHandleDragDrop sourceItem, IHandleDragDrop targetItem )
        {
            return targetItem.CanBeDropTarget( sourceItem )
                && sourceItem.CanBeDropSource( targetItem );
        }

        private void DropAction( IHandleDragDrop sourceItem, IHandleDragDrop targetItem )
        {
            targetItem.ExecuteDropAction( sourceItem );
        }

        private IHandleDragDrop GetNearestContainer( FrameworkElement element )
        {
            IHandleDragDrop container = null;
            if( element != null )
            {
                container = (element as FrameworkElement).DataContext as IHandleDragDrop;
                while( container == null && element != null )
                {
                    element = VisualTreeHelper.GetParent( element ) as FrameworkElement;
                    if( element != null )
                        container = element.DataContext as IHandleDragDrop;
                }
            }
            return container;
        }
    }
}
