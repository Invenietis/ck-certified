#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WindowManager.Model\WindowManagerExtensions.cs) is part of CiviKey. 
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

using CK.Windows;

namespace CK.WindowManager.Model
{
    public static class WindowManagerExtensions
    {

        public static void RegisterWindow( this IWindowManager m, string name, CKWindow window )
        {
            m.Register( new WindowElement( window, name ) );
        }

        public static void UnregisterWindow( this IWindowManager m, string name )
        {
            IWindowElement el = m.GetByName( name );
            if( el != null ) m.Unregister( el );
        }
    }
}
