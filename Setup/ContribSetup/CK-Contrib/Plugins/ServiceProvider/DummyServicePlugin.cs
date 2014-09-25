#region LGPL License
/*----------------------------------------------------------------------------
* This file (Setup\ContribSetup\CK-Contrib\Plugins\ServiceProvider\DummyServicePlugin.cs) is part of CiviKey. 
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
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using System.Windows;

namespace DummyPlugins
{
    /// <summary>
    /// Class that represent a CiviKey service. It is a plugin that implements a CiviKeyService (here, IDummyService)
    /// </summary>
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class DummyServicePlugin : IPlugin, IDummyService
    {
        //This GUID should be re-generated to give this plugin a unique ID
        const string PluginGuidString = "{8C4DA5A4-95E5-4a9e-83BF-51FFC0B43E59}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "DummyServicePlugin";

        //Reference to the storage object that enables one to save data.
        //This object is injected after all plugins' Setup method have been called
        public IPluginConfigAccessor Config { get; set; }

        /// <summary>
        /// Constructor of the class, all services are null
        /// </summary>
        public DummyServicePlugin()
        {
        }

        /// <summary>
        /// First called method on the class, at this point, all services are null.
        /// Used to set up the service exposed by this plugin (if any).
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        /// <summary>
        /// Called after the Setup method.
        /// All launched services are now set, you may now set up the link to the service used by this class
        /// </summary>
        public void Start()
        {
            //Saving a value into the SharedDictionary. Note that these values are saved as XML when CiviKey is stopped.
            if( Config.User["AnswerToLife"] == null)
                Config.User.Add( "AnswerToLife", 42 );
        }

        /// <summary>
        /// First method called when the plugin is stopping
        /// You should remove all references to any service here.
        /// </summary>
        public void Stop()
        {

        }

        /// <summary>
        /// Called after Stop()
        /// All services are null
        /// </summary>
        public void Teardown()
        {
        }

        public int GetAnswerToLife()
        {
            //Retrieve values from the SharedDictionary
            return (int)Config.User["AnswerToLife"];
        }
    }
}
