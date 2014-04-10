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
        List<Guid> _plugins;
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
            var profiles = this.AddCurrentItem( R.Profile, "", _app.CivikeyHost.Context.ConfigManager.SystemConfiguration, a => a.CurrentUserProfile, a => a.UserProfiles, false, "" );

            _app.CivikeyHost.Context.ConfigManager.SystemConfiguration.UserProfiles.CollectionChanged += ( s, e ) =>
            {
                profiles.RefreshValues( s, e );
            };

            var scroll = new ConfigImplementationSelectorItem( _app.ConfigManager, new PluginCluster( _runner, _userConf, _screenScrollerId, new Guid[]{ _basicScrollId }, new Guid[0]));
            scroll.DisplayName = R.ScreenScrolling;
            Items.Add( scroll );
            _items.Add( scroll );

            var radar = new ConfigImplementationSelectorItem( _app.ConfigManager, new PluginCluster( _runner, _userConf, _radarId, new Guid[] { _basicScrollId }, new Guid[0] ) );
            radar.DisplayName = R.Radar;
            Items.Add( radar );
            _items.Add( radar );

            var empty = new ConfigImplementationSelectorItem( _app.ConfigManager, new PluginCluster( _runner, _userConf, Guid.Empty ) );
            empty.DisplayName = "DEMERDE TOI";
            Items.Add( empty );
            _items.Add( empty );

            var apply = new ConfigItemAction( _app.ConfigManager, new VMCommand( Apply ) );
            apply.DisplayName = "APPLY";
            Items.Add( apply );

            base.OnInitialize();
        }

        private void Apply()
        {
            ApplyStop();
            ApplyStart();
        }

        private void ApplyStop()
        {
            foreach( var i in _items )
            {
                if( !i.IsSelected )
                {
                    i.PluginCluster.StopPlugin();
                }
            }
        }

        private void ApplyStart()
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
