#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.ViewModel\VMContextElement.cs) is part of CiviKey. 
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

using System.Collections.Generic;
using System.Linq;
namespace CK.WPF.ViewModel
{
    /// <summary>
    /// </summary>
    public abstract class VMContextElement<TC, TB, TZ, TK> : VMBase
        where TC : VMContext<TC, TB, TZ, TK>
        where TB : VMKeyboard<TC, TB, TZ, TK>
        where TZ : VMZone<TC, TB, TZ, TK>
        where TK : VMKey<TC, TB, TZ, TK>
    {
        TC _context;

        protected VMContextElement( TC context )
        {
            _context = context;
        }


        private IEnumerable<VMContextElement<TC, TB, TZ, TK>> GetParents()
        {
            VMContextElement<TC, TB, TZ, TK> elem = this;
            while( elem != null )
            {
                elem = elem.GetParent();

                if( elem != null )
                    yield return elem;
            }
        }


        public IEnumerable<VMContextElement<TC, TB, TZ, TK>> Parents
        {
            get
            {
                return GetParents().Reverse();
            }
        }


        /// <summary>
        /// Gets the parent of the object.
        /// Null for the VMContext
        /// </summary>
        public abstract VMContextElement<TC, TB, TZ, TK> GetParent();

        /// <summary>
        /// Gets the <see cref="VMContext"/> to which this element belongs.
        /// </summary>
        public TC Context { get { return _context; } }

        /// <summary>
        /// Internal method called by this <see cref="Context"/> only.
        /// </summary>
        internal void Dispose()
        {
            OnDispose();
        }

        protected virtual void OnDispose()
        {
        }

    }
}