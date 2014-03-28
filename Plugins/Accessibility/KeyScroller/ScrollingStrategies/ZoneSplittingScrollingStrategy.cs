using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin.Config;
using HighlightModel;
using SimpleSkin.ViewModels;
using System.Timers;
using System;

namespace KeyScroller
{
    [Strategy( ZoneDividerScrollingStrategy.StrategyName )]
    internal class ZoneDividerScrollingStrategy : ZoneScrollingStrategy
    {
        public static readonly int ZoneDivider = 2;
        const string StrategyName = "ZoneDividerScrollingStrategy";

        public ZoneDividerScrollingStrategy() : base()
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

            //False if there are not enough children or if one child is already an VirtualZone or if the current element is a Root
            if(Current.Children.Count > ZoneDividerScrollingStrategy.ZoneDivider && Current.Children[0] as VirtualZone == null )
            {
                IHighlightableElement old = Current;
                Current = new VirtualZone(Current);
                
                if( old.IsHighlightableTreeRoot ) ((VirtualZone) Current).Skip = SkippingBehavior.Skip;

                var parent = Parents.Peek() as VirtualZone;
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

            //False when the element is found in its parents or when the parent is a root element
            if( idx < 0 && (Peek() == null || !Peek().IsHighlightableTreeRoot) ) 
                throw new InvalidOperationException( "Something goes wrong : the current element is not contained by its parent !" );
            
            if( idx >= 0 )
            {
                //The current child is the last one
                if( idx + 1 >= sibblings.Count ) return false;

                Current = sibblings.ElementAt( idx + 1 );
                return true;
            }

            return UpToParent();
        }
    }
}
