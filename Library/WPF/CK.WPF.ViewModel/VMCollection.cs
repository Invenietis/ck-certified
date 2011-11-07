using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.ComponentModel;
using CK.Core;

namespace CK.WPF.ViewModel
{
    public class VMCollection<VM,M> : INotifyCollectionChanged, INotifyPropertyChanged, ICollection<VM>
    {
        IEnumerable<M> _model;
        Converter<M,VM> _converter;
        EnumerableConverter<VM,M> _enumAdapter;

        public event PropertyChangedEventHandler  PropertyChanged;
        public event NotifyCollectionChangedEventHandler  CollectionChanged;

        public VMCollection( IEnumerable<M> model, Converter<M, VM> converter )
        {
            _model = model;
            _converter = converter;
            _enumAdapter = new EnumerableConverter<VM, M>( _model, _converter );
        }

        public IEnumerator<VM> GetEnumerator()
        {
            return _enumAdapter.GetEnumerator();
        }

        public int Count
        {
            get { return _model.Count(); }
        }

        public void Refresh()
        {
            if( CollectionChanged != null ) CollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( "Count" ) );
        }

        #region ICollection impl

        public void Add( VM item )
        {
            throw new ArgumentException();
        }

        public void Clear()
        {
            throw new ArgumentException();
        }

        public bool Contains( VM item )
        {
            return _model.Any( ( m ) => _converter( m ).Equals(item));
        }

        public void CopyTo( VM[] array, int arrayIndex )
        {
            throw new ArgumentException();
        }

        public bool IsReadOnly
        {
            get { throw new ArgumentException(); }
        }

        public bool Remove( VM item )
        {
            throw new ArgumentException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
