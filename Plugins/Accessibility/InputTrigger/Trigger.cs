#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\InputTrigger\Trigger.cs) is part of CiviKey. 
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
using CommonServices;

namespace InputTrigger
{
    [Serializable]
    public class Trigger : ITrigger
    {
        public static readonly ITrigger Default = new Trigger( 122 , TriggerDevice.Keyboard);

        public Trigger(int keyCode, TriggerDevice source) 
        {
            KeyCode = keyCode;
            Source = source;
        }

        #region ITrigger Members

        public int KeyCode { get; set; }

        public TriggerDevice Source { get; set; }

        #endregion
    }
}
