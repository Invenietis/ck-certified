#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WindowManager.Model\WindowElementEventArgs.cs) is part of CiviKey. 
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

namespace CK.WindowManager.Model
{
    public class WindowElementEventArgs : EventArgs
    {
        public IWindowElement Window { get; private set; }

        public WindowElementEventArgs( IWindowElement window )
        {
            Window = window;
        }
    }

    public class WindowElementLocationEventArgs : WindowElementEventArgs
    {
        public double DeltaTop { get; private set; }

        public double DeltaLeft { get; private set; }

        public WindowElementLocationEventArgs( IWindowElement window, double deltaTop, double deltaLeft )
            : base( window )
        {
            DeltaTop = deltaTop;
            DeltaLeft = deltaLeft;
        }
    }

    public class WindowElementResizeEventArgs : WindowElementEventArgs
    {
        public double DeltaWidth { get; private set; }

        public double DeltaHeight { get; private set; }

        public WindowElementResizeEventArgs( IWindowElement window, double deltaWidth, double deltaHeight )
            : base( window )
        {
            DeltaWidth = deltaWidth;
            DeltaHeight = deltaHeight;
        }
    }

}
