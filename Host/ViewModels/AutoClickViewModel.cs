using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using CK.WPF.Controls;
using CK.Plugin.Config;
using CK.Core;
using CK.Reflection;
using Host.Resources;
using System.ComponentModel;
using CK.Plugin;
using CK.Windows.Config;

namespace Host.VM
{
    public class AutoClickViewModel : ConfigPage
    {
        AppViewModel _app;
        IPluginProxy _acPlugin;
        Guid _acId;
        IObjectPluginConfig _config;

        public AutoClickViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.AutoClickConfig;
            _app = app;
            _app.ConfigContainer.Changed += new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );
            _app.PluginRunner.PluginHost.StatusChanged += ( o, e ) => 
            {
                if( e.PluginProxy.PluginKey.PluginId == _acId && _acPlugin == null )
                {
                    InitializePlugin();
                }

                NotifyOfPropertyChange( () => ActivateAutoClic );
                NotifyOfPropertyChange( () => CountDownDuration );
                NotifyOfPropertyChange( () => TimeBeforeCountDownStarts );
                NotifyOfPropertyChange( () => ShowMousePanelOption );
            };
        }

        void InitializePlugin()
        {
            _acPlugin = _app.PluginRunner.PluginHost.FindLoadedPlugin( _acId, true );
            if( _acPlugin != null ) _config = _app.ConfigContainer.GetObjectPluginConfig( _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _acPlugin );
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.PluginId.UniqueId == _acId )
            {
                NotifyOfPropertyChange( () => CountDownDuration );
                NotifyOfPropertyChange( () => TimeBeforeCountDownStarts );
                NotifyOfPropertyChange( () => ShowMousePanelOption );
            }
        }

        public bool ActivateAutoClic
        {
            get { return _acPlugin != null ? _app.PluginRunner.PluginHost.IsPluginRunning( _acPlugin.PluginKey ) : false; }
            set
            {
                using( var wait = _app.ShowBusyIndicator() )
                {
                    if( value )
                    {
                        _app.StartPlugin( _acId );

                        if( _acPlugin == null ) _acPlugin = _app.PluginRunner.PluginHost.FindLoadedPlugin( _acId, true );
                        _config = _acPlugin != null ? _app.ConfigContainer.GetObjectPluginConfig( _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _acPlugin ) : null;
                    }
                    else
                    {
                        _app.StopPlugin( _acId );
                    }
                }

                NotifyOfPropertyChange( () => ActivateAutoClic );
                NotifyOfPropertyChange( () => CountDownDuration );
                NotifyOfPropertyChange( () => TimeBeforeCountDownStarts );
                NotifyOfPropertyChange( () => ShowMousePanelOption );
            }
        }

        public int CountDownDuration
        {
            get { return _config != null ? _config.GetOrSet( "CountDownDuration", 2000 ) : 0; }
            set
            {
                if( _config != null ) _config.Set( "CountDownDuration", value );
            }
        }

        public int TimeBeforeCountDownStarts
        {
            get { return _config != null ? _config.GetOrSet( "TimeBeforeCountDownStarts", 1500 ) : 0; }
            set
            {
                if( _config != null ) _config.Set( "TimeBeforeCountDownStarts", value );
            }
        }

        public bool ShowMousePanelOption
        {
            get { return _config != null ? _config.GetOrSet( "ShowMousePanelOption", false ) : false; }
            set
            {
                if( _config != null ) _config.Set( "ShowMousePanelOption", value );
            }
        }

        protected override void OnInitialize()
        {
            _acId = new Guid( "{989BE0E6-D710-489e-918F-FBB8700E2BB2}" );

            InitializePlugin();

            var g = this.AddActivableSection( R.AutoClickSectionName.ToLower(), R.AutoClickConfig, this, h => h.ActivateAutoClic, this );

            ConfigItemMillisecondProperty p2 = new ConfigItemMillisecondProperty( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, h => h.TimeBeforeCountDownStarts ) );
            p2.DisplayName = R.AutoClickTimeBeforeCountDownStarts;
            g.Items.Add( p2 );

            ConfigItemMillisecondProperty p = new ConfigItemMillisecondProperty( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, h => h.CountDownDuration ) );
            p.DisplayName = R.AutoClickCountDownDuration;
            g.Items.Add( p );

            g.AddProperty( R.AutoClickShowMousePanelOption, this, h => ShowMousePanelOption );

            base.OnInitialize();
        }
    }
}
