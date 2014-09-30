#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\KeyboardDisplayer.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WindowManager.Model;
using CK.Windows;
using CommonServices;
using CommonServices.Accessibility;
using HighlightModel;
using Host.Services;
using SimpleSkin.ViewModels;

namespace SimpleSkin
{
    [Plugin( PluginIdString, PublicName = PluginPublicName, Version = PluginVersion,
       Categories = new string[] { "Visual", "Accessibility" } )]
    public class KeyboardDisplayer : IPlugin
    {
        #region Plugin description

        const string PluginPublicName = "Keyboard Displayer";
        const string PluginIdString = "{D173E013-2491-4491-BF3E-CA2F8552B5EB}";
        const string PluginVersion = "1.0.0";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ISharedData SharedData { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        [ConfigurationAccessor( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" )] //MainKeyboardManager
        public IPluginConfigAccessor Config { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ITopMostService> TopMostService { get; set; }

        [RequiredService]
        public INotificationService Notification { get; set; }

        class SkinInfo
        {
            public SkinInfo( SkinWindow window, VMContextActiveKeyboard vm, Dispatcher d, WindowManagerSubscriber sub )
            {
                Skin = window;
                ViewModel = vm;
                Dispatcher = d;
                Subscriber = sub;
                NameKeyboard = vm.KeyboardVM.Keyboard.Name;
            }

            public readonly WindowManagerSubscriber Subscriber;

            public readonly SkinWindow Skin;

            public readonly VMContextActiveKeyboard ViewModel;

            public readonly Dispatcher Dispatcher;

            /// <summary>
            /// must be updated manual
            /// </summary>
            public string NameKeyboard;

            public bool IsClosing;
        }

        public NoFocusManager NoFocusManager { get { return NoFocusManager.Default; } }

        IDictionary<string, SkinInfo> _skins;

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            _skins = new Dictionary<string, SkinInfo>();
            return true;
        }

        public void Start()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( KeyboardContext.Service.Keyboards.Actives.Count == 0 )
            {
                ShowNoActiveKeyboardNotification();
            }
            else
            {
                foreach( var activeKeyboard in KeyboardContext.Service.Keyboards.Actives )
                {
                    InitializeActiveWindow( activeKeyboard );
                }
            }
            RegisterEvents();
        }

        public void Stop()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            UnregisterEvents();

            foreach( var skin in _skins.Values )
            {
                UninitializeActiveWindows( skin );
            }

            _skins.Clear();
        }

        public void Teardown()
        {
        }

        #endregion

        #region WindowManager and WindowBinder Members

        void Subscribe( SkinInfo skinInfo )
        {
            skinInfo.Subscriber.Subscribe( skinInfo.NameKeyboard, skinInfo.Skin );
        }

        void Unsubscribe( SkinInfo skinInfo )
        {
            skinInfo.Subscriber.Unsubscribe();
        }

        #endregion WindowManager and WindowBidner Members

        #region TopMostService Methods

        void RegisterTopMostService( SkinInfo skinInfo )
        {
            if( TopMostService.Status == InternalRunningStatus.Started )
            {
                TopMostService.Service.RegisterTopMostElement( "10", skinInfo.Skin );
            }
        }

        void UnregisterTopMostService( SkinInfo skinInfo )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( TopMostService.Status == InternalRunningStatus.Started )
            {
                TopMostService.Service.UnregisterTopMostElement( skinInfo.Skin );
            }
        }

        void OnTopMostServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                ForEachSkin( s => RegisterTopMostService( s ) );
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                ForEachSkin( s => UnregisterTopMostService( s ) );
            }
        }

        #endregion TopMostService Methods

        #region Windows Initialization

        void InitializeActiveWindow( IKeyboard keyboard )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            var vm = new VMContextActiveKeyboard( NoFocusManager, keyboard.Name, Context, KeyboardContext.Service.Keyboards.Context, Config, SharedData );
            var subscriber = new WindowManagerSubscriber( WindowManager, WindowBinder );

            var skin = NoFocusManager.CreateNoFocusWindow<SkinWindow>( nfm => new SkinWindow( nfm )
            {
                DataContext = vm
            } );

            SkinInfo skinInfo = new SkinInfo( skin, vm, NoFocusManager.NoFocusDispatcher, subscriber );
            _skins.Add( keyboard.Name, skinInfo );

            skinInfo.Dispatcher.Invoke( (Action)(() =>
            {
                skinInfo.Skin.Show();
                skinInfo.Skin.ShowInTaskbar = false;
            }) );

            Subscribe( skinInfo );
            RegisterHighlighter( skinInfo );
            RegisterTopMostService( skinInfo );

            RegisterSkinEvents( skinInfo );
        }

        // when we call this function, we must remove the skin from the dictionary 
        void UninitializeActiveWindows( SkinInfo skin )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            UnregisterSkinEvents( skin );
            Unsubscribe( skin );
            UnregisterFromHighlighter( skin );
            UnregisterTopMostService( skin );

            skin.ViewModel.Dispose();

            //temporary 03/03/2014
            if( !skin.IsClosing )
            {
                skin.IsClosing = true;
                skin.Skin.Dispatcher.Invoke( (Action)(() =>
                {
                    skin.Skin.Close();
                }) );
            }
        }

        #endregion Windows Initialization

        #region Hightlight Methods

        void OnHighlighterServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( e.Current == InternalRunningStatus.Started )
            {
                ForEachSkin( s => RegisterHighlighter( s ) );
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                ForEachSkin( s => UnregisterFromHighlighter( s ) );
            }
        }

        private void UnregisterFromHighlighter( SkinInfo skinInfo )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( Highlighter.Status.IsStartingOrStarted )
            {
                Highlighter.Service.UnregisterTree( skinInfo.NameKeyboard, skinInfo.ViewModel.KeyboardVM );
            }
        }

        private void RegisterHighlighter( SkinInfo skinInfo )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                Highlighter.Service.RegisterTree( skinInfo.NameKeyboard, skinInfo.NameKeyboard,
                    new ExtensibleHighlightableElementProxy( skinInfo.NameKeyboard, skinInfo.ViewModel.KeyboardVM, true ) );
            }
        }

        #endregion

        #region Register Unregister Events

        private void RegisterEvents()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            KeyboardContext.Service.Keyboards.KeyboardActivated += OnKeyboardActivated;
            KeyboardContext.Service.Keyboards.KeyboardDeactivated += OnKeyboardDeactivated;
            KeyboardContext.Service.Keyboards.KeyboardDestroyed += OnKeyboardDestroyed;
            KeyboardContext.Service.Keyboards.KeyboardCreated += OnKeyboardCreated;
            KeyboardContext.Service.Keyboards.KeyboardRenamed += OnKeyboardRenamed;

            TopMostService.ServiceStatusChanged += OnTopMostServiceStatusChanged;
            Highlighter.ServiceStatusChanged += OnHighlighterServiceStatusChanged;
        }

        private void RegisterSkinEvents( SkinInfo skinInfo )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            //temporary 03/03/2014
            skinInfo.Skin.StateChanged += OnStateChanged;
            skinInfo.Skin.Closing += new CancelEventHandler( OnWindowClosing );
        }

        private void UnregisterEvents()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            KeyboardContext.Service.Keyboards.KeyboardActivated -= OnKeyboardActivated;
            KeyboardContext.Service.Keyboards.KeyboardDeactivated -= OnKeyboardDeactivated;
            KeyboardContext.Service.Keyboards.KeyboardDestroyed -= OnKeyboardDestroyed;
            KeyboardContext.Service.Keyboards.KeyboardCreated -= OnKeyboardCreated;
            KeyboardContext.Service.Keyboards.KeyboardRenamed -= OnKeyboardRenamed;

            TopMostService.ServiceStatusChanged -= OnTopMostServiceStatusChanged;
            Highlighter.ServiceStatusChanged -= OnHighlighterServiceStatusChanged;

            ForEachSkin( UnregisterSkinEvents );
        }

        private void UnregisterSkinEvents( SkinInfo skinInfo )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            //temporary 03/03/2014
            skinInfo.Skin.StateChanged -= OnStateChanged;

            skinInfo.Skin.Closing -= new CancelEventHandler( OnWindowClosing );
        }

        #endregion Register Unregister Events

        #region OnXXXX

        void OnKeyboardDeactivated( object sender, KeyboardEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            Debug.Assert( _skins.ContainsKey( e.Keyboard.Name ) );

            UninitializeActiveWindows( _skins[e.Keyboard.Name] );
            _skins.Remove( _skins[e.Keyboard.Name].ViewModel.KeyboardVM.Keyboard.Name );
        }

        void OnKeyboardActivated( object sender, KeyboardEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            InitializeActiveWindow( e.Keyboard );
        }

        void OnKeyboardRenamed( object sender, KeyboardRenamedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            SkinInfo skin;
            if( _skins.TryGetValue( e.PreviousName, out skin ) )
            {
                _skins.Remove( e.PreviousName );
                UnregisterFromHighlighter( skin );
                Unsubscribe( skin );

                skin.NameKeyboard = e.Keyboard.Name;
                _skins.Add( skin.NameKeyboard, skin );
                RegisterHighlighter( skin );
                Subscribe( skin );

            }
        }

        void OnKeyboardDestroyed( object sender, KeyboardEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( e.Keyboard.IsActive )
            {
                OnKeyboardDeactivated( sender, e );
            }
        }

        void OnKeyboardCreated( object sender, KeyboardEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( e.Keyboard.IsActive && !_skins.ContainsKey( e.Keyboard.Name ) )
            {
                OnKeyboardActivated( sender, e );
            }
        }

        #region temporary 03/03/2014

        void OnStateChanged( object sender, EventArgs e )
        {
            SkinWindow window = sender as SkinWindow;
            if( window != null )
            {
                WindowState state = window.WindowState;

                NoFocusManager.ExternalDispatcher.BeginInvoke( (Action)(() =>
                {
                    if( state == WindowState.Minimized )
                    {
                        if( Highlighter.Status == InternalRunningStatus.Started )
                        {
                            ForEachSkin( UnregisterFromHighlighter );
                        }
                    }
                    else if( state == WindowState.Normal )
                    {
                        RestoreSkin();
                    }
                }) );
            }
        }

        /// <summary>
        /// Hides the keyboard's MiniView and shows the keyboard
        /// </summary>
        public void RestoreSkin()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            ForEachSkin( RegisterHighlighter );
        }

        #endregion temporary

        void OnWindowClosing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            SkinWindow sw = sender as SkinWindow;
            Debug.Assert( sw != null );

            if( !_skins.Values.Single( s => s.Skin == sw ).IsClosing )
                e.Cancel = true;
        }

        #endregion

        #region Helper

        /// <summary>
        /// Calls the action for each skin.
        /// As skins can belong to different dispatchers, you may specify whether it should make sure the call is made through each skin Dispatcher.
        /// </summary>
        /// <param name="action">the action to call on each skin</param>
        /// <param name="dispatch">whether this aciton should be called on a skin's own dispatcher</param>
        /// <param name="synchronous">Only taken into account if dispatch == true</param>
        private void ForEachSkin( Action<SkinInfo> action )
        {
            foreach( var skin in _skins.Values )
            {
                action( skin );
            }
        }

        private void ShowNoActiveKeyboardNotification()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( Notification != null )
            {
                Notification.ShowNotification( PluginId.UniqueId, "Aucun clavier n'est actif",
                    "Aucun clavier n'est actif, veuillez activer un clavier.", 1000, NotificationTypes.Warning );
            }
        }

        #endregion Helper
    }
}
