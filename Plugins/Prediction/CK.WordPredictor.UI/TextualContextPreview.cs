using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugins.SendInputDriver;
using CK.WindowManager.Model;
using CK.WordPredictor.Model;
using CK.WordPredictor.UI.ViewModels;

namespace CK.WordPredictor.UI
{

    [Plugin( "{E9D02BE8-B1CA-4057-8E74-2A89C411565C}", PublicName = "TextualContext - Echo", Categories = new string[] { "Prediction", "Visual" } )]
    public class TextualContextPreview : IPlugin
    {
        const string WindowName = "TextualContextPreview";

        TextualContextPreviewViewModel _vm;
        TextualContextPreviewWindow _window;

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<ITextualContextService> TextualContextService { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        IWindowElement _me;
        WindowManagerSubscriber _subscriber;

        public bool Setup( IPluginSetupInfo info )
        {
            _subscriber = new WindowManagerSubscriber( WindowManager, WindowBinder );
            return true;
        }

        public void Start()
        {
            _vm = new TextualContextPreviewViewModel( TextualContextService );
            _window = new TextualContextPreviewWindow( _vm );
            _window.Width = 600;
            _window.Height = 300;
            _window.Show();

            _subscriber.Subscribe( WindowName, _window );
            _subscriber.WindowRegistered = ( e ) =>
            {
                if( e.Window.Name == WindowName )
                {
                    _me = e.Window;
                    // Auto attached with the skin if the skin is registered
                    var textualAreaWindowElement = WindowManager.Service.GetByName( TextualContextArea.WindowName );
                    if( textualAreaWindowElement != null )
                    {
                        WindowBinder.Service.Attach( textualAreaWindowElement, _me, BindingPosition.Top );
                    }
                }
                // If the skin is registered when we are launched before it, 
                // listen to to its registration and auto-attach
                if( e.Window.Name == TextualContextArea.WindowName )
                {
                    if( _me != null ) WindowBinder.Service.Attach( e.Window, _me, BindingPosition.Top );
                }
            };
            _subscriber.WindowUnregistered = ( e ) =>
            {
                if( e.Window.Name == TextualContextArea.WindowName )
                {
                    WindowBinder.Service.Detach( _me, e.Window );
                }
            };
        }

        public void Stop()
        {
            _vm.Dispose();
            _window.Close();

            _subscriber.Unsubscribe();
        }

        public void Teardown()
        {
        }
    }
}
