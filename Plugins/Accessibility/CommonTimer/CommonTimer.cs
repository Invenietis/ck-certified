#region LGPL License
/*----------------------------------------------------------------------------
* This file (StandardPlugins\CommonTimer\CommonTimer.cs) is part of CiviKey. 
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
using CommonServices;
using CK.Plugin.Config;

namespace CK.Plugins.CommonTimer
{
    [Plugin( CommonTimer.PluginId, PublicName = CommonTimer.PluginName,  Version = CommonTimer.PluginVersion,
        Categories = new string[] { "Accessibility" },
        IconUri = "Plugins/CommonTimer/Resources/CommonTimer.ico",
        Description = "Simple timer, used to be a reference for others plugins" )]
    public class CommonTimer : IPlugin, ICommonTimer
    {
        const string PluginId = "{E93C53AC-1621-4767-8489-097767205C87}";
        const string PluginName = "Common timer";
        const string PluginVersion = "1.0.0";
        
        const int DefaultInterval = 500;

        Timer _timer;

        /// <summary>
        /// Gets the configuration of the plugin.
        /// </summary>
        public IPluginConfigAccessor Configuration { get; set; }

		public event EventHandler Tick
		{
			add { _timer.Tick += value; }
			remove { _timer.Tick -= value; }
		}

        public event EventHandler IntervalChanged;

		public int Interval
		{
            get { return Configuration.User.GetOrSet( "Interval", DefaultInterval ); }
			set
			{
                if( value <= 0 ) value = DefaultInterval;
                if( value != Interval )
                {
                    Configuration.User["Interval"] = _timer.Interval = value;
                    if( IntervalChanged != null ) IntervalChanged( this, EventArgs.Empty );
                }
			}
		}

        public bool Setup( IPluginSetupInfo info )
        {
            _timer = new Timer();
            _timer.Interval = Interval;
            return true;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Teardown()
        {
            _timer.Dispose();
        }
    }
}
