#region LGPL License
/*----------------------------------------------------------------------------
* This file (Setup\ContribSetup\CK-Contrib\Plugins\ServiceProvider\IDummyService.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using CK.Plugin;

namespace DummyPlugins
{
    /// <summary>
    /// A service MUST implement IDynamicService, this interface has no methods nor properties, it is used as a flag that descrbies this interface as a service
    /// </summary>
    public interface IDummyService : IDynamicService
    {
        int GetAnswerToLife();
    }
}
