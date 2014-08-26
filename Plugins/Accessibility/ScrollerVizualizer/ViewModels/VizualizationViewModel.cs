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
                IVizualizableHighlightableElement vEl = e.Element as IVizualizableHighlightableElement;
               
                if( e.HasRegistered && vEl != null)
                    Elements.Insert(0, new VizualHighlightable( vEl ) );
                else if( vEl != null )
                {
                   Elements.Remove( Elements.FirstOrDefault( x => x.Element == vEl ) );
                }
                else
                {
                    Elements.Remove( Elements.Where( x => (x.Element as ExtensibleHighlightableElementProxy) != null && (x.Element as ExtensibleHighlightableElementProxy).HighlightableElement == e.Element ).FirstOrDefault() );
                }
            };
        }
    }
}
