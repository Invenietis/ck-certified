using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonServices.Accessibility;
using HighlightModel;

namespace MouseRadar
{
    public class StateScheduler
    {
        IHighlighterService _highliter;

        public RootElement Root { get; private set; }
        State _last;
        StateContext _context;

        bool _firstAppend = true;

        public StateScheduler()
        {
            Root = new RootElement();
            _context = new StateContext( Root );
        }


        public void ValidateChildren( IHighlighterService highliter )
        {
            _highliter = highliter;

            _highliter.RegisterTree( Root );
            _highliter.BeginHighlight += ( o, e ) =>
            {
                if( !(e.Element is State) ) return;
                State s = (State)e.Element;

                Console.WriteLine( "higlight " + s.Current );
            };
            _highliter.EndHighlight += ( o, e ) =>
            {
                if( !(e.Element is State) ) return;
                State s = (State)e.Element;
                Console.WriteLine( "end higlight " + s.Current );
            };
            _highliter.SelectElement += ( o, e ) =>
            {
                if( !(e.Element is State) ) return;
                State s = (State)e.Element;
                if( s != null )
                {
                    s.Action( _context );
                    Console.WriteLine( "Action " + s.Current );
                }
            };
        }
    }
}
