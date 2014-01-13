using System.Windows;

namespace SimpleSkin.Helpers
{
    public class DraggableVisualAttachedProperty
    {
        public static bool GetDraggableVisual( DependencyObject obj )
        {
            return (bool)obj.GetValue( DraggableVisualProperty );
        }

        public static void SetDraggableVisual( DependencyObject obj, bool value )
        {
            obj.SetValue( DraggableVisualProperty, value );
        }

        public static readonly DependencyProperty DraggableVisualProperty =
        DependencyProperty.RegisterAttached( "IsDraggableVisual", typeof( bool ), typeof( DraggableVisualAttachedProperty ) );
    }
}
