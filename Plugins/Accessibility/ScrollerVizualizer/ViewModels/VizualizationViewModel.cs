using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using HighlightModel;
using CommonServices.Accessibility;

namespace ScrollerVizualizer
{
    public class VizualizationViewModel
    {
        public ObservableCollection<VizualHighlightable> Elements { get; private set; }

        public VizualizationViewModel(IEnumerable<IVizualizableHighlightableElement> elements, IHighlighterService scroller )
        {
            Elements = new ObservableCollection<VizualHighlightable>( elements.Select( x => new VizualHighlightable( x ) ).ToArray() );
            scroller.BeginHighlight += (o, e) => 
            {
                var v = Elements.FirstOrDefault( x => x.Element == e.Element );
                if( v != null )
                    v.IsHighlighted = true;
            };
            scroller.EndHighlight += ( o, e ) =>
            {
                var v = Elements.FirstOrDefault( x => x.Element == e.Element );
                if(v != null)
                    v.IsHighlighted = false;
            };
            scroller.ElementRegisteredOrUnregistered += ( o, e ) =>
            {
                IVizualizableHighlightableElement vEl = e.Element as IVizualizableHighlightableElement;

                if( e.HasRegistered && vEl != null)
                    Elements.Add( new VizualHighlightable( vEl ) );
                else if(vEl != null)
                    Elements.Remove( new VizualHighlightable( vEl ) );
            };
        }
    }
}
