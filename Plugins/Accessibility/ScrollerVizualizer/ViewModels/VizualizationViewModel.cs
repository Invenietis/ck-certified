using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using HighlightModel;
using CommonServices.Accessibility;
using SimpleSkin.ViewModels;

namespace ScrollerVizualizer
{
    public class VizualizationViewModel
    {
        public ObservableCollection<VizualHighlightable> Elements { get; private set; }

        public VizualizationViewModel(IHighlighterService scroller )
        {
            Elements = new ObservableCollection<VizualHighlightable>( scroller.Elements
                .Where( x => (x as IVizualizableHighlightableElement) != null )
                .Select( x => new VizualHighlightable( (IVizualizableHighlightableElement)x ) )
                .ToList() );
            
            scroller.BeginHighlight += (o, e) => 
            {
                var v = Elements.FirstOrDefault( x => x.Element == e.Root );

                if( v != null )
                    v.IsHighlighted = true;
            };

            scroller.EndHighlight += ( o, e ) =>
            {
                var v = Elements.FirstOrDefault( x => x.Element == e.Root );

                if(v != null)
                    v.IsHighlighted = false;
            };

            scroller.ElementRegisteredOrUnregistered += ( o, e ) =>
            {
                Elements.Clear();
                foreach( var ele in scroller.Elements
                    .Where( x => (x as IVizualizableHighlightableElement) != null )
                    .Select( x => new VizualHighlightable( (IVizualizableHighlightableElement)x ) )
                    .ToList() )
                {
                    Elements.Add( ele );
                }
            };
        }
    }
}
