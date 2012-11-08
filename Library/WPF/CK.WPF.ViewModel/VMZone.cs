#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.ViewModel\VMZone.cs) is part of CiviKey. 
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

using System.Collections.ObjectModel;
using CK.Keyboard.Model;
using CK.Core;

namespace CK.WPF.ViewModel
{
    public abstract class VMZone<TC, TB, TZ, TK> : VMContextElement<TC, TB, TZ, TK>
        where TC : VMContext<TC, TB, TZ, TK>
        where TB : VMKeyboard<TC, TB, TZ, TK>
        where TZ : VMZone<TC, TB, TZ, TK>
        where TK : VMKey<TC, TB, TZ, TK>
    {
        IZone _zone;
        ObservableSortedArrayKeyList<TK,int> _keys;

        public ObservableSortedArrayKeyList<TK, int> Keys { get { return _keys; } }

        public string Name { get { return _zone.Name; } }

        public VMZone( TC context, IZone zone )
            : base( context, zone )
        {
            _zone = zone;
            _keys = new ObservableSortedArrayKeyList<TK, int>( k => k.Index );

            foreach( IKey key in _zone.Keys )
            {
                TK k = Context.Obtain( key );
                Keys.Add( k );
            }
        }



        protected override void OnDispose()
        {
            foreach( TK key in Keys )
            {
                key.Dispose();
            }
        }
    }
}
