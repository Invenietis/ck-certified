#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\Tools\ConfigFeatureStarter.cs) is part of CiviKey. 
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
using System.Linq;
//using CK.WPF.Controls;
using CK.Plugin;
using CK.Plugin.Config;
using System.Windows.Input;
using CK.Windows.Config;
using CK.Windows;
using System.Collections.Generic;

namespace Host.VM
{
    public class ConfigFeatureStarter : ConfigItem
    {
        ISimplePluginRunner _runner;
        PluginCluster _pluginCluster;

        public ConfigFeatureStarter( ConfigManager configManager, ISimplePluginRunner runner, PluginCluster pluginCluster )
            : base( configManager )
        {
            _pluginCluster = pluginCluster;
            _runner = runner;
            _runner.PluginHost.StatusChanged += ( o, e ) =>
            {
                if( _pluginCluster.StartWithPlugin.Contains( e.PluginProxy.PluginKey.PluginId ) || _pluginCluster.StopWithPlugin.Contains( e.PluginProxy.PluginKey.PluginId ) )
                {
                    NotifyOfPropertyChange( () => Start );
                    NotifyOfPropertyChange( () => Stop );
                    NotifyOfPropertyChange( () => IsRunning );
                    NotifyOfPropertyChange( () => IsRunnable );
                }
            };

            Start = new SimpleCommand( StartPlugin, CanStart );
            Stop = new SimpleCommand( StopPlugin );
        }

        public bool IsRunning
        {
            get { return _pluginCluster.IsRunning; }
        }

        //Can't find all the info to decide whether a plugin is Disabled or not, yet.
        public bool IsRunnable
        {
            get
            {
                return _pluginCluster.IsRunnable;
            }
        }

        bool CanStart()
        {
            return _pluginCluster.StartWithPlugin.All
            (
                ( id ) =>
                {
                    var p = _runner.Discoverer.FindPlugin( id );
                    return p != null && !p.HasError;
                }
            );
        }

        void StartPlugin()
        {
            _pluginCluster.StartPlugin();

            NotifyOfPropertyChange( () => IsRunning );
            NotifyOfPropertyChange( () => IsRunnable );
        }

        void StopPlugin()
        {
            _pluginCluster.StopPlugin();

            NotifyOfPropertyChange( () => IsRunning );
            NotifyOfPropertyChange( () => IsRunnable );
        }

        public ICommand Start { get; private set; }

        public ICommand Stop { get; private set; }
    }
}
