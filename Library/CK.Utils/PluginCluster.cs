#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\Tools\PluginCluster.cs) is part of CiviKey. 
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
using CK.Plugin;
using CK.Plugin.Config;
using CK.Core;

namespace CK.Utils
{
    /// <summary>
    /// This object wraps a number of plugins that work together to create a functionnality.
    /// These plugins form a bundle that are started and stopped togther in order to make sure the system stays stable.
    /// Handles setting explicitely the plugins to start and those to stop.
    /// </summary>
    public class PluginCluster
    {
        Func<List<Guid>> _startWithPlugin;
        Func<List<Guid>> _stopWithPlugin;
        ISimplePluginRunner _runner;
        IUserConfiguration _userConfig;

        /// <summary>
        /// Ctor to use in simple cases
        /// </summary>
        /// <param name="runner">The runner</param>
        /// <param name="userConfig">The user configuration</param>
        /// <param name="pluginIds">The pluginIds of those that should be started and stopped when the cluster is started or stopped. 
        /// The first one is considered the "main" (note this has no impact on the way start/stop are handled)</param>
        public PluginCluster( ISimplePluginRunner runner, IUserConfiguration userConfig, params Guid[] pluginIds )
        {
            if( pluginIds.Length == 0 ) throw new ArgumentException( "A PluginCluster cannot be created without pluginIds. It need at least one" );
            List<Guid> plugins = pluginIds.ToList();
            _startWithPlugin = () => plugins;
            _stopWithPlugin = () => plugins;
            _runner = runner;
            _userConfig = userConfig;
        }

        /// <summary>
        /// Ctor to use when the plugins that should be started together with the main plugin are different from those that should be stopped together with the main plugin.
        /// </summary>
        /// <param name="runner">The runner</param>
        /// <param name="userConfig">The user configuration</param>
        /// <param name="mainPluginId">The Guid of the main plugin</param>
        /// <param name="startWithPlugin">The plugins to start together with the main plugin</param>
        /// <param name="stopWithPlugin">The plugins to stop together with the main plugin</param>
        public PluginCluster( ISimplePluginRunner runner, IUserConfiguration userConfig, Guid mainPluginId, IEnumerable<Guid> startWithPlugin, IEnumerable<Guid> stopWithPlugin )
        {
            var startList = new List<Guid>();
            startList.Add( mainPluginId );
            startList.AddRange( startWithPlugin );

            _startWithPlugin = () => startList;

            var stopList = new List<Guid>();
            stopList.Add( mainPluginId );
            stopList.AddRange( stopWithPlugin );

            _stopWithPlugin = () => stopList;

            _runner = runner;
            _userConfig = userConfig;
        }

        public PluginCluster( ISimplePluginRunner runner, IUserConfiguration userConfig, Func<Guid> selectMainPlugin, Func<IEnumerable<Guid>> selectStartWithPlugin = null, Func<IEnumerable<Guid>> selectStopWithPlugin = null )
        {
            Func<List<Guid>> startFunc = () =>
            {
                var startList = new List<Guid>();
                startList.Add( selectMainPlugin() );
                if( selectStartWithPlugin != null ) startList.AddRange( selectStartWithPlugin() );
                return startList;
            };

            _startWithPlugin = startFunc;
            Func<List<Guid>> stopFunc = () =>
            {
                var stopList = new List<Guid>();
                stopList.Add( selectMainPlugin() );
                if( selectStopWithPlugin != null ) stopList.AddRange( selectStopWithPlugin() );
                return stopList;
            };

            _stopWithPlugin = stopFunc;

            _runner = runner;
            _userConfig = userConfig;
        }

        public IUserConfiguration UserConfig { get { return _userConfig; } }

        IReadOnlyList<Guid> _startWithPluginRO;
        public IReadOnlyList<Guid> StartWithPlugin
        {
            get
            {
                if( _startWithPluginRO == null ) _startWithPluginRO = _startWithPlugin().ToReadOnlyList();
                return _startWithPluginRO;
            }
        }

        IReadOnlyList<Guid> _stopWithPluginRO;
        public IReadOnlyList<Guid> StopWithPlugin
        {
            get
            {
                if( _stopWithPluginRO == null ) _stopWithPluginRO = _stopWithPlugin().ToReadOnlyList();
                return _stopWithPluginRO;
            }
        }

        /// <summary>
        /// The Guid of the main plugin.
        /// If the simple constructor has been used to create this object, the first of the list is considered the main plugin.
        /// </summary>
        public Guid MainPluginId { get { return _startWithPlugin().FirstOrDefault(); } }

        public bool IsRunning
        {
            get { return _startWithPlugin().Intersect( _stopWithPlugin() ).All( ( id ) => _runner.PluginHost.IsPluginRunning( id ) ); }
        }

        //Can't find all the info to decide whether a plugin is Disabled or not, yet.
        public bool IsRunnable
        {
            get
            {
                //TODO
                return true;
            }
        }

        /// <summary>
        /// Set and applies the configuration with started status.
        /// Applies even for plugins outside the cluster, if his configuration has changed.
        /// </summary>
        public void StartPlugin()
        {
            SetStartConfigStatus(); 
            _runner.Apply();
        }

        /// <summary>
        /// Set and applies the configuration with stopped status.
        /// Applies even for plugins outside the cluster, if his configuration has changed.
        /// </summary>
        public void StopPlugin()
        {
            SetStopConfigStatus();
            _runner.Apply();
        }

        /// <summary>
        /// Just set the configuration with started status.
        /// </summary>
        public void SetStartConfigStatus()
        {
            foreach( var id in _startWithPlugin() )
            {
                _userConfig.PluginsStatus.SetStatus( id, ConfigPluginStatus.AutomaticStart );
                _userConfig.LiveUserConfiguration.SetAction( id, ConfigUserAction.Started );
            }
        }

        /// <summary>
        /// Just set the configuration with stopped status.
        /// </summary>
        public void SetStopConfigStatus()
        {
            foreach( var id in _stopWithPlugin() )
            {
                _userConfig.PluginsStatus.SetStatus( id, ConfigPluginStatus.Manual );
                _userConfig.LiveUserConfiguration.SetAction( id, ConfigUserAction.Stopped );
            }
        }

        /// <summary>
        /// Applies all configuration changes, even for plugins outside the cluster if his configuration has changed.
        /// </summary>
        public void ApplyNewConfig()
        {
            _runner.Apply();
        }
    }
}
