using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WindowManager.Model;
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

        struct SkinInfo
        {
            public SkinInfo( SkinWindow window, VMContextActiveKeyboard vm, Dispatcher d , WindowManagerSubscriber sub )
            {
                Skin = window;
                ViewModel = vm;
                Dispatcher = d;
                Subscriber = sub;
            }
            
            public readonly WindowManagerSubscriber Subscriber;

            public readonly SkinWindow Skin;

            public readonly VMContextActiveKeyboard ViewModel;

            public readonly Dispatcher Dispatcher;
        }

        #region IPlugin Members

        CKNoFocusWindowManager _noFocusWindowManager;
        IDictionary<string,SkinInfo> _skins;

        public bool Setup( IPluginSetupInfo info )
        {
            _skins = new Dictionary<string, SkinInfo>();
            return true;
        }

        public void Start()
        {
            if( KeyboardContext.Status == InternalRunningStatus.Started && KeyboardContext.Service.Keyboards.Actives.Count > 0 )
            {
                _noFocusWindowManager = new CKNoFocusWindowManager();
                foreach( var activeKeyboard in KeyboardContext.Service.Keyboards.Actives )
                {
                    var subscriber = new WindowManagerSubscriber( WindowManager, WindowBinder );
                    var vm = new VMContextActiveKeyboard( activeKeyboard.Name, Context, KeyboardContext.Service.Keyboards.Context, Config, _noFocusWindowManager.NoFocusWindowThreadDispatcher );
                    var skin = _noFocusWindowManager.CreateNoFocusWindow<SkinWindow>( () => new SkinWindow
                    {
                        DataContext = vm
                    } );
                    SkinInfo skinInfo = new SkinInfo( skin, vm, _noFocusWindowManager.NoFocusWindowThreadDispatcher, subscriber );
                    _skins.Add( activeKeyboard.Name, skinInfo );
                    
                    SubscribeToWindowManager( skinInfo );
                    skinInfo.Dispatcher.BeginInvoke( new Action( () => skinInfo.Skin.Show() ) );
                }
            }
        }

        public void Stop()
        {
            foreach( var skin in _skins.Values )
            {
                skin.Subscriber.Unsubscribe();
                skin.ViewModel.Dispose();
                skin.Dispatcher.BeginInvoke( (Action)(() =>
                {
                    skin.Skin.Close();
                }) );
            }
            _skins.Clear();
        }

        public void Teardown()
        {
            if( _noFocusWindowManager != null )
            {
                //TODO : remove when the NoFocusWindowManager is exported to a service.
                //Then register the Shutdown call to the ApplicationExiting event.
                _noFocusWindowManager.Shutdown();
            }
        }
        
        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        void SubscribeToWindowManager( SkinInfo skinInfo )
        {
            skinInfo.Dispatcher.BeginInvoke( new Action( () =>
            {
                skinInfo.Subscriber.Subscribe( skinInfo.ViewModel.KeyboardVM.Keyboard.Name, skinInfo.Skin );
            } ) );
        }

        #endregion
    }
}
