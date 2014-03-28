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
        public OneByOneWalker( IHighlightableElement root ) : base( root ) { }

        public override bool UpToParent()
        {
            base.UpToParent();
            if(MoveNext()) return true;

            GoToAbsoluteRoot();
            return false;
        }
    }

    /// <summary>
    /// A ScrollingStrategy that scroll only on sheets elements.
    /// </summary>
    [StrategyAttribute( OneByOneScrollingStrategy.StrategyName )]
    public class OneByOneScrollingStrategy : ScrollingStrategyBase
    {
        const string StrategyName = "OneByOneScrollingStrategy";
        public override string Name
        {
            get { return StrategyName; }
        }

        public OneByOneScrollingStrategy() : base()
        {
            Johnnie = new OneByOneWalker(this);
        }

        protected override void ProcessSkipBehavior()
        {
            switch( Johnnie.Current.Skip )
            {
                case SkippingBehavior.Skip:
                    MoveNext( ActionType.MoveNext );
                    break;
                default:
                    if( Johnnie.Current.Children.Count > 0 && !Johnnie.Current.IsHighlightableTreeRoot || Johnnie.Current.Skip == SkippingBehavior.EnterChildren )
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
