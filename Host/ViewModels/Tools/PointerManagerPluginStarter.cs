using System;
using System.Linq;
using CK.Plugin;
using CK.Plugin.Config;
using System.Windows.Input;
using CK.Windows.Config;
using CK.Windows;
using System.Collections.Generic;
using CK.Keyboard.Model;

namespace Host.VM
{
    public class PointerManagerPluginStarter : ConfigItem
    {
        ISimplePluginRunner _runner;
        PluginCluster _pluginCluster;
        ConfigPage _optionPage;
        AppViewModel _app;
        IKeyboard _keyboard;

        Guid _basicScrollId = new Guid( "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}" );
        Guid _radarId = new Guid( "{390AFE83-C5A2-4733-B5BC-5F680ABD0111}" );

        public PointerManagerPluginStarter( AppViewModel app, ConfigPage optionPage = null )
            : base( app.ConfigManager )
        {
            _app = app;
            _optionPage = optionPage;

            _pluginCluster = new PluginCluster(
                    _app.PluginRunner,
                    _app.CivikeyHost.Context.ConfigManager.UserConfiguration,
                    () => _app.CivikeyHost.UserConfig.GetOrSet( "PointerManager_Plugin", _radarId ),
                    () => new Guid[] { _basicScrollId } );

            _runner = _app.PluginRunner;
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

            Start = new SimpleCommand( StartPointerManager, CanStart );
            Stop = new SimpleCommand( StopPointerManager );
            OpenEditor = new SimpleCommand( GoTo );

            _app.KeyboardContext.Keyboards.KeyboardRenamed += Keyboards_KeyboardRenamed;
        }

        void Keyboards_KeyboardRenamed( object sender, KeyboardRenamedEventArgs e )
        {
            if( e.PreviousName == _app.CivikeyHost.UserConfig.GetOrSet( "PointerManager_KeyboardName", "Clavier-souris" ) ) _app.CivikeyHost.UserConfig.Set( "PointerManager_KeyboardName", e.Keyboard.Name );
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

        void StartPointerManager()
        {
            if( _app.CivikeyHost.UserConfig.GetOrSet( "PointerManager_UseKeyboard", false ) )
            {
                string keyboardConfName = _app.CivikeyHost.UserConfig.GetOrSet( "PointerManager_KeyboardName", "Clavier-souris" );
                _keyboard = _app.KeyboardContext.Keyboards.FirstOrDefault( k => keyboardConfName == k.Name );
                if( _keyboard != null )
                {
                    _keyboard.IsActive = true;

                    NotifyOfPropertyChange( () => IsRunning );
                    NotifyOfPropertyChange( () => IsRunnable );

                    return;
                }
                else
                {
                    _app.CivikeyHost.UserConfig.Set( "PointerManager_Plugin", _radarId );
                    _app.CivikeyHost.UserConfig.Set( "PointerManager_UseKeyboard", false );
                }
            }
            _pluginCluster.StartPlugin();

            NotifyOfPropertyChange( () => IsRunning );
            NotifyOfPropertyChange( () => IsRunnable );
        }

        void StopPointerManager()
        {
            if( _app.CivikeyHost.UserConfig.GetOrSet( "PointerManager_UseKeyboard", false ) )
            {
                _keyboard.IsActive = false;
                _keyboard = null;
            }
            else
            {
                _pluginCluster.StopPlugin();
            }

            NotifyOfPropertyChange( () => IsRunning );
            NotifyOfPropertyChange( () => IsRunnable );
        }

        void GoTo()
        {
            ConfigManager.ActivateItem( _optionPage );
        }

        public ICommand Start { get; private set; }

        public ICommand Stop { get; private set; }

        public ICommand OpenEditor { get; private set; }
    }
}
