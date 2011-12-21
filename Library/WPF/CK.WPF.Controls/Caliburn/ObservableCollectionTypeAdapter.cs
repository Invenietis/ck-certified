using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using CK.Core;

namespace CK.WPF.Controls
{
    public class ObservableCollectionTypeAdapter<TOuter,TInner> : ReadOnlyListTypeAdapter<TOuter,TInner>, IObservableCollection<TOuter>
        where TInner : TOuter
    {
        public ObservableCollectionTypeAdapter( IObservableCollection<TInner> inner )
            : base( inner )
        {
            inner.PropertyChanged += ( o, e ) =>
                {
                    var h = PropertyChanged;
                    if( h != null ) h( this, e );
                };
            inner.CollectionChanged += ( o, e ) =>
                {
                    var h = CollectionChanged;
                    if( h != null ) h( this, e );
                };
        }

        public new IObservableCollection<TInner> Inner { get { return (IObservableCollection<TInner>)base.Inner; } }

        public void AddRange( IEnumerable<TOuter> items )
        {
            Inner.AddRange( items.Cast<TInner>() );
        }

        public void RemoveRange( IEnumerable<TOuter> items )
        {
            Inner.RemoveRange( items.Cast<TInner>() );
        }

        public int IndexOf( TOuter item )
        {
            return base.IndexOf( item );
        }

        public void Insert( int index, TOuter item )
        {
            Inner.Insert( index, (TInner)item );
        }

        public void RemoveAt( int index )
        {
            Inner.RemoveAt( index );
        }

        public new TOuter this[int index]
        {
            get { return Inner[index]; }
            set { Inner[index] = (TInner)value; }
        }

        public void Add( TOuter item )
        {
            Inner.Add( (TInner)item );
        }

        public void Clear()
        {
            Inner.Clear();
        }

        public bool Contains( TOuter item )
        {
            return base.Contains( item );
        }

        public void CopyTo( TOuter[] array, int arrayIndex )
        {
            for( int i = 0; i < Inner.Count; ++i ) 
            {
                array[i+arrayIndex] = Inner[i];
            }
        }

        public bool IsReadOnly
        {
            get { return Inner.IsReadOnly; }
        }

        public bool Remove( TOuter item )
        {
            return Inner.Remove( (TInner)item );
        }

        public bool IsNotifying
        {
            get { return Inner.IsNotifying; }
            set { Inner.IsNotifying = value; }
        }

        public void NotifyOfPropertyChange( string propertyName )
        {
            Inner.NotifyOfPropertyChange( propertyName );
        }

        public void Refresh()
        {
            Inner.Refresh();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event NotifyCollectionChangedEventHandler  CollectionChanged;
    
    }

}
