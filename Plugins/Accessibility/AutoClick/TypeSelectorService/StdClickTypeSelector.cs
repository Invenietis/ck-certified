#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\StdClickTypeSelector.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugins.AutoClick.ViewModel;
using CK.Plugins.AutoClick.Model;
using CK.Plugins.AutoClick.Views;
using System.Windows.Input;
using CK.WPF.ViewModel;
using System.Diagnostics;
using System.ComponentModel;
using CK.Plugin;
using CK.Core;

namespace CK.Plugins.AutoClick
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class StdClickTypeSelector : CK.WPF.ViewModel.VMBase, IClickTypeSelector, IPlugin
    {
        const string PluginGuidString = "{F9687F04-7370-4812-9EB4-1320EB282DD8}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Click Type Selector";
        public readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginIdVersion, PluginPublicName );

        #region Variables & Properties

        //ViewModel
        public ClicksVM ClicksVM { get; set; }

        #endregion

        #region IPlugin members

        public bool Setup( IPluginSetupInfo info )
        {
            ClicksVM = new ClicksVM();
            return true;
        }

        public void Start()
        {
            ClickSelectorWindow _clickSelectorWindow = new ClickSelectorWindow() { DataContext = this };
            _clickSelectorWindow.Show();
        }

        public void Stop()
        {

        }

        public void Teardown()
        {
            ClicksVM = null;
        }

        #endregion

        #region IClickTypeSelector Members

        #region Methods

        /// <summary>
        /// Part of the IClickTypeSelector Interface, called by the AutoClickPlugin when it needs to launch a click.
        /// This method doesn't return the Click as it may need to be asyncronous (when the PointerClickTypeSelector is enabled)
        /// </summary>
        public void AskClickType()
        {
            ClickVM clickToLaunch = null;

            clickToLaunch = ClicksVM.GetNextClick( true );

            if( AutoClickClickTypeChosen != null && clickToLaunch != null )
            {
                AutoClickClickTypeChosen( this, new ClickTypeEventArgs( clickToLaunch ) );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event fired when a click has been chosen
        /// </summary>
        public event ClickTypeChosenEventHandler AutoClickClickTypeChosen;

        /// <summary>
        /// Event fired when the AutoClickPlugin should be stopped
        /// </summary>
        public event AutoClickStopEventHandler AutoClickStopEvent;

        /// <summary>
        /// Event fired when the AutoclickPlugin may go back to work
        /// </summary>
        public event AutoClickResumeEventHandler AutoClickResumeEvent;

        #endregion

        #endregion


    }
}
