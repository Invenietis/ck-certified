#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\ClickSelectorService\IClickSelector.cs) is part of CiviKey. 
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
using CK.Plugins.AutoClick.ViewModel;
using CK.Plugin;

namespace CK.Plugins.AutoClick
{
    public interface IClickSelector : IDynamicService
    {
        #region Methods

        /// <summary>
        /// Triggers the choosing of a click type.
        /// This method doesn't return the Click as it may need to be asyncronous (when the PointerClickTypeSelector is enabled)
        /// </summary>
        void AskClickType();

        #endregion

        #region Events

        /// <summary>
        /// Fired when a ClickType is chosen
        /// </summary>
        event ClickChosenEventHandler ClickChosen;

        /// <summary>
        /// Fired when the "NoClick" clickType is selected
        /// </summary>
        event EventHandler StopEvent;

        /// <summary>
        /// Fired when the "NoClick" clickType is no longer selected
        /// </summary>
        event EventHandler ResumeEvent;

        #endregion
    }

    #region Delegates

    public delegate void ClickChosenEventHandler(object sender, ClickTypeEventArgs e);

    #endregion

    #region EventArgs

    /// <summary>
    /// EventArgs that contains the click chosen by the ClickTypeSelector.
    /// </summary>
    public class ClickTypeEventArgs : EventArgs
    {
        public ClickVM ClickVM { get; set; }

        public ClickTypeEventArgs( ClickVM clickVM )
        {
            ClickVM = clickVM;            
        }
    }

    #endregion
}
