#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\ViewModels\VMContextElementSimple.cs) is part of CiviKey. 
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
using CK.WPF.ViewModel;
using CK.Windows;
using System.Windows.Threading;
using System.Diagnostics;

namespace SimpleSkin.ViewModels
{
    public abstract class VMContextElement : VMBase
    {
        VMContextSimpleBase _context;

        public VMContextElement( VMContextSimpleBase context )
        {
            _context = context;
        }

        /// <summary>
        /// Gets the <see cref="VMContext"/> to which this element belongs.
        /// </summary>
        public VMContextSimpleBase Context { get { return _context; } }

        internal abstract void Dispose();

        internal void SafeSet<T>( T value, Action<T> setter, bool synchronous = true )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == _context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            T val = value;
            if( synchronous )
                _context.NoFocusManager.NoFocusDispatcher.Invoke( setter, val );
            else
                _context.NoFocusManager.NoFocusDispatcher.BeginInvoke( setter, val );
        }
    }
}
