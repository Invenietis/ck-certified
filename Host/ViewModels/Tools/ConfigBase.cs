using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows;
using CK.Windows.Config;

namespace Host.VM
{
    public abstract class ConfigBase : ConfigPage
    {
        protected AppViewModel _app;
        Guid _editedPluginId;
        protected IPluginProxy _plugin;
        IPluginInfo _pluginInfo;
        IObjectPluginConfig _config;

        public IObjectPluginConfig Config
        {
            get { return _config; }
        }

        public ConfigBase( string editedPluginId, string displayName, AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = displayName;

            _editedPluginId = new Guid( editedPluginId );
            _app = app;
            _app.ConfigContainer.Changed += new EventHandler<ConfigChangedEventArgs>( OnConfigChangedWrapper );

            InitializePlugin();

            _app.PluginRunner.ApplyDone += ( o, e ) =>
            {
                if( _pluginInfo == null || _plugin == null )
                {
                    InitializePlugin();
                }
            };
        }

        void InitializePlugin()
        {
            bool info = _pluginInfo == null;
            bool plugin = _plugin == null;

            _pluginInfo = _pluginInfo ?? _app.PluginRunner.Discoverer.FindPlugin( _editedPluginId );
            _plugin = _plugin ?? _app.PluginRunner.PluginHost.FindLoadedPlugin( _editedPluginId, true );

            if( _plugin != null )
            {
                _config = _app.ConfigContainer.GetObjectPluginConfig( _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _plugin );
                Debug.Assert( _config != null );
            }

            if( info && _pluginInfo != null ) OnPluginDiscovered();
            if( plugin && _plugin != null ) OnPluginLoaded();

            NotifyOfPropertiesChange();
        }

        public bool ActivatePlugin
        {
            get { return _plugin != null ? _app.PluginRunner.PluginHost.IsPluginRunning( _plugin.PluginKey ) : false; }
            set
            {
                using( var wait = _app.ShowBusyIndicator() )
                {
                    if( value )
                    {
                        _app.StartPlugin( _editedPluginId );

                        if( _plugin == null ) _plugin = _app.PluginRunner.PluginHost.FindLoadedPlugin( _editedPluginId, true );
                        _config = _plugin != null ? _app.ConfigContainer.GetObjectPluginConfig( _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _plugin ) : null;
                    }
                    else
                    {
                        _app.StopPlugin( _editedPluginId );
                    }
                }

                NotifyOfPropertiesChange();
            }
        }

        protected virtual void NotifyOfPropertiesChange()
        {
            NotifyOfPropertyChange( () => ActivatePlugin );
            OnConfigChangedInternal( this, null );
        }

        /// <summary>
        /// Called when the plugin could be retrieved (the plugin is LOADED). From that moment on, if a service exists, it is available.
        /// </summary>
        protected virtual void OnPluginLoaded()
        {
        }

        /// <summary>
        /// Called when the plugin info could be retrieved the plugin has at least been DISCOVERED). From that moment on, the Config is available.
        /// </summary>
        protected virtual void OnPluginDiscovered()
        {
        }

        private void OnConfigChangedWrapper( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Contains( _plugin ) )
            {
                OnConfigChangedInternal( sender, e );
            }
        }

        void OnConfigChangedInternal( object sender, ConfigChangedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );
            OnConfigChanged( sender, e );
        }

        protected abstract void OnConfigChanged( object sender, ConfigChangedEventArgs e );

        protected ConfigActivableSection AddActivableSection( string name, string description )
        {
            return this.AddActivableSection( name, description, this, h => h.ActivatePlugin, this );
        }

        protected override void OnInitialize()
        {
            InitializePlugin();
            base.OnInitialize();
        }
    }
}
