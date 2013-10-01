using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WPF.ViewModel;
using HighlightModel;

namespace SimpleSkin.ViewModels
{
    public class VMSplitZone : VMZoneSimple
    {
        public VMSplitZone( VMContextSimple ctx, IEnumerable<IHighlightableElement> part1, IEnumerable<IHighlightableElement> part2 )
            : this( ctx, part1 )
        {
            Next = new VMSplitZone( ctx, part2 );
        }

        private VMSplitZone( VMContextSimple ctx, IEnumerable<IHighlightableElement> part1 )
            : base( ctx )
        {
            foreach( var v in part1 )
            {
                var e = v as VMKeySimple;
                if( e != null ) _keys.Add( e );
            }

            Next = null;
        }

        public VMSplitZone Next { get; set; }
    }
}
