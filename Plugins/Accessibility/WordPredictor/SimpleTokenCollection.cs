using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WordPredictor.Model;

namespace WordPredictor
{
    internal class SimpleTokenCollection : ITokenCollection
    {
        IList<IToken> _token;

        public void Add( string token )
        {
            var ts = new SimpleToken( token );
            _token.Add( ts );
            if( CollectionChanged != null )
                CollectionChanged( this, new System.Collections.Specialized.NotifyCollectionChangedEventArgs( System.Collections.Specialized.NotifyCollectionChangedAction.Add, ts ) );
        }

        public SimpleTokenCollection( params string[] tokens )
        {
            _token = new List<IToken>( tokens.Select( t => new SimpleToken( t ) ) );
        }

        public event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

        #region IReadOnlyList<IToken> Members

        public int IndexOf( object item )
        {
            return _token.IndexOf( (IToken)item );
        }

        public IToken this[int index]
        {
            get { return _token[index]; }
            set { _token[index] = value; }
        }

        #endregion

        #region IReadOnlyCollection<IToken> Members

        public bool Contains( object item )
        {
            return _token.Contains( item );
        }

        public int Count
        {
            get { return _token.Count; }
        }

        #endregion

        #region IEnumerable<IToken> Members

        public IEnumerator<IToken> GetEnumerator()
        {
            return _token.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _token.GetEnumerator();
        }

        #endregion
    }

}
