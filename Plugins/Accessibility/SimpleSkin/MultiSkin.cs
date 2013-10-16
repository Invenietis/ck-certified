using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using SimpleSkin.ViewModels;

namespace SimpleSkin
{
    [Plugin( MultiSkin.PluginIdString, PublicName = PluginPublicName, Version = MultiSkin.PluginIdVersion,
       Categories = new string[] { "Visual", "Accessibility" } )]
    public class MultiSkin : IPlugin
    {
        const string PluginPublicName = "MultiSkin";
        const string PluginIdString = "{D173E013-2491-4491-BF3E-CA2F8552B5EB}";
        const string PluginIdVersion = "0.9.0";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        #region IPlugin Members

        CKNoFocusWindowManager _noFocusWindowManager;
        IDictionary<string,SkinWindow> _skins;

        public bool Setup( IPluginSetupInfo info )
        {
            _skins = new Dictionary<string, SkinWindow>();
            return true;
        }

        public void Start()
        {
            if( KeyboardContext.Status == InternalRunningStatus.Started && KeyboardContext.Service.Keyboards.Actives.Count > 0 )
            {
                _noFocusWindowManager = new CKNoFocusWindowManager();
                foreach( var activeKeyboard in KeyboardContext.Service.Keyboards.Actives )
                {
                    var vm = new VMContextActiveKeyboard( activeKeyboard.Name, Context, KeyboardContext.Service.Keyboards.Context, Config, _noFocusWindowManager.NoFocusWindowThreadDispatcher );
                    var skin = _noFocusWindowManager.CreateNoFocusWindow<SkinWindow>( () => new SkinWindow
                    {
                        DataContext = vm
                    } );
                    _skins.Add( activeKeyboard.Name, skin );

                    skin.Dispatcher.Invoke( new Action( () => skin.Show() ) );
                }
            }
        }

        public void Stop()
        {
            foreach( var skin in _skins.Values )
            {
                skin.Close();
                ((IDisposable)skin.DataContext).Dispose();
            }
            _skins.Clear();
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
