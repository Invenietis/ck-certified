#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\CommonServices\Accessibility\ITextTemplateService.cs) is part of CiviKey. 
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

namespace CommonServices
{
    public interface ITextTemplateService : IDynamicService
    {
        /// <summary>
        /// The placeholder opent tag
        /// </summary>
        string OpentTag { get; }

        /// <summary>
        /// The placeholder close tag
        /// </summary>
        string CloseTag { get; }

        /// <summary>
        /// Open the window editor that allow the user to set
        /// the textbox fields
        /// </summary>
        void OpenEditor( string template );

        /// <summary>
        /// Send the result to the output
        /// </summary>
        void SendFormatedTemplate();
    }
}
