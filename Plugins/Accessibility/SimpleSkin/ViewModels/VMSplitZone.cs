using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.WPF.ViewModel;
using HighlightModel;

namespace SimpleSkin.ViewModels
{
    /// <summary>
    /// This class is a part of VMZoneSimple which is created by the register strategy of SplitScrollingStrategy
    /// </summary>
    public class VMSplitZone : VMZoneSimple
    {
        public VMSplitZone( VMContextSimple ctx, IEnumerable<IHighlightableElement> vmks )
            : base( ctx )
        {
            foreach( var v in vmks )
            {
                VMKeySimple vmk = v as VMKeySimple;
                if( vmk != null ) _keys.Add( vmk );
            }
        }
    }
}
