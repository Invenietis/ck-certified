#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.ViewModel\VMCollection.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CK.Core;

namespace CK.WPF.ViewModel
{
    public class VMCollection<VM,M> : INotifyCollectionChanged, INotifyPropertyChanged, ICollection<VM>
    {
        IEnumerable<M> _model;
        Converter<M,VM> _converter;
        CKEnumerableConverter<VM, M> _enumAdapter;

        public event PropertyChangedEventHandler  PropertyChanged;
        public event NotifyCollectionChangedEventHandler  CollectionChanged;

        public VMCollection( IEnumerable<M> model, Converter<M, VM> converter )
        {
            _model = model;
            _converter = converter;
            _enumAdapter = new CKEnumerableConverter<VM, M>( _model, _converter );
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
