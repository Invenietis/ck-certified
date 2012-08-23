#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\Helpers\ScreenHelper.cs) is part of CiviKey. 
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

using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Interop;
using System;

namespace SimpleSkin.Helper
{
    public class ScreenHelper
    {
        public static bool IsInScreen( Rectangle rect )
        {
            return Screen.AllScreens.Any( ( s ) => s.WorkingArea.Contains( rect ) );
        }

        public static bool IsInScreen( Point point )
        {
            return Screen.AllScreens.Any( ( s ) => s.WorkingArea.Contains( point ) );
        }

        public static Point GetCenterOfParentScreen( Rectangle rect )
        {
            Screen parent = Screen.FromRectangle( rect );
            return new Point( parent.WorkingArea.Width / 2, parent.WorkingArea.Height / 2 );
        }

        public static Rectangle GetPrimaryScreenSize()
        {
            return Screen.PrimaryScreen.Bounds;
        }

    }
}
