#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor\WordPrediction\WordPredictedCollection.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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

        #region ICKReadOnlyList<IWordPredicted> Members

        public int IndexOf( object item )
        {
            return _list.IndexOf( (IWordPredicted)item );
        }

        public IWordPredicted this[int index]
        {
            get { return _list.Count > index ? _list[index] : null; }
        }

        #endregion

        #region ICKReadOnlyCollection<IWordPredicted> Members

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
