using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows.Helpers;
using CK.WPF.ViewModel;
using CommonServices.Accessibility;
using HighlightModel;

namespace SimpleSkin
{

    public class MiniViewVM : VMBase, IHighlightableElement, IDisposable
    {
        public SimpleSkin Parent { get; set; }
        IPluginConfigAccessor Config { get { return Parent.Config; } }

        bool _isHighlighted;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set { _isHighlighted = value; OnPropertyChanged( "IsHighlighted" ); }
        }

        public MiniViewVM( SimpleSkin parent )
        {
            _isHighlighted = false;
            Parent = parent;
        }

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return CKReadOnlyListEmpty<IHighlightableElement>.Empty; }
        }

        public int X
        {
            get
            {
                var position = Config.Context["MiniViewPositionX"];
                if( position == null )
                {
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
                    System.Drawing.Point p = ScreenHelper.GetCenterOfParentScreen( rect );

                    return p.X;
                }
                else
                    return ( Int32.Parse( position.ToString() ) );
            }
            set { Config.Context["MiniViewPositionX"] = value; }
        }

        public int Y
        {
            get
            {
                var position = Config.Context["MiniViewPositionY"];
                if( position == null )
                    return 0;
                else
                    return ( Int32.Parse( position.ToString() ) );
            }
            set { Config.Context["MiniViewPositionY"] = value; }
        }

        int _width = 160;
        public int Width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged( "Width" ); }
        }

        int _height = 160;
        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged( "Height" );
            }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.None; }
        }

        public void Dispose()
        {
        }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            if( Parent.IsViewHidden )
            {
                IsHighlighted = true;
            }

            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            if( Parent.IsViewHidden )
            {
                IsHighlighted = false;
            }

            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            if( Parent.IsViewHidden )
            {
                Parent.RestoreSkin();
            }

            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return true; }
        }
    }
}
