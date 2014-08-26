using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonServices.Accessibility;
using HighlightModel;

namespace ScrollerVizualizer
{
    public class VizualHighlightable
    {
        /// <summary>
        /// Wheter the element is highlight or not
        /// </summary>
        public bool IsHighlighted { get; set; }

        public string Name { get { return Element.Name; } }

        /// <summary>
        /// The wrapped vizualizable elemen
        /// </summary>
        public IVizualizableHighlightableElement Element { get; private set; }

        public VizualHighlightable( IVizualizableHighlightableElement element )
        {
            Element = element;
        }
    }
}