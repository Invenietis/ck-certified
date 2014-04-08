#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\ConfigFeatureStarter.cs) is part of CiviKey. 
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
        Guid[] _startWithPlugin;
        Guid[] _stopWithPlugin;
        ISimplePluginRunner _runner;
        IUserConfiguration _userConfig;


        public ConfigFeatureStarter( ConfigManager configManager, ISimplePluginRunner runner, IUserConfiguration userConfig, params Guid[] pluginId )
            : base( configManager )
        {
            _startWithPlugin = pluginId;
            _stopWithPlugin = pluginId;
            _runner = runner;
            _userConfig = userConfig;

            _runner.PluginHost.StatusChanged += ( o, e ) =>
            {
                if( _startWithPlugin.Contains( e.PluginProxy.PluginKey.PluginId ) || _stopWithPlugin.Contains( e.PluginProxy.PluginKey.PluginId ) )
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

        public ConfigFeatureStarter( ConfigManager configManager, ISimplePluginRunner runner, IUserConfiguration userConfig, Guid pluginId, IEnumerable<Guid> startWithPlugin, IEnumerable<Guid> stopWithPlugin )
            : base( configManager )
        {
            _startWithPlugin = new Guid[] { pluginId }.Union( startWithPlugin ).ToArray();
            _stopWithPlugin = new Guid[] { pluginId }.Union( stopWithPlugin ).ToArray();
            _runner = runner;
            _userConfig = userConfig;

            _runner.PluginHost.StatusChanged += ( o, e ) =>
            {
                if( _startWithPlugin.Contains( e.PluginProxy.PluginKey.PluginId ) || _stopWithPlugin.Contains( e.PluginProxy.PluginKey.PluginId ) )
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

        bool CanStart()
        {
            return _startWithPlugin.All
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
            foreach( var id in _startWithPlugin )
            {
                _userConfig.PluginsStatus.SetStatus( id, ConfigPluginStatus.AutomaticStart );
                _userConfig.LiveUserConfiguration.SetAction( id, ConfigUserAction.Started );
            }
            
            _runner.Apply();

            NotifyOfPropertyChange( () => IsRunning );
            NotifyOfPropertyChange( () => IsRunnable );
        }

        void StopPlugin()
        {
            foreach( var id in _stopWithPlugin )
            {
                _userConfig.PluginsStatus.SetStatus( id, ConfigPluginStatus.Manual );
                _userConfig.LiveUserConfiguration.SetAction( id, ConfigUserAction.Stopped );
            }

            _runner.Apply();

            NotifyOfPropertyChange( () => IsRunning );
            NotifyOfPropertyChange( () => IsRunnable );
        }

        public bool IsRunning
        {
            get { return _startWithPlugin.Intersect(_stopWithPlugin).All( ( id ) => _runner.PluginHost.IsPluginRunning( id ) ); }
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


        public ICommand Start { get; private set; }

        public ICommand Stop { get; private set; }
    }
}
