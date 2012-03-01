using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Config;
using CK.Core;
using CK.Reflection;
using Host.Resources;
using CK.Keyboard.Model;
using CK.Windows.Config;
using Host.VM;

namespace Host
{
    public class RootConfigViewModel : CK.Windows.Config.ConfigPage
    {
        AppViewModel _app;
        AppConfigViewModel _appConfigVm;
        Guid _autoclicId;

        public RootConfigViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = Resources.R.Home;
            _app = app;
            _autoclicId = new Guid( "{989BE0E6-D710-489e-918F-FBB8700E2BB2}" );
        }

        protected override void OnInitialize()
        {
            if( _app.KeyboardContext != null )
            {
                var keyboards = this.AddCurrentItem( R.Keyboard, null, _app.KeyboardContext, c => c.CurrentKeyboard, c => c.Keyboards, false, "" );
                keyboards.ImagePath = "/Views/Images/Keyboard.png";//"pack://application:,,,/CK-Certified;component/Views/Images/Keyboard.png"

                _app.KeyboardContext.Keyboards.KeyboardCreated += ( s, e ) => { keyboards.RefreshValues( s, e ); };
                _app.KeyboardContext.Keyboards.KeyboardDestroyed += ( s, e ) => { keyboards.RefreshValues( s, e ); };
                _app.KeyboardContext.Keyboards.KeyboardRenamed += ( s, e ) => { keyboards.RefreshValues( s, e ); };
                _app.KeyboardContext.Keyboards.CurrentChanged += ( s, e ) => { keyboards.RefreshCurrent( s, e ); };

            }
            var g = this.AddGroup();
            var i = new ConfigFeatureStarter( ConfigManager, _app.PluginRunner, _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _autoclicId ) { DisplayName = R.AutoClickSectionName };
            g.Items.Add( i );

            this.AddLink( _appConfigVm ?? ( _appConfigVm = new AppConfigViewModel( _app ) ) );
            this.AddAction( R.ObjectExplorer, R.AdvancedUserNotice, StartObjectExplorer );
            base.OnInitialize();
        }

        public CivikeyStandardHost CivikeyHost { get; private set; }

        public void StartObjectExplorer()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{4BF2616D-ED41-4E9F-BB60-72661D71D4AF}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }

    }
}
