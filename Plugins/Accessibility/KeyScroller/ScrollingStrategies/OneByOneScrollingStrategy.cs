using System.Collections.Generic;
using System.Windows.Threading;
using CK.Core;
using HighlightModel;
using CK.Plugin.Config;
using System.Diagnostics;
using System.Timers;
using System.Linq;
namespace Scroller
{
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

        protected override void ProcessSkipBehavior(ActionType action)
        { 
            switch( Johnnie.Current.Skip )
            {
                case SkippingBehavior.Skip:
                    MoveNext( ActionType.MoveNext );
                    break;
                default:

                    if( Johnnie.Current.Children.Count > 0 && !Johnnie.Current.IsHighlightableTreeRoot || Johnnie.Current.Skip == SkippingBehavior.EnterChildren || Johnnie.Sibblings.Count( s => s.Skip != SkippingBehavior.Skip ) == 1 && Johnnie.Current.Children.Count > 0 )
                    {
                        if( action != ActionType.UpToParent )
                            MoveNext( ActionType.EnterChild );
                        else
                            MoveNext( ActionType.MoveNext );
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
