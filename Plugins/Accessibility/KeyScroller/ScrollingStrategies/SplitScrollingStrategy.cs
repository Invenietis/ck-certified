using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin.Config;
using HighlightModel;
using SimpleSkin.ViewModels;

namespace KeyScroller
{
    [Strategy( SplitScrollingStrategy.StrategyName )]
    internal class SplitScrollingStrategy : BasicScrollingStrategy
    {
        const string StrategyName = "SplitScrollingStrategy";
        const int childrenLimitBeforeSplit = 6;
        public SplitScrollingStrategy( DispatcherTimer timer, List<IHighlightableElement> elements, IPluginConfigAccessor configuration )
            : base( timer, elements, configuration )
        {
        }

        public override string Name
        {
            get { return StrategyName; }
        }

        /// <summary>
        /// This implementation of "RegisterTree" splits VMZone in two part when the children limits are reached
        /// </summary>
        /// <param name="element"></param>
        public override void RegisterTree( IHighlightableElement element )
        {
            if( !_elements.Contains( element ) )
            {
                IList<IHighlightableElement> highlightable = new List<IHighlightableElement>();
                foreach( var e in element.Children )
                {
                    VMZoneSimple vmz = e as VMZoneSimple;
                    if( vmz != null )
                    {
                        // e is a VMZone element
                        if( (e.Children.Count - 1) > childrenLimitBeforeSplit )
                        {
                            highlightable.Add( new VMSplitZone( vmz.Context, vmz.Children.Skip( 0 ).Take( e.Children.Count / 2 ) ) );
                            highlightable.Add( new VMSplitZone( vmz.Context, vmz.Children.Skip( (e.Children.Count / 2) ) ) );
                        }
                        else
                        {
                            // add the VMZone from scratch
                            highlightable.Add( vmz );
                        }
                    }
                    else
                    {
                        highlightable.Add( e );
                    }
                }
                _elements.Add( new VMSRootTemp( highlightable ) );
                Start();
            }
        }
    }

    public class VMSRootTemp : IHighlightableElement
    {
        IEnumerable<IHighlightableElement> _elements;

        public VMSRootTemp( IEnumerable<IHighlightableElement> children )
        {
            _elements = children;
        }

        #region IHighlightableElement Members

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return _elements.ToReadOnlyList(); }
        }

        public int X
        {
            get { return _elements.Min( k => k.X ); }
        }

        public int Y
        {
            get { return _elements.Min( k => k.Y ); }
        }

        public int Width
        {
            get { return _elements.Max( k => k.X + k.Width ) - X; }
        }

        public int Height
        {
            get { return _elements.Max( k => k.Y + k.Height ) - Y; }
        }

        public SkippingBehavior Skip
        {
            get
            {
                if( _elements.Count() > 0 && _elements.Any( z => z.Skip != SkippingBehavior.Skip ) )
                    return SkippingBehavior.EnterChildren; //We enter only if there are zones that need to be scrolled through.
                return SkippingBehavior.None;
            }
        }

        #endregion
    }
}
