using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Core;

namespace Host.VM
{
    public class PluginCluster
    {
        List<Guid> _startWithPlugin;
        List<Guid> _stopWithPlugin;
        Guid _pluginId;
        ISimplePluginRunner _runner;
        IUserConfiguration _userConfig;

        public PluginCluster( ISimplePluginRunner runner, IUserConfiguration userConfig, params Guid[] pluginId )
        {
            _pluginId = pluginId[0];
            List<Guid> plugins = pluginId.ToList();
            _startWithPlugin = plugins;
            _stopWithPlugin = plugins;
            _runner = runner;
            _userConfig = userConfig;
        }

        public PluginCluster( ISimplePluginRunner runner, IUserConfiguration userConfig, Guid pluginId, IEnumerable<Guid> startWithPlugin, IEnumerable<Guid> stopWithPlugin )
        {
            _pluginId = pluginId;
            _startWithPlugin = new List<Guid>();
            _startWithPlugin.Add( pluginId );
            _startWithPlugin.AddRange( startWithPlugin );

            _stopWithPlugin = new List<Guid>();
            _stopWithPlugin.Add( pluginId );
            _stopWithPlugin.AddRange( stopWithPlugin );

            _runner = runner;
            _userConfig = userConfig;
        }

        public IReadOnlyList<Guid> StartWithPlugin { get { return _startWithPlugin.ToReadOnlyList(); } }

        public IReadOnlyList<Guid> StopWithPlugin { get { return _stopWithPlugin.ToReadOnlyList(); } }

        public Guid MainPluginId { get { return _pluginId; } }

        public bool IsRunning
        {
            get { return _startWithPlugin.Intersect( _stopWithPlugin ).All( ( id ) => _runner.PluginHost.IsPluginRunning( id ) ); }
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

        public void StartPlugin()
        {
            foreach( var id in _startWithPlugin )
            {
                _userConfig.PluginsStatus.SetStatus( id, ConfigPluginStatus.AutomaticStart );
                _userConfig.LiveUserConfiguration.SetAction( id, ConfigUserAction.Started );
            }

            _runner.Apply();
        }

        public void StopPlugin()
        {
            foreach( var id in _stopWithPlugin )
            {
                _userConfig.PluginsStatus.SetStatus( id, ConfigPluginStatus.Manual );
                _userConfig.LiveUserConfiguration.SetAction( id, ConfigUserAction.Stopped );
            }

            _runner.Apply();
        }


    }
}
