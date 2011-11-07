#region LGPL License
/*----------------------------------------------------------------------------
* This file (StandardPlugins\CommonTimer\CommonTimerEditor.cs) is part of CiviKey. 
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
* Copyright © 2007-2009, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Windows.Forms;
using CK.Plugin;
using CK.Plugin.Config;

namespace CK.StandardPlugins.CommonTimer
{
    //[Plugin( "{B9C6CCB0-BA03-477a-94D6-6035D39156E5}", 
    //    PublicName = "CommonTimer Editor", 
    //    Version = "1.0.0", 
    //    Categories = new string[] { "Editors" } )]
    public class CommonTimerEditor : IPlugin
    {
        [ConfigurationAccessor( "{E93C53AC-1621-4767-8489-097767205C87}" )]
        public IPluginConfigAccessor EditedConfiguration { get; set; }
       
        public int Interval
        {
            get
            {
                object o = EditedConfiguration.User["Interval"];
                return o == null ? 500 : (int)o;
            }
            set { EditedConfiguration.User["Interval"] = value; }
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
           
        }

        public void Stop()
        {
          
        }

        public void Teardown()
        {
           
        }
    }
}
