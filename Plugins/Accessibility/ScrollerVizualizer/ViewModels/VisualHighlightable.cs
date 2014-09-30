using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using HighlightModel;

namespace ScrollerVisualizer
{
    public class VisualHighlightable : INotifyPropertyChanged
    {
        bool _isHighlighted;
        bool _isSelected;
        public VisualHighlightable Parent { get; set; }
        public List<VisualHighlightable> Children { get; set; }

        public bool HasChildren { get { return Children.Count > 0; } }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                FirePropertyChanged( "IsSelected" );
            }
        }

        public bool IsHighlighted 
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;
                FirePropertyChanged( "IsHighlighted" );
            }
        }
        
        public string Name { get { return Element.ElementName; } }

        public string ImageSource { get { return Element.VectorImagePath; } }

        /// <summary>
        /// The wrapped vizualizable elemen
        /// </summary>
        public IVisualizableHighlightableElement Element { get; private set; }

        public VisualHighlightable( IVisualizableHighlightableElement element, VisualHighlightable parent = null )
        {
            Element = element;
            Children = element.Children
                .Where( x => (x as IVisualizableHighlightableElement) != null )
                .Select( x => new VisualHighlightable( x as IVisualizableHighlightableElement, this ) )
                .ToList();

            Parent = parent;
        }

        void FirePropertyChanged(string propertyName)
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}