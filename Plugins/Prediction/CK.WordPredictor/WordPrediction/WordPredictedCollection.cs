using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    internal class WordPredictedCollection : IWordPredictedCollection
    {
        ObservableCollection<IWordPredicted> _list;

        public WordPredictedCollection( ObservableCollection<IWordPredicted> collection )
        {
            if( collection == null ) throw new ArgumentNullException( "collection" );
            _list = collection;
            _list.CollectionChanged += OnCollectionChanged;
        }

        protected virtual void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if( CollectionChanged != null )
            {
                CollectionChanged( this, e );
            }
        }

        #region IReadOnlyList<IWordPredicted> Members

        public int IndexOf( object item )
        {
            return _list.IndexOf( (IWordPredicted)item );
        }

        public IWordPredicted this[int index]
        {
            get { return _list[index]; }
        }

        #endregion

        #region IReadOnlyCollection<IWordPredicted> Members

        public bool Contains( object item )
        {
            return _list.Contains( item );
        }

        public int Count
        {
            get { return _list.Count; }
        }

        #endregion

        #region IEnumerable<IWordPredicted> Members

        public IEnumerator<IWordPredicted> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion
    }

}
