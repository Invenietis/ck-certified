#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WordPredictor.Model\IWordPredictedService.cs) is part of CiviKey. 
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
    /// <summary>
    /// A pub/sub service for successful word prediction. 
    /// A plugin that need to know if a word has been successfully predicted by a prediction engine musts subscribe to <see cref="WordPredictionSuccessful"/>.
    /// Conversely, a plugin can warn the world that a word has been successfully predicted by a prediction engine by calling <see cref="WordHasBeenChosen"/>.
    /// </summary>
    public interface IWordPredictedService : IDynamicService
    {
        /// <summary>
        /// This event is raised when a word has been chosen by the user. 
        /// We therefore consider that the prediction is successful.
        /// </summary>
        event EventHandler<WordPredictionSuccessfulEventArgs> WordPredictionSuccessful;

        /// <summary>
        /// Calls this whenever a predicted word has been chosen by the user.
        /// </summary>
        /// <param name="word">The word that was predicted by a prediction engine.</param>
        void WordHasBeenChosen( string word );
    }
}
