#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor.UI\TextualContextPreview.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin;
using CK.WindowManager.Model;
using CK.WordPredictor.Model;
using CK.WordPredictor.UI.ViewModels;

namespace CK.WordPredictor.UI
{

    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion, Categories = new string[] { "Prediction", "Visual" } )]
    public class TextualContextPreview : IPlugin
    {
        #region Plugin description

        const string PluginGuidString = "{E9D02BE8-B1CA-4057-8E74-2A89C411565C}";
        const string PluginVersion = "1.0.0";
        const string PluginPublicName = "TextualContext - Echo";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

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
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );

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
