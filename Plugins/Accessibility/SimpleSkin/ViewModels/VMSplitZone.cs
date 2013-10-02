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
        public VMSplitZone( VMZoneSimple vms, IEnumerable<IHighlightableElement> part1, IEnumerable<IHighlightableElement> part2 )
            : this( vms, part1 )
        {
            Next = new VMSplitZone( vms, part2 );
        }

        private VMSplitZone( VMZoneSimple vms, IEnumerable<IHighlightableElement> part1 )
            : base( vms.Context )
        {
            foreach( var v in part1 )
            {
                var e = v as VMKeySimple;
                if( e != null ) _keys.Add( e );
            }

            Original = vms;
            Next = null;
        }

        public VMZoneSimple Original { get; set; }

        public VMSplitZone Next { get; set; }
    }
}
