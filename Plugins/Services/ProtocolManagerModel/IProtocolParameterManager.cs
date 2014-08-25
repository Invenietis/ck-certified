#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\ProtocolManagerModel\IProtocolParameterManager.cs) is part of CiviKey. 
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

using System.ComponentModel;

namespace ProtocolManagerModel
{
    /// <summary>
    /// A class that implements this interface is capable of handling the string parameter of a Protocol : it can be filled from the parameter (which is a string), and can be written as a string.
    /// Implements INotifyPropertyChanged.
    /// </summary>
    public interface IProtocolParameterManager : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets a string containing the Parameter's GetCommandString return value and populates the object with it.
        /// </summary>
        /// <param name="parameter">The parameter as a string (is the result of the GetCommandString method of this object's implementation.</param>
        void FillFromString( string parameter );

        /// <summary>
        /// Returns the representation of this object's implementation's value.
        /// This value is to be processed by a corresponding CommandHandler.
        /// </summary>
        /// <returns>Returns the representation of this object's implementation's value. This value is to be processed by a corresponding CommandHandler.</returns>
        string GetParameterString();

        /// <summary>
        /// Gets whether the parameter is valid (ie: can be safely saved as is)
        /// </summary>
        bool IsValid { get; }

        IProtocolEditorRoot Root { get; set; }
    }
}
