using System.Collections.Generic;
using HighlightModel;

namespace SimpleSkin.ViewModels
{
    public class VMSplitZone : VMZoneSimple
    {
        public VMSplitZone( VMZoneSimple vms, IEnumerable<IHighlightableElement> part1, IEnumerable<IHighlightableElement> part2 )
            : this( vms, part1 )
        {
            Next = new VMSplitZone( vms, part2, this );
        }

        private VMSplitZone( VMZoneSimple vms, IEnumerable<IHighlightableElement> part1, VMSplitZone parent )
            : this( vms, part1 )
        {
            Parent = parent;
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
            Parent = null;
        }

        public VMZoneSimple Original { get; private set; }

        public VMSplitZone Next { get; private set; }

        public VMSplitZone Parent { get; private set; }
    }
}
