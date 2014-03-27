using System.Collections.Generic;
using System.Windows.Threading;
using CK.Core;
using HighlightModel;
using CK.Plugin.Config;
using System.Diagnostics;
using System.Timers;

namespace KeyScroller
{
    public class OneByOneWalker : Walker
    {
        public override bool UpToParent()
        {
            base.UpToParent();
            return MoveNext();
        }
    }

    /// <summary>
    /// Scrolling on each key one after the other, without taking zones into account
    /// </summary>
    [StrategyAttribute( OneByOneScrollingStrategy.StrategyName )]
    public class OneByOneScrollingStrategy : ScrollingStrategy
    {
        const string StrategyName = "OneByOneScrollingStrategy";
        public override string Name
        {
            get { return StrategyName; }
        }

        public OneByOneScrollingStrategy() : base()
        {
            Johnnie = new OneByOneWalker();
        }

        protected override void ProcessSkipBehavior()
        {
            switch( Johnnie.Current.Skip )
            {
                case SkippingBehavior.Skip:
                    MoveNext( ActionType.MoveNext );
                    break;
                default:
                    if( Johnnie.Current.Children.Count > 0 )
                    {
                        MoveNext( ActionType.EnterChild );
                    }
                    break;
            }
        }

        public override void OnExternalEvent()
        {
            if( Johnnie.Current != null )
            {
                FireSelectElement();
            }
        }
    }
}
