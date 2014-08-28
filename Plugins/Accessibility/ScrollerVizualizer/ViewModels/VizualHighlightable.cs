using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CommonServices.Accessibility;
using HighlightModel;

namespace ScrollerVizualizer
{
    public class VizualHighlightable : INotifyPropertyChanged
    {
        bool _isHighlighted;
        bool _isSelected;
        public VizualHighlightable Parent { get; set; }
        public List<VizualHighlightable> Children { get; set; }

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

        public string ImageSource { get { return Element.ImagePath; } }

        /// <summary>
        /// The wrapped vizualizable elemen
        /// </summary>
        public IVizualizableHighlightableElement Element { get; private set; }

        public VizualHighlightable( IVizualizableHighlightableElement element, VizualHighlightable parent = null )
        {
            Element = element;
            Children = element.Children
                .Where( x => (x as IVizualizableHighlightableElement) != null )
                .Select( x => new VizualHighlightable( x as IVizualizableHighlightableElement, this ) )
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