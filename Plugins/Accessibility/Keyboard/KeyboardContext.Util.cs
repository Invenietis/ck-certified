#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\Keyboard\KeyboardContext.Util.cs) is part of CiviKey. 
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
using System.Text.RegularExpressions;

namespace CK.Keyboard
{
	public partial class KeyboardContext
	{
        /// <summary>
        /// Computes a unique name (suffixed with '(n)' where n is a number) given 
        /// a function that check existence of proposed names.
        /// It relies on <see cref="R.KeyboardAutoNumPattern"/> and <see cref="R.KeyboardAutoNumRegex"/> resources
        /// to offer culture dependant naming.
        /// </summary>
        /// <param name="newName">Proposed name.</param>
        /// <param name="currentName">Current name (null if none).</param>
        /// <param name="exists">Function that check the existence.</param>
        /// <returns>A unique name based on proposed name.</returns>
        internal static string EnsureUnique( string newName, string currentName, Predicate<string> exists )
        {
            string nCleaned = Regex.Replace( newName, R.KeyboardAutoNumRegex, String.Empty );
            string n = nCleaned;
            if ( n != currentName )
            {
                int autoNum = 1;
                while ( n != currentName && exists( n ) )
                {
                    n = String.Format( R.KeyboardAutoNumPattern, nCleaned, autoNum++ );
                }
            }
            return n;
        }
	}
}
