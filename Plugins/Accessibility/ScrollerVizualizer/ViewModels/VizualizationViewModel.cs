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
        bool _verticalOrientation;

        public ObservableCollection<VizualHighlightable> Elements { get; private set; }

        public bool IsScrollerActive
        {
            get { return _isScrollerActive; }
            set
            {
                _isScrollerActive = value;
                FirePropertyChanged( "IsScrollerActive" );
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

        public bool VerticalOrientation
        {
            get { return _verticalOrientation; }
            set
            {
                _verticalOrientation = value;
                FirePropertyChanged( "VerticalOrientation" );
            }
        }

        public VizualizationViewModel(IHighlighterService scroller )
        {
            VerticalOrientation = true;
            Elements = new ObservableCollection<VizualHighlightable>( scroller.Elements
                .Where( x => (x as IVizualizableHighlightableElement) != null )
                .Select( x => new VizualHighlightable( (IVizualizableHighlightableElement)x ) )
                .ToList() );
            IsScrollerActive = scroller.IsHighlighting;

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

            scroller.OnTrigger += ( o, e ) =>
            {
                var v = Elements.FirstOrDefault( x => x.Element == e.Root || x.Element == e.Element );
                if( v != null && scroller.IsHighlighting )
                    v.IsSelected = true;
            };
            
            scroller.BeginHighlight += (o, e) => 
            {
                var v = Elements.FirstOrDefault( x => x.Element == e.Root );

                if( v != null )
                {
                    v.IsHighlighted = true;
                    var c = v.Children.FirstOrDefault( x => x.Element == e.Element );
                    if( c != null ) c.IsHighlighted = true;
                    if( e.Element != e.Root ) v.IsSelected = true;
                    else v.IsSelected = false;
                }
                
            };

            scroller.EndHighlight += ( o, e ) =>
            {
                var v = Elements.FirstOrDefault( x => x.Element == e.Root || x.Element == e.Element );

                if( v != null )
                {
                    v.IsHighlighted = false;
                    var c = v.Children.FirstOrDefault( x => x.IsHighlighted );
                    if( c != null ) c.IsHighlighted = false;
                }

            };

            scroller.ElementRegisteredOrUnregistered += ( o, e ) =>
            {
                if(Elements.Count > 0) Elements.Clear();

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
