using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
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
        IWindowElement _textualContext;
        WindowManagerSubscriber _subscriber;

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _vm = new TextualContextPreviewViewModel( TextualContextService );
            _window = new TextualContextPreviewWindow( _vm );
            _window.Width = 600;
            _window.Height = 300;
            _window.Show();

            _subscriber = new WindowManagerSubscriber( WindowManager, WindowBinder );
            _subscriber.OnBinderStarted = () =>
            {
                if( _textualContext != null & _me != null )
                    WindowBinder.Service.Bind( _textualContext, _me, BindingPosition.Top );
            };
            _subscriber.OnBinderStopped = () =>
            {
                if( _textualContext != null & _me != null )
                    WindowBinder.Service.Unbind( _textualContext, _me );
            };
            _subscriber.WindowRegistered = ( e ) =>
            {
                if( e.Window.Name == WindowName )
                {
                    _me = e.Window;
                    _textualContext = WindowManager.Service.GetByName( TextualContextArea.WindowName );
                }
                if( e.Window.Name == TextualContextArea.WindowName ) _textualContext = e.Window;

                _subscriber.OnBinderStarted();
            };
            _subscriber.WindowUnregistered = ( e ) =>
            {
                if( e.Window.Name == TextualContextArea.WindowName ) _subscriber.OnBinderStopped();
            };
            _subscriber.Subscribe( WindowName, _window );
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
