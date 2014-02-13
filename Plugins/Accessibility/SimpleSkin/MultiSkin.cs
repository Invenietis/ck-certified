using System;
using System.Collections.Generic;
using System.Windows.Threading;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WindowManager.Model;
using CommonServices.Accessibility;
using HighlightModel;
using SimpleSkin.ViewModels;
using System.Linq;

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

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IHighlighterService> HighlighterService { get; set; }

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
        IDictionary<IKeyboard,IHighlightableElement> _highlightableElementRegistered;

        public bool Setup( IPluginSetupInfo info )
        {
            _skins = new Dictionary<string, SkinInfo>();
            _highlightableElementRegistered = new Dictionary<IKeyboard, IHighlightableElement>();
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

                    // ToDoF change static implementation
                    if( activeKeyboard.Name == "Prediction" )
                    {
                        VMKeyboardSimple vmk = vm.Keyboards.Where( VMK => { return VMK.Keyboard.Name == "Prediction"; } ).FirstOrDefault();
                        VMZoneSimple vmz = vmk.Zones.Where( VMZ => { return VMZ.Name == "Prediction"; } ).FirstOrDefault();
                        HighlighterService.Service.RegisterInRegisteredElementAt( "Keyboard", "Azerty", ChildPosition.Pre, vmz );
                        _highlightableElementRegistered.Add( vmk.Keyboard, vmz );
                    }
                }
            }
        }

        public void Stop()
        {
            // ToDoF change static implementation
            HighlighterService.Service.UnregisterInRegisteredElement( "Keyboard", "Azerty", ChildPosition.Pre, _highlightableElementRegistered.Values.FirstOrDefault() );

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
