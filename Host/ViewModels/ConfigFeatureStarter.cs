using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using CK.WPF.Controls;
using CK.Plugin;
using CK.Plugin.Config;
using System.Windows.Input;
using CK.Windows.Config;
using CK.Windows;

namespace Host.VM
{
    public class ConfigFeatureStarter : ConfigItem
    {
        Guid[] _pluginIds;
        ISimplePluginRunner _runner;
        IUserConfiguration _userConfig;


        public ConfigFeatureStarter( ConfigManager configManager, ISimplePluginRunner runner, IUserConfiguration userConfig, params Guid[] pluginId )
            : base( configManager )
        {
            _pluginIds = pluginId;
            _runner = runner;
            _userConfig = userConfig;

            _runner.PluginHost.StatusChanged += ( o, e ) =>
            {
                if( _pluginIds.Contains( e.PluginProxy.PluginKey.PluginId ) )
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
            return _pluginIds.All
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
            foreach( var id in _pluginIds )
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
            foreach( var id in _pluginIds )
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
            get { return _pluginIds.All( ( id ) => _runner.PluginHost.IsPluginRunning( id ) );}
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
