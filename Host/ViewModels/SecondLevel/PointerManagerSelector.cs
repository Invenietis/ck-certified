using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows;
using CK.Windows.App;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    public class PointerManagerSelector : ConfigPage
    {
        readonly AppViewModel _app;
        readonly ISimplePluginRunner _runner;
        readonly IUserConfiguration _userConf;

        readonly Guid _screenScrollerId = new Guid( "{AE25D80B-B927-487E-9274-48362AF95FC0}" ); //ScreenScrollerPlugin
        readonly Guid _radarId = new Guid( "{390AFE83-C5A2-4733-B5BC-5F680ABD0111}" ); //MouseRadarPlugin
        readonly Guid _basicScrollId = new Guid( "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}" ); //ScrollerPlugin

        readonly List<ConfigImplementationSelectorItem> _items;

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

        public PointerManagerSelector( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = "Selection du dispositif de pointage";
            _app = app;
            _runner = app.PluginRunner;
            _userConf = _app.CivikeyHost.Context.ConfigManager.UserConfiguration;

            _items = new List<ConfigImplementationSelectorItem>();
        }

        const string groupName = "CursorPointing";

        protected override void OnInitialize()
        {
            Guid defaultPlugin = Plugin;
            _previous = defaultPlugin;

            var scroll = new ConfigImplementationSelectorItem( _app.ConfigManager, new PluginCluster( _runner, _userConf, () => _screenScrollerId ), groupName );
            scroll.DisplayName = R.ScreenScrolling;
            scroll.Description = R.ScreenScrollingDescription;
            if( defaultPlugin == _screenScrollerId ) scroll.IsDefaultItem = true;
            Items.Add( scroll );
            _items.Add( scroll );

            var radar = new ConfigImplementationSelectorItem( _app.ConfigManager, new PluginCluster( _runner, _userConf, () => _radarId ), new Guid( "{275B0E68-B880-463A-96E5-342C8E31E229}" ), groupName ); //MouseRadarEditor
            radar.DisplayName = R.Radar;
            radar.Description = R.RadarDescription;
            if( defaultPlugin == _radarId
                || ( UseKeyboard && _app.KeyboardContext.Keyboards.FirstOrDefault( k => k.Name == KeyboardName ) == null) )
            {
                radar.IsDefaultItem = true;
            }
            Items.Add( radar );
            _items.Add( radar );

            var mouseKeyboard = new ConfigImplementationSelectorItem( _app.ConfigManager, new PluginCluster( _runner, _userConf, Guid.Empty ), new KeyboardPointerSelector( _app ), groupName );
            mouseKeyboard.DisplayName = R.KeyboardPointer;
            mouseKeyboard.Description = R.KeyboardPointerDescription;
            if( UseKeyboard && _app.KeyboardContext.Keyboards.FirstOrDefault( k => k.Name == KeyboardName ) != null ) 
                mouseKeyboard.IsDefaultItem = true;
            Items.Add( mouseKeyboard );
            _items.Add( mouseKeyboard );

            var apply = new RadioConfigItemApply( _app.ConfigManager, new VMCommand( Apply ), scroll, radar, mouseKeyboard );
            apply.DisplayName = R.Apply;
            Items.Add( apply );

            base.OnInitialize();
        }

        Guid _previous;
        //TODO : need adaptation for checkbox selection
        Guid Current { get { return _items.Single( i => i.IsSelected ).PluginCluster.MainPluginId; } }

        internal bool IsDirty
        {
            get { return _previous != Current; }
        }

        private void Apply()
        {
            if( Current == Guid.Empty )
            {
                if( IsStart() )
                {
                    SetConfigStopped();
                    _runner.Apply();
                    _app.KeyboardContext.Keyboards[KeyboardName].IsActive = true;
                }
                UseKeyboard = true;
            }
            else
            {
                if( IsStart() )
                {
                    SetConfigStopped();
                    SetConfigStarted();
                    _runner.Apply();
                }
                UseKeyboard = false;
                Plugin = Current;
            }
            _previous = Current;
        }

        private bool IsStart()
        {
            return (!UseKeyboard && _previous != Guid.Empty && _runner.PluginHost.IsPluginRunning( _previous ))
                    || ( UseKeyboard && _app.KeyboardContext.Keyboards[KeyboardName].IsActive);
        }

        private void SetConfigStopped()
        {
            foreach( var i in _items )
            {
                if( !i.IsSelected )
                {
                    i.PluginCluster.StopPlugin();
                }
            }
        }

        private void SetConfigStarted()
        {
            foreach( var i in _items )
            {
                if( i.IsSelected )
                {
                    i.PluginCluster.StartPlugin();
                }
            }
        }
    }
}
