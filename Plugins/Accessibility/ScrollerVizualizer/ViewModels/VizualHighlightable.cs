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

        public VizualHighlightable( IVizualizableHighlightableElement element )
        {
            Element = element;
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