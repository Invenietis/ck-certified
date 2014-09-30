#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\HelpServices\IHelpUpdaterService.cs) is part of CiviKey. 
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
using CK.Core;
using CK.Plugin;

namespace Help.Services
{
    public interface IHelpUpdaterService : IDynamicService
    {
        /// <summary>
        /// Fired when a new update is available for a plugin.
        /// At this point the update content is not dowloaded yet.
        /// </summary>
        event EventHandler<HelpUpdateEventArgs> UpdateAvailable;

        /// <summary>
        /// Fired when a new update content is available on the file system.
        /// At this point the update can be inspected and installed.
        /// </summary>
        event EventHandler<HelpUpdateDownloadedEventArgs> UpdateDownloaded;

        /// <summary>
        /// Fired when a new update content is installed.
        /// </summary>
        event EventHandler<HelpUpdateDownloadedEventArgs> UpdateInstalled;

        void StartManualUpdate();
    }

    public class HelpUpdateEventArgs : EventArgs
    {
        public HelpUpdateEventArgs( INamedVersionedUniqueId plugin )
        {
            Plugin = plugin;
        }

        public INamedVersionedUniqueId Plugin { get; private set; }
    }

    public class HelpUpdateDownloadedEventArgs : HelpUpdateEventArgs
    {
        public HelpUpdateDownloadedEventArgs( INamedVersionedUniqueId plugin, IDownloadResult downloadResult )
            : base( plugin )
        {
            DownloadResult = downloadResult;
        }

        public IDownloadResult DownloadResult { get; private set; }
    }
}
