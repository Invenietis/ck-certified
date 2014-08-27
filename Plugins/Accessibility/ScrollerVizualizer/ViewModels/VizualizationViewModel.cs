using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using HighlightModel;
using CommonServices.Accessibility;
using SimpleSkin.ViewModels;
using System.ComponentModel;
using ScrollerVizualizer.Resources;

namespace ScrollerVizualizer
{
    public class VizualizationViewModel : INotifyPropertyChanged
    {
        bool _isScrollerActive;
        string _key;

        public ObservableCollection<VizualHighlightable> Elements { get; private set; }

        public bool IsScrollerActive
        {
            get { return _isScrollerActive; }
            set
            {
                _isScrollerActive = value;
                FirePropertyChanged( "IsScrollerACtive" );
            }
        }

        public string Key
        {
            get { return R.Trigger + _key; }
            set
            {
                _key = value;
                FirePropertyChanged( "Key" );
            }
        }

        public VizualizationViewModel(IHighlighterService scroller )
        {
            Elements = new ObservableCollection<VizualHighlightable>( scroller.Elements
                .Where( x => (x as IVizualizableHighlightableElement) != null )
                .Select( x => new VizualHighlightable( (IVizualizableHighlightableElement)x ) )
                .ToList() );
            if( scroller.Trigger != null )
            {
                Key = scroller.Trigger.DisplayName;
            }
            scroller.HighliterStatusChanged += (o, e) =>
            {
                IsScrollerActive = scroller.IsHighlighting;
            };

            scroller.TriggerChanged += (o, e) =>
            {
                Key = scroller.Trigger.DisplayName;
            };

            scroller.BeginHighlight += (o, e) => 
            {
                var v = Elements.FirstOrDefault( x => x.Element == e.Root );

                if( v != null )
                    v.IsHighlighted = true;
            };

            scroller.EndHighlight += ( o, e ) =>
            {
                var v = Elements.FirstOrDefault( x => x.Element == e.Root );

                if(v != null)
                    v.IsHighlighted = false;

                IsScrollerActive = scroller.IsHighlighting;
            };

            scroller.ElementRegisteredOrUnregistered += ( o, e ) =>
            {
                Elements.Clear();
                foreach( var ele in scroller.Elements
                    .Where( x => (x as IVizualizableHighlightableElement) != null )
                    .Select( x => new VizualHighlightable( (IVizualizableHighlightableElement)x ) )
                    .ToList() )
                {
                    Elements.Add( ele );
                }
            };
        }

        void FirePropertyChanged( string propertyName )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
