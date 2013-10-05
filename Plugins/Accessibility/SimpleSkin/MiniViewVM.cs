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
            if( Parent.Highlighter.Status == InternalRunningStatus.Started )
            {
                Parent.Highlighter.Service.SelectElement += OnSelectElement;
                Parent.Highlighter.Service.BeginHighlight += OnBeginHighlight;
                Parent.Highlighter.Service.EndHighlight += OnEndHighlight;
            }
            Parent.Highlighter.ServiceStatusChanged += OnHighlighterServiceStatusChanged;
        }

        void OnBeginHighlight( object sender, HighlightEventArgs e )
        {
            if( Parent.IsViewHidden && e.Element == this )
            {
                IsHighlighted = true;
            }
        }

        void OnEndHighlight( object sender, HighlightEventArgs e )
        {
            if( Parent.IsViewHidden && e.Element == this )
            {
                IsHighlighted = false;
            }
        }

        void OnSelectElement( object sender, HighlightEventArgs e )
        {
            if( Parent.IsViewHidden && e.Element == this )
            {
                Parent.RestoreSkin();
            }
        }

        void OnHighlighterServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                Parent.Highlighter.Service.BeginHighlight += OnBeginHighlight;
                Parent.Highlighter.Service.EndHighlight += OnEndHighlight;
                Parent.Highlighter.Service.SelectElement += OnSelectElement;
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                Parent.Highlighter.Service.BeginHighlight -= OnBeginHighlight;
                Parent.Highlighter.Service.EndHighlight -= OnEndHighlight;
                Parent.Highlighter.Service.SelectElement -= OnSelectElement;
            }
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
                    return (Int32.Parse( position.ToString() ));
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
                    return (Int32.Parse( position.ToString() ));

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
            Parent.Highlighter.ServiceStatusChanged -= OnHighlighterServiceStatusChanged;
            if( Parent.Highlighter.Status == InternalRunningStatus.Started )
            {
                Parent.Highlighter.Service.SelectElement -= OnSelectElement;
                Parent.Highlighter.Service.BeginHighlight -= OnBeginHighlight;
                Parent.Highlighter.Service.EndHighlight -= OnEndHighlight;
            }
        }


    }
}
