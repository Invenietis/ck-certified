#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WordPredictor.Model\IPredictionTextAreaService.cs) is part of CiviKey. 
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
using CK.Plugin;

namespace CK.WordPredictor.Model
{
    public interface IPredictionTextAreaService : IDynamicService
    {
        /// <summary>
        /// This event is raised whenever the text area content changed
        /// </summary>
        event EventHandler<PredictionAreaContentEventArgs> PredictionAreaContentChanged;

        event EventHandler<IsDrivenChangedEventArgs> IsDrivenChanged;

        /// <summary>
        /// This event is raised when the <see cref="ITextualContextService"/> has been sent by the service.
        /// </summary>
        event EventHandler<PredictionAreaContentEventArgs> PredictionAreaTextSent;

        /// <summary>
        /// Gets or sets the fact that this service's implementations handle the caretIndex and prediction context
        /// This value should ONLY be set by the plugin that holds the textarea.
        /// Others should NOT set this value.
        /// </summary>
        bool IsDriven { get; set; }
      
        /// <summary>
        /// Raises the <see cref="TextualContextSent"/> event
        /// </summary>
        void SendText();

        /// <summary>
        /// Raises the <see cref="PredictionAreaContentChanged" /> event if any of the properties changes
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caretIndex"></param>
        void ChangePredictionAreaContent( string text, int caretIndex );
    }
}
