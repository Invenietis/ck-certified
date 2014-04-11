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
    public class ImplementationSelector : ConfigPage
    {
        readonly AppViewModel _app;
        readonly ISimplePluginRunner _runner;
        readonly IUserConfiguration _userConf;

        readonly Guid _screenScrollerId;
        readonly Guid _radarId;
        readonly Guid _basicScrollId;

        readonly List<ConfigImplementationSelectorItem> _items;

        public ImplementationSelector( string displayName, AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = displayName;
            _app = app;
            _runner = app.PluginRunner;
            _userConf = _app.CivikeyHost.Context.ConfigManager.UserConfiguration;

            _screenScrollerId = new Guid( "{AE25D80B-B927-487E-9274-48362AF95FC0}" );
            _radarId = new Guid( "{390AFE83-C5A2-4733-B5BC-5F680ABD0111}" );
            _basicScrollId = new Guid( "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}" );

            _items = new List<ConfigImplementationSelectorItem>();
        }

        protected override void OnInitialize()
        {

            Guid defaultPlugin = GetDefaultItem( _screenScrollerId, _radarId );

            var scroll = new ConfigImplementationSelectorItem( _app.ConfigManager, new PluginCluster( _runner, _userConf, _screenScrollerId, new Guid[]{ _basicScrollId }, new Guid[0]), "CursorPointing");
            scroll.DisplayName = R.ScreenScrolling;
            if( defaultPlugin == _screenScrollerId ) scroll.IsDefaultItem = true;
            Items.Add( scroll );
            _items.Add( scroll );

            var radar = new ConfigImplementationSelectorItem( _app.ConfigManager, new PluginCluster( _runner, _userConf, _radarId, new Guid[] { _basicScrollId }, new Guid[0] ), new Guid( "{275B0E68-B880-463A-96E5-342C8E31E229}" ), "CursorPointing" );
            radar.DisplayName = R.Radar;
            if( defaultPlugin == _radarId ) radar.IsDefaultItem = true;
            Items.Add( radar );
            _items.Add( radar );

            var empty = new ConfigImplementationSelectorItem( _app.ConfigManager, new PluginCluster( _runner, _userConf, Guid.Empty ), "CursorPointing" );
            empty.DisplayName = "Aucun ";
            if( defaultPlugin == Guid.Empty ) empty.IsDefaultItem = true;
            Items.Add( empty );
            _items.Add( empty );

            var apply = new ConfigItemApply( _app.ConfigManager, new VMCommand( Apply ) );
            apply.DisplayName = "Appliquer";
            Items.Add( apply );

            base.OnInitialize();
        }

        private void Apply()
        {
            SetConfigStopped();
            SetConfigStarted();
            _runner.Apply();
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

        private Guid GetDefaultItem( params Guid[] ids )
        {
            if( ids.Count( i => _runner.PluginHost.IsPluginRunning( i ) ) > 1 ) return Guid.Empty;
            return ids.First( i => _runner.PluginHost.IsPluginRunning( i ) );
        }
    }
}
