using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Windows;
using CK.Windows.Config;

namespace Host.VM
{
    public class PointerManagerPluginStarter : ConfigItem
    {
        ISimplePluginRunner _runner;
        PluginCluster _pluginCluster;
        ConfigPage _optionPage;
        AppViewModel _app;
        IKeyboard _keyboard;

        readonly Guid _screenScrollerId = new Guid( "{AE25D80B-B927-487E-9274-48362AF95FC0}" ); //ScreenScrollerPlugin
        readonly Guid _radarId = new Guid( "{390AFE83-C5A2-4733-B5BC-5F680ABD0111}" ); //MouseRadarPlugin
        readonly Guid _basicScrollId = new Guid( "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}" ); //ScrollerPlugin

        string KeyboardName
        {
            get { return _app.CivikeyHost.UserConfig.GetOrSet( "PointerManager_KeyboardName", "Clavier-souris" ); }
            set { _app.CivikeyHost.UserConfig.Set( "PointerManager_KeyboardName", value ); }
        }

        bool UseKeyboard
        {
            get { return _app.CivikeyHost.UserConfig.GetOrSet( "PointerManager_UseKeyboard", false ); }
            set { _app.CivikeyHost.UserConfig.Set( "PointerManager_UseKeyboard", value ); }
        }

        Guid Plugin
        {
            get { return _app.CivikeyHost.UserConfig.GetOrSet( "PointerManager_Plugin", _radarId ); }
            set { _app.CivikeyHost.UserConfig.Set( "PointerManager_Plugin", value ); }
        }

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
            if( e.PreviousName == KeyboardName ) KeyboardName = e.Keyboard.Name;
        }

        public bool IsRunning
        {
            get { return _pluginCluster.IsRunning || ( UseKeyboard && _app.KeyboardContext.Keyboards[KeyboardName].IsActive ); }
        }

        //Can't find all infos to decide whether a plugin is Disabled or not, yet.
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
            ) || (UseKeyboard && _app.KeyboardContext.Keyboards.FirstOrDefault( k => k.Name == KeyboardName ) != null);
        }

        void StartPointerManager()
        {
            if( UseKeyboard )
            {
                IKeyboard keyboard = _app.KeyboardContext.Keyboards.FirstOrDefault( k => KeyboardName == k.Name );

                if( keyboard != null )
                {
                    keyboard.IsActive = true;

                    NotifyOfPropertyChange( () => IsRunning );
                    NotifyOfPropertyChange( () => IsRunnable );

                    return;
                }
                else
                {
                    Plugin = _radarId;
                    UseKeyboard = false;
                }
            }
            _pluginCluster.StartPlugin();

            NotifyOfPropertyChange( () => IsRunning );
            NotifyOfPropertyChange( () => IsRunnable );
        }

        void StopPointerManager()
        {
            if( UseKeyboard )
            {
                Debug.Assert( _app.KeyboardContext.Keyboards.FirstOrDefault( k => k.Name == KeyboardName ) != null );
                Debug.Assert( _app.KeyboardContext.Keyboards.FirstOrDefault( k => k.Name == KeyboardName ) != null );
                _app.KeyboardContext.Keyboards[KeyboardName].IsActive = false;
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

        //need implementation
        public bool CanOpenEditor { get { return true; } }
    }
}
