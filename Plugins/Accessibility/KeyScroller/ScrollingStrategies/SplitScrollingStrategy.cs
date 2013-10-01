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
        const int childrenLimitBeforeSplit = 5;
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
                var highlightable = RegisterRecursive( element.Children );
                _elements.Add( new VMSTemp( highlightable ) );
                Start();
            }
        }

        IEnumerable<IHighlightableElement> RegisterRecursive( IEnumerable<IHighlightableElement> elements )
        {
            IList<IHighlightableElement> highlightable = new List<IHighlightableElement>();
            foreach( var e in elements )
            {
                IEnumerable<IHighlightableElement> childrenElements = null;
                if( e.Children.Count > 0 ) childrenElements = RegisterRecursive( e.Children );

                VMZoneSimple vmz = e as VMZoneSimple;
                if( vmz != null && childrenElements != null && ((vmz.Children.Count - 1) > childrenLimitBeforeSplit) )
                {
                    highlightable.Add( new VMSplitZone( vmz.Context, childrenElements.Skip( 0 ).Take( vmz.Children.Count / 2 ) ) );
                    highlightable.Add( new VMSplitZone( vmz.Context, childrenElements.Skip( (vmz.Children.Count / 2) ) ) );
                }
                else
                {
                    highlightable.Add( e );
                }
            }
            return highlightable;
        }
    }

    public class VMSTemp : IHighlightableElement
    {
        IEnumerable<IHighlightableElement> _elements;

        public VMSTemp( IEnumerable<IHighlightableElement> children )
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
