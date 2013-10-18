using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    internal class TokenCollection : ITokenCollection
    {
        List<IToken> _token;

        internal void Add( string token )
        {
            InsertAt( _token.Count, token );
        }

        internal void AddRange( string[] tokens, bool raiseCollectionChange )
        {
            _token.AddRange( tokens.Select( e => new Token( e ) ) );
            if( CollectionChanged != null && raiseCollectionChange )
                CollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, _token ) );
        }

        internal void AddRange( string[] tokens )
        {
            AddRange( tokens, true );
        }

        internal void InsertAt( int insertionPoint, string token )
        {
            var ts = new Token( token );

            if( insertionPoint == _token.Count ) _token.Add( ts );
            else _token.Insert( insertionPoint, ts );

            if( CollectionChanged != null )
                CollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, ts ) );
        }

        public TokenCollection( params string[] tokens )
        {
            if( CollectionChanged != null )
                CollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );

            _token = new List<IToken>( tokens.Select( t => new Token( t ) ) );

            if( CollectionChanged != null )
                CollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, _token ) );
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #region ICKReadOnlyList<IToken> Members

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

        #region ICKReadOnlyCollection<IToken> Members

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

        internal void Clear()
        {
            Clear( true );
        }

        internal void Clear( bool raiseCollectionChange )
        {
            _token.Clear();
            if( CollectionChanged != null && raiseCollectionChange )
                CollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        }

    }

}
