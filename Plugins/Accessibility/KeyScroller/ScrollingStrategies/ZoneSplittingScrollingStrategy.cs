using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin.Config;
using HighlightModel;
using SimpleSkin.ViewModels;
using System.Timers;
using System;

namespace Scroller
{
    [Strategy( HalfZoneScrollingStrategy.StrategyName )]
    internal class HalfZoneScrollingStrategy : ZoneScrollingStrategy
    {
        public static readonly int ZoneDivider = 2;
        const string StrategyName = "HalfZoneScrollingStrategy";

        public HalfZoneScrollingStrategy() : base()
        {
            if( ZoneDivider < 2 ) throw new InvalidOperationException( "The ZoneDivider can't be less than 2 !" );

            Johnnie = new ZondeDivderWalker( this );
        }

        public override string Name
        {
            get { return StrategyName; }
        }
    }

    public class ZondeDivderWalker : Walker
    {
        public ZondeDivderWalker(IHighlightableElement root) : base ( root )
        { }

        public override bool EnterChild()
        {
            if( Current.Children.Count == 0 ) return false;

            //False if the current element is not a root/relative root or if there are not enough children or if one child is an KeySimple
            if( !Current.IsHighlightableTreeRoot && Current.Children.Count > HalfZoneScrollingStrategy.ZoneDivider && Current.Children[0] as VMKeySimple != null && Peek() as VirtualZone == null )
            {
                IHighlightableElement old = Current;
                Current = new VirtualZone(Current);
                
                //if( old.IsHighlightableTreeRoot ) ((VirtualZone) Current).Skip = SkippingBehavior.Skip;

                var parent = Peek() as VirtualZone;
                if( parent != null ) parent.UpdateChild( old, Current );
            }

            return base.EnterChild();
        }

        public override bool MoveNext()
        {
            ICKReadOnlyList<IHighlightableElement> sibblings = GetSibblings();
            if( sibblings == null || sibblings.Count == 1 ) //false if there is no parent or there are no sibblings at all
                return false;

            int idx = sibblings.IndexOf( Current );

            //False when the element is found in its parents 
            //or when the parent is a root element and the current element is a virtualzone : 
            //Means that we may found the WrappedElement of the current VirtualZone in the sibblings list.
            if( idx < 0 && (Peek() == null || !Peek().IsHighlightableTreeRoot && Current as VirtualZone == null) ) 
                throw new InvalidOperationException( "Something goes wrong : the current element is not contained by its parent !" );
            
            if( idx >= 0 )
            {
                //The current child is the last one
                if( idx + 1 >= sibblings.Count ) return false;

                Current = sibblings.ElementAt( idx + 1 );
                return true;
            }

            //Here we get the wrapped element and restart the procedure.
            Current = ((VirtualZone) Current).WrappedElement;
            return MoveNext();
        }
    }
}
