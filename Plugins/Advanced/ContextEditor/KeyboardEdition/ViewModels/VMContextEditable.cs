#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\EditableSkin\ViewModels\VMContextEditable.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using CK.Context;
using System;
using CK.Plugin;
using CommonServices;
using System.Collections.Generic;
using System.Windows.Forms;
using KeyboardEditor.Tools;
using System.ComponentModel;
using System.Collections.ObjectModel;
using ProtocolManagerModel;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public enum KeyboardEditorMouseEvent
    {
        MouseMove = 1,
        PointerButtonUp = 2,
        PointerButtonDown = 3
    }

    public enum ModeTypes
    {
        None = 0,//When displaying the panel to choose between Mode and Layout
        Mode = 1,
        Layout = 2
    }

    public class VMContextEditable : VMBase, IDisposable
    {
        public const int down = 40;
        public const int left = 37;
        public const int right = 39;
        public const int suppr = 46;
        public const int up = 38;
        IPluginConfigAccessor _config;
        IContext _ctx;
        VMKeyboardEditable _currentKeyboard;
        ModeTypes _currentlyDisplayedModeType = ModeTypes.None;
        Dictionary<object, VMContextElementEditable> _dic;
        EventHandler<CurrentKeyboardChangedEventArgs> _evCurrentKeyboardChanged;

        EventHandler<KeyboardEventArgs> _evKeyboardCreated;
        EventHandler<KeyboardEventArgs> _evKeyboardDestroyed;
        PropertyChangedEventHandler _evUserConfigurationChanged;
        IKeyboardContext _kbctx;
        ObservableCollection<VMKeyboardEditable> _keyboards;
        IKeyboardEditorRoot _root;
        public VMContextEditable( IKeyboardEditorRoot root, IKeyboard keyboardToEdit, IPluginConfigAccessor config, IPluginConfigAccessor skinConfiguration )
        {
            if( keyboardToEdit == null ) throw new ArgumentException( "The keyboardToEdit must not be null" );

            _dic = new Dictionary<object, VMContextElementEditable>();
            _keyboards = new ObservableCollection<VMKeyboardEditable>();
            _kbctx = root.KeyboardContext.Service.Keyboards.Context;
            _ctx = root.Context;
            _config = config;
            _root = root;

            SkinConfiguration = skinConfiguration;

            DefaultImages = new Dictionary<string, string>();
            GetDefaultImages();

            KeyboardVM = CreateKeyboard( keyboardToEdit );

            _dic.Add( keyboardToEdit, _currentKeyboard );
            _keyboards.Add( _currentKeyboard );

            RegisterEvents();
        }

        internal void AddDefaultImage( string key, string value )
        {
            DefaultImages.Add( key, value );
            OnPropertyChanged( "DefaultImages" );
        }

        private void GetDefaultImages()
        {
            string pathRoot = "pack://application:,,,/SimpleSkin;component/Images";
            string arrowRoot = pathRoot + "/arrows";
            string clickRoot = pathRoot + "/clics";
            DefaultImages.Add( Images.Enter, pathRoot + "/enter.png" );
            DefaultImages.Add( Images.Exit, pathRoot + "/exit.png" );
            DefaultImages.Add( Images.Eye, pathRoot + "/eye.png" );
            DefaultImages.Add( Images.Help, pathRoot + "/help.png" );
            DefaultImages.Add( Images.Keyboard, pathRoot + "/kb.png" );
            DefaultImages.Add( Images.Caps, pathRoot + "/maj.png" );
            DefaultImages.Add( Images.Menu, pathRoot + "/menu.png" );
            DefaultImages.Add( Images.MouseKeyboard, pathRoot + "/mousekb.png" );
            DefaultImages.Add( Images.Directions, pathRoot + "/move.png" );
            DefaultImages.Add( Images.Padlock, pathRoot + "/padlock.png" );
            DefaultImages.Add( Images.Suppr, pathRoot + "/retarr.png" );
            DefaultImages.Add( Images.Tab, pathRoot + "/tab.png" );
            DefaultImages.Add( Images.WindowsLogo, pathRoot + "/windows.png" );

            DefaultImages.Add( Images.BoldDownArrow, arrowRoot + "/bottom.png" );
            DefaultImages.Add( Images.BoldDownLeftArrow, arrowRoot + "/left-bottom.png" );
            DefaultImages.Add( Images.BoldLeftArrow, arrowRoot + "/left.png" );
            DefaultImages.Add( Images.BoldDownRightArrow, arrowRoot + "/right-bottom.png" );
            DefaultImages.Add( Images.BoldRightArrow, arrowRoot + "/right.png" );
            DefaultImages.Add( Images.BoldTopLeftArrow, arrowRoot + "/top-left.png" );
            DefaultImages.Add( Images.BoldTopRightArrow, arrowRoot + "/top-right.png" );
            DefaultImages.Add( Images.BoldUpArrow, arrowRoot + "/top.png" );

            DefaultImages.Add( Images.DoubleLeftClick, clickRoot + "/doubleleftclick.png" );
            DefaultImages.Add( Images.DragDrop, clickRoot + "/dragdrop.png" );
            DefaultImages.Add( Images.LeftClick, clickRoot + "/leftclick.png" );
            DefaultImages.Add( Images.RightClick, clickRoot + "/rightclick.png" );
        }

        #region Properties

        VMContextElementEditable _selectedElement;

        public IPluginConfigAccessor Config { get { return _config; } }

        public IContext Context { get { return _ctx; } }

        /// <summary>
        /// The fact that we are displaying the LayoutKeyMode or the KeyMode, on a IKey edition panel must be handled at a higher level.
        /// This property gets whether we should display the KeyMode or the LayoutKeyMode panel.
        /// </summary>
        public ModeTypes CurrentlyDisplayedModeType
        {
            get { return _currentlyDisplayedModeType; }
            set
            {
                if( _currentlyDisplayedModeType != value )
                {
                    _currentlyDisplayedModeType = value;

                    if( SelectedElement is VMKeyEditable )
                    {
                        ((VMKeyEditable)SelectedElement).LayoutKeyModeVM.TriggerPropertyChanged( "IsSelected" );
                        ((VMKeyEditable)SelectedElement).KeyModeVM.TriggerPropertyChanged( "IsSelected" );
                    }
                    else if( SelectedElement is VMKeyModeEditable ) ((VMKeyModeEditable)SelectedElement).TriggerPropertyChanged( "IsSelected" );
                    else if( SelectedElement is VMLayoutKeyModeEditable ) ((VMLayoutKeyModeEditable)SelectedElement).TriggerPropertyChanged( "IsSelected" );

                    OnPropertyChanged( "CurrentlyDisplayedModeType" );
                }
            }
        }

        public Dictionary<string, string> DefaultImages { get; private set; }

        public IKeyboardContext KeyboardContext { get { return _kbctx; } }

        public ObservableCollection<VMKeyboardEditable> Keyboards { get { return _keyboards; } }
        public VMKeyboardEditable KeyboardVM
        {
            get { return _currentKeyboard; }
            set { _currentKeyboard = value; OnPropertyChanged( "KeyboardVM" ); }
        }

        //TODO : check where this is used and remove it
        public IKeyboardContext Model { get { return KeyboardContext; } }

        /// <summary>
        /// Gets the pointer device driver, can be used to hook events
        /// </summary>
        public IService<IPointerDeviceDriver> PointerDeviceDriver { get { return _root.PointerDeviceDriver; } }

        public VMContextElementEditable SelectedElement
        {
            get
            {
                if( _selectedElement == null )
                {
                    _selectedElement = KeyboardVM;
                    _selectedElement.IsSelected = true;
                }
                return _selectedElement;
            }
            set
            {
                if( _selectedElement != value && value != null )
                {
                    if( _selectedElement != null )
                    {
                        _previouslySelectedElement = _selectedElement;
                        _selectedElement.IsSelected = false;
                    }
                    _selectedElement = value;
                    _selectedElement.IsSelected = true;
                    OnPropertyChanged( "SelectedElement" );
                }
            }
        }

        VMContextElementEditable _previouslySelectedElement;
        public VMContextElementEditable PreviouslySelectedElement
        {
            get { return _previouslySelectedElement; }
            private set { _previouslySelectedElement = value; }
        }

        public IPluginConfigAccessor SkinConfiguration { get; set; }
        internal IService<IProtocolEditorsManager> ProtocolManagerService { get { return _root.ProtocolManagerService; } }
        #endregion

        #region OnXXX
        public void Dispose()
        {
            foreach( var item in _dic )
            {
                item.Value.Dispose();
            }

            //foreach( var keyboard in _keyboards )
            //{
            //    keyboard.Dispose();
            //}

            _keyboards.Clear();
            _dic.Clear();

            UnregisterEvents();
        }

        internal void OnModelDestroy( object m )
        {
            VMContextElementEditable vm;
            if( _dic.TryGetValue( m, out vm ) )
            {
                vm.Dispose();
                _dic.Remove( m );
            }
        }

        private void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            //Do nothing, we are not bound to the current keyboard of the keyboard context
        }

        private void OnKeyboardCreated( object sender, CK.Keyboard.Model.KeyboardEventArgs e )
        {
            VMKeyboardEditable k = CreateKeyboard( e.Keyboard );
            _dic.Add( e.Keyboard, k );
            _keyboards.Add( k );
        }

        private void OnKeyboardDestroyed( object sender, CK.Keyboard.Model.KeyboardEventArgs e )
        {
            _keyboards.Remove( Obtain( e.Keyboard ) );
            OnModelDestroy( e.Keyboard );
        }

        private void OnKeyDown( object sender, HookInvokedEventArgs e )
        {
            Keys key = (Keys)(((int)e.LParam >> 16) & 0xFFFF);
            int modifier = (int)e.LParam & 0xFFFF;
            int delta = modifier == Constants.SHIFT ? 10 : 1;
            SelectedElement.OnKeyDownAction( (int)key, delta );
        }

        private void OnMouseEventTriggered( KeyboardEditorMouseEvent eventType, PointerDeviceEventArgs args )
        {
            if( SelectedElement as VMKeyEditable != null ) (SelectedElement as VMKeyEditable).TriggerMouseEvent( eventType, args );
            else if( SelectedElement as VMKeyModeBase != null ) ((SelectedElement as VMKeyModeBase).Parent as VMKeyEditable).TriggerMouseEvent( eventType, args );
            else if( SelectedElement as VMZoneEditable != null )
            {
                foreach( var key in (SelectedElement as VMZoneEditable).Keys )
                {
                    key.TriggerMouseEvent( eventType, args );
                }
            }
            else if( SelectedElement as VMKeyboardEditable != null )
            {
                foreach( var zone in (SelectedElement as VMKeyboardEditable).Zones )
                {
                    foreach( var key in zone.Keys )
                    {
                        key.TriggerMouseEvent( eventType, args );
                    }
                }
            }
        }

        //For now, the VMContext is the one handling mouse triggers directly to the ones that need it.
        private void OnMouseMove( object sender, PointerDeviceEventArgs args )
        {
            OnMouseEventTriggered( KeyboardEditorMouseEvent.MouseMove, args );
        }

        //For now, the VMContext is the one handling mouse triggers directly to the ones that need it.
        private void OnPointerButtonUp( object sender, PointerDeviceEventArgs args )
        {
            OnMouseEventTriggered( KeyboardEditorMouseEvent.PointerButtonUp, args );
        }

        private void OnUserConfigurationChanged( object sender, PropertyChangedEventArgs e )
        {
            //If the CurrentContext has changed, but not because a new context has been loaded (happens when the userConf if changed but the context is kept the same).
            if( e.PropertyName == "CurrentContextProfile" )
            {
                OnPropertyChanged( "KeyboardVM" );
            }
        }

        void RegisterEvents()
        {
            if( PointerDeviceDriver.Status == InternalRunningStatus.Started )
            {
                PointerDeviceDriver.Service.PointerMove += OnMouseMove;
                PointerDeviceDriver.Service.PointerButtonUp += OnPointerButtonUp;
            }

            _evKeyboardCreated = new EventHandler<KeyboardEventArgs>( OnKeyboardCreated );
            _evCurrentKeyboardChanged = new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
            _evKeyboardDestroyed = new EventHandler<KeyboardEventArgs>( OnKeyboardDestroyed );
            _evUserConfigurationChanged = new PropertyChangedEventHandler( OnUserConfigurationChanged );

            _kbctx.Keyboards.KeyboardCreated += _evKeyboardCreated;
            _kbctx.CurrentKeyboardChanged += _evCurrentKeyboardChanged;
            _kbctx.Keyboards.KeyboardDestroyed += _evKeyboardDestroyed;
            _ctx.ConfigManager.UserConfiguration.PropertyChanged += _evUserConfigurationChanged;

            //KL
            //if( KeyboardDriver.Status == InternalRunningStatus.Started )
            //{
            //    KeyboardDriver.Service.KeyDown += OnKeyDown;
            //    KeyboardDriver.Service.RegisterCancellableKey( suppr );
            //    KeyboardDriver.Service.RegisterCancellableKey( up );
            //    KeyboardDriver.Service.RegisterCancellableKey( down );
            //    KeyboardDriver.Service.RegisterCancellableKey( left );
            //    KeyboardDriver.Service.RegisterCancellableKey( right );
            //}

            _root.HookInvoqued += OnKeyDown;
        }

        void UnregisterEvents()
        {
            if( PointerDeviceDriver != null && PointerDeviceDriver.Status != InternalRunningStatus.Stopped )
            {
                PointerDeviceDriver.Service.PointerMove -= OnMouseMove;
                PointerDeviceDriver.Service.PointerButtonUp -= OnPointerButtonUp;
            }

            _kbctx.Keyboards.KeyboardCreated -= _evKeyboardCreated;
            _kbctx.CurrentKeyboardChanged -= _evCurrentKeyboardChanged;
            _kbctx.Keyboards.KeyboardDestroyed -= _evKeyboardDestroyed;
            _ctx.ConfigManager.UserConfiguration.PropertyChanged -= _evUserConfigurationChanged;

            //KL
            //if( KeyboardDriver != null && KeyboardDriver.Status != InternalRunningStatus.Stopped )
            //{
            //    KeyboardDriver.Service.KeyDown -= OnKeyDown;
            //    KeyboardDriver.Service.UnregisterCancellableKey( suppr );
            //    KeyboardDriver.Service.UnregisterCancellableKey( up );
            //    KeyboardDriver.Service.UnregisterCancellableKey( down );
            //    KeyboardDriver.Service.UnregisterCancellableKey( left );
            //    KeyboardDriver.Service.UnregisterCancellableKey( right );
            //}

            _root.HookInvoqued -= OnKeyDown;
        }
        #endregion

        #region Components Create & Obtain Methods

        VMCommand<VMContextElementEditable> _selectCommand;
        public VMCommand<VMContextElementEditable> SelectCommand
        {
            get
            {
                if( _selectCommand == null )
                {
                    _selectCommand = new CK.WPF.ViewModel.VMCommand<VMContextElementEditable>( ( elem ) =>
                    {
                        if( elem is VMKeyEditable ) CurrentlyDisplayedModeType = ModeTypes.None;
                        SelectedElement = elem;
                    } );
                }
                return _selectCommand;
            }
        }

        public VMKeyboardEditable Obtain( IKeyboard keyboard )
        {
            VMKeyboardEditable k = FindViewModel<VMKeyboardEditable>( keyboard );
            if( k == null ) throw new Exception( "Context mismatch." );
            return k;
        }

        public VMZoneEditable Obtain( IZone zone )
        {
            VMZoneEditable z = FindViewModel<VMZoneEditable>( zone );
            if( z == null )
            {
                if( zone.Context != _kbctx )
                    throw new Exception( "Context mismatch." );
                z = CreateZone( zone );
                _dic.Add( zone, z );
            }
            return z;
        }

        public VMKeyEditable Obtain( IKey key )
        {
            VMKeyEditable k = FindViewModel<VMKeyEditable>( key );
            if( k == null )
            {
                if( key.Context != _kbctx )
                    throw new Exception( "Context mismatch." );
                k = CreateKey( key );
                _dic.Add( key, k );
            }
            return k;
        }

        public VMKeyModeEditable Obtain( IKeyMode keyMode )
        {
            VMKeyModeEditable km = FindViewModel<VMKeyModeEditable>( keyMode );
            if( km == null )
            {
                if( keyMode.Context != _kbctx )
                    throw new Exception( "Context mismatch." );
                km = CreateKeyMode( keyMode );
                if( km != null ) //the viewmodel can be null, if the implementation doesn't use this level of objects (SimpleSkin doesn't use these templates)
                    _dic.Add( keyMode, km );
            }
            return km;
        }

        public VMLayoutKeyModeEditable Obtain( ILayoutKeyMode layoutKeyMode )
        {
            VMLayoutKeyModeEditable lkm = FindViewModel<VMLayoutKeyModeEditable>( layoutKeyMode );
            if( lkm == null )
            {
                if( layoutKeyMode.Context != _kbctx )
                    throw new Exception( "Context mismatch." );
                lkm = CreateLayoutKeyMode( layoutKeyMode );
                if( lkm != null ) //the viewmodel can be null, if the implementation doesn't use this level of objects (SimpleSkin doesn't use these templates)
                    _dic.Add( layoutKeyMode, lkm );
            }
            return lkm;
        }

        protected VMKeyEditable CreateKey( IKey k )
        {
            VMKeyEditable vmKey = new VMKeyEditable( this, k );
            return vmKey;
        }

        protected VMKeyboardEditable CreateKeyboard( IKeyboard kb )
        {
            VMKeyboardEditable vmKeyboard = new VMKeyboardEditable( this, kb );
            return vmKeyboard;
        }

        protected VMKeyModeEditable CreateKeyMode( IKeyMode keyMode )
        {
            VMKeyModeEditable vm = new VMKeyModeEditable( this, keyMode );
            return vm;
        }

        protected VMLayoutKeyModeEditable CreateLayoutKeyMode( ILayoutKeyMode layoutKeyMode )
        {
            VMLayoutKeyModeEditable vm = new VMLayoutKeyModeEditable( this, layoutKeyMode );
            return vm;
        }
        protected VMZoneEditable CreateZone( IZone z )
        {
            VMZoneEditable vmZone = new VMZoneEditable( this, z );
            return vmZone;
        }
        T FindViewModel<T>( object m )
            where T : VMContextElementEditable
        {
            VMContextElementEditable vm;
            _dic.TryGetValue( m, out vm );
            return (T)vm;
        }
        #endregion

        public static readonly List<char> FontAwesomeLetters = new List<char>() {
                '\xf042', 
                '\xf170', 
                '\xf037', 
                '\xf039', 
                '\xf036', 
                '\xf038', 
                '\xf0f9', 
                '\xf13d', 
                '\xf17b', 
                '\xf103', 
                '\xf100', 
                '\xf101', 
                '\xf102', 
                '\xf107', 
                '\xf104', 
                '\xf105', 
                '\xf106', 
                '\xf179', 
                '\xf187', 
                '\xf0ab', 
                '\xf0a8', 
                '\xf01a', 
                '\xf190', 
                '\xf18e', 
                '\xf01b', 
                '\xf0a9', 
                '\xf0aa', 
                '\xf063', 
                '\xf060', 
                '\xf061', 
                '\xf062', 
                '\xf047', 
                '\xf0b2', 
                '\xf07e', 
                '\xf07d', 
                '\xf069', 
                '\xf1b9', 
                '\xf04a', 
                '\xf05e', 
                '\xf19c', 
                '\xf080', 
                '\xf02a', 
                '\xf0c9', 
                '\xf0fc', 
                '\xf1b4', 
                '\xf1b5', 
                '\xf0f3', 
                '\xf0a2', 
                '\xf171', 
                '\xf172', 
                '\xf15a', 
                '\xf032', 
                '\xf0e7', 
                '\xf1e2', 
                '\xf02d', 
                '\xf02e', 
                '\xf097', 
                '\xf0b1', 
                '\xf15a', 
                '\xf188', 
                '\xf1ad', 
                '\xf0f7', 
                '\xf0a1', 
                '\xf140', 
                '\xf1ba', 
                '\xf073', 
                '\xf133', 
                '\xf030', 
                '\xf083', 
                '\xf1b9', 
                '\xf0d7', 
                '\xf0d9', 
                '\xf0da', 
                '\xf150', 
                '\xf191', 
                '\xf152', 
                '\xf151', 
                '\xf0d8', 
                '\xf0a3', 
                '\xf0c1', 
                '\xf127', 
                '\xf00c', 
                '\xf058', 
                '\xf05d', 
                '\xf14a', 
                '\xf046', 
                '\xf13a', 
                '\xf137', 
                '\xf138', 
                '\xf139', 
                '\xf078', 
                '\xf053', 
                '\xf054', 
                '\xf077', 
                '\xf1ae', 
                '\xf111', 
                '\xf10c', 
                '\xf1ce', 
                '\xf1db', 
                '\xf0ea', 
                '\xf017', 
                '\xf0c2', 
                '\xf0ed', 
                '\xf0ee', 
                '\xf157', 
                '\xf121', 
                '\xf126', 
                '\xf1cb', 
                '\xf0f4', 
                '\xf013', 
                '\xf085', 
                '\xf0db', 
                '\xf075', 
                '\xf0e5', 
                '\xf086', 
                '\xf0e6', 
                '\xf14e', 
                '\xf066', 
                '\xf0c5', 
                '\xf09d', 
                '\xf125', 
                '\xf05b', 
                '\xf13c', 
                '\xf1b2', 
                '\xf1b3', 
                '\xf0c4', 
                '\xf0f5', 
                '\xf0e4', 
                '\xf1c0', 
                '\xf03b', 
                '\xf1a5', 
                '\xf108', 
                '\xf1bd', 
                '\xf1a6', 
                '\xf155', 
                '\xf192', 
                '\xf019', 
                '\xf17d', 
                '\xf16b', 
                '\xf1a9', 
                '\xf044', 
                '\xf052', 
                '\xf141', 
                '\xf142', 
                '\xf1d1', 
                '\xf0e0', 
                '\xf003', 
                '\xf199', 
                '\xf12d', 
                '\xf153', 
                '\xf153', 
                '\xf0ec', 
                '\xf12a', 
                '\xf06a', 
                '\xf071', 
                '\xf065', 
                '\xf08e', 
                '\xf14c', 
                '\xf06e', 
                '\xf070', 
                '\xf09a', 
                '\xf082', 
                '\xf049', 
                '\xf050', 
                '\xf1ac', 
                '\xf182', 
                '\xf0fb', 
                '\xf15b', 
                '\xf1c6', 
                '\xf1c7', 
                '\xf1c9', 
                '\xf1c3', 
                '\xf1c5', 
                '\xf1c8', 
                '\xf016', 
                '\xf1c1', 
                '\xf1c5', 
                '\xf1c5', 
                '\xf1c4', 
                '\xf1c7', 
                '\xf15c', 
                '\xf0f6', 
                '\xf1c8', 
                '\xf1c2', 
                '\xf1c6', 
                '\xf0c5', 
                '\xf008', 
                '\xf0b0', 
                '\xf06d', 
                '\xf134', 
                '\xf024', 
                '\xf11e', 
                '\xf11d', 
                '\xf0e7', 
                '\xf0c3', 
                '\xf16e', 
                '\xf0c7', 
                '\xf07b', 
                '\xf114', 
                '\xf07c', 
                '\xf115', 
                '\xf031', 
                '\xf04e', 
                '\xf180', 
                '\xf119', 
                '\xf11b', 
                '\xf0e3', 
                '\xf154', 
                '\xf1d1', 
                '\xf013', 
                '\xf085', 
                '\xf06b', 
                '\xf1d3', 
                '\xf1d2', 
                '\xf09b', 
                '\xf113', 
                '\xf092', 
                '\xf184', 
                '\xf000', 
                '\xf0ac', 
                '\xf1a0', 
                '\xf0d5', 
                '\xf0d4', 
                '\xf19d', 
                '\xf0c0', 
                '\xf0fd', 
                '\xf1d4', 
                '\xf0a7', 
                '\xf0a5', 
                '\xf0a4', 
                '\xf0a6', 
                '\xf0a0', 
                '\xf1dc', 
                '\xf025', 
                '\xf004', 
                '\xf08a', 
                '\xf1da', 
                '\xf015', 
                '\xf0f8', 
                '\xf13b', 
                '\xf03e', 
                '\xf01c', 
                '\xf03c', 
                '\xf129', 
                '\xf05a', 
                '\xf156', 
                '\xf16d', 
                '\xf19c', 
                '\xf033', 
                '\xf1aa', 
                '\xf157', 
                '\xf1cc', 
                '\xf084', 
                '\xf11c', 
                '\xf159', 
                '\xf1ab', 
                '\xf109', 
                '\xf06c', 
                '\xf0e3', 
                '\xf094', 
                '\xf149', 
                '\xf148', 
                '\xf1cd', 
                '\xf1cd', 
                '\xf1cd', 
                '\xf0eb', 
                '\xf0c1', 
                '\xf0e1', 
                '\xf08c', 
                '\xf17c', 
                '\xf03a', 
                '\xf022', 
                '\xf0cb', 
                '\xf0ca', 
                '\xf124', 
                '\xf023', 
                '\xf175', 
                '\xf177', 
                '\xf178', 
                '\xf176', 
                '\xf0d0', 
                '\xf076', 
                '\xf064', 
                '\xf112', 
                '\xf122', 
                '\xf183', 
                '\xf041', 
                '\xf136', 
                '\xf0fa', 
                '\xf11a', 
                '\xf130', 
                '\xf131', 
                '\xf068', 
                '\xf056', 
                '\xf146', 
                '\xf147', 
                '\xf10b', 
                '\xf10b', 
                '\xf0d6', 
                '\xf186', 
                '\xf19d', 
                '\xf001', 
                '\xf0c9', 
                '\xf19b', 
                '\xf03b', 
                '\xf18c', 
                '\xf1d8', 
                '\xf1d9', 
                '\xf0c6', 
                '\xf1dd', 
                '\xf0ea', 
                '\xf04c', 
                '\xf1b0', 
                '\xf040', 
                '\xf14b', 
                '\xf044', 
                '\xf095', 
                '\xf098', 
                '\xf03e', 
                '\xf03e', 
                '\xf1a7', 
                '\xf1a8', 
                '\xf1a7', 
                '\xf0d2', 
                '\xf0d3', 
                '\xf072', 
                '\xf04b', 
                '\xf144', 
                '\xf01d', 
                '\xf067', 
                '\xf055', 
                '\xf0fe', 
                '\xf196', 
                '\xf011', 
                '\xf02f', 
                '\xf12e', 
                '\xf1d6', 
                '\xf029', 
                '\xf128', 
                '\xf059', 
                '\xf10d', 
                '\xf10e', 
                '\xf1d0', 
                '\xf074', 
                '\xf1d0', 
                '\xf1b8', 
                '\xf1a1', 
                '\xf1a2', 
                '\xf021', 
                '\xf18b', 
                '\xf0c9', 
                '\xf01e', 
                '\xf112', 
                '\xf122', 
                '\xf079', 
                '\xf157', 
                '\xf018', 
                '\xf135', 
                '\xf0e2', 
                '\xf01e', 
                '\xf158', 
                '\xf09e', 
                '\xf143', 
                '\xf158', 
                '\xf158', 
                '\xf156', 
                '\xf0c7', 
                '\xf0c4', 
                '\xf002', 
                '\xf010', 
                '\xf00e', 
                '\xf1d8', 
                '\xf1d9', 
                '\xf064', 
                '\xf1e0', 
                '\xf1e1', 
                '\xf14d', 
                '\xf045', 
                '\xf132', 
                '\xf07a', 
                '\xf090', 
                '\xf08b', 
                '\xf012', 
                '\xf0e8', 
                '\xf17e', 
                '\xf198', 
                '\xf1de', 
                '\xf118', 
                '\xf0dc', 
                '\xf15d', 
                '\xf15e', 
                '\xf160', 
                '\xf161', 
                '\xf0de', 
                '\xf0dd', 
                '\xf0dd', 
                '\xf162', 
                '\xf163', 
                '\xf0de', 
                '\xf1be', 
                '\xf197', 
                '\xf110', 
                '\xf1b1', 
                '\xf1bc', 
                '\xf0c8', 
                '\xf096', 
                '\xf18d', 
                '\xf16c', 
                '\xf005', 
                '\xf089', 
                '\xf123', 
                '\xf123', 
                '\xf123', 
                '\xf006', 
                '\xf1b6', 
                '\xf1b7', 
                '\xf048', 
                '\xf051', 
                '\xf0f1', 
                '\xf04d', 
                '\xf0cc', 
                '\xf1a4', 
                '\xf1a3', 
                '\xf12c', 
                '\xf0f2', 
                '\xf185', 
                '\xf12b', 
                '\xf1cd', 
                '\xf0ce', 
                '\xf10a', 
                '\xf0e4', 
                '\xf02b', 
                '\xf02c', 
                '\xf0ae', 
                '\xf1ba', 
                '\xf1d5', 
                '\xf120', 
                '\xf034', 
                '\xf035', 
                '\xf00a', 
                '\xf009', 
                '\xf00b', 
                '\xf08d', 
                '\xf165', 
                '\xf088', 
                '\xf087', 
                '\xf164', 
                '\xf145', 
                '\xf00d', 
                '\xf057', 
                '\xf05c', 
                '\xf043', 
                '\xf150', 
                '\xf191', 
                '\xf152', 
                '\xf151', 
                '\xf014', 
                '\xf1bb', 
                '\xf181', 
                '\xf091', 
                '\xf0d1', 
                '\xf195', 
                '\xf173', 
                '\xf174', 
                '\xf195', 
                '\xf099', 
                '\xf081', 
                '\xf0e9', 
                '\xf0cd', 
                '\xf0e2', 
                '\xf19c', 
                '\xf127', 
                '\xf09c', 
                '\xf13e', 
                '\xf0dc', 
                '\xf093', 
                '\xf155', 
                '\xf007', 
                '\xf0f0', 
                '\xf0c0', 
                '\xf03d', 
                '\xf194', 
                '\xf1ca', 
                '\xf189', 
                '\xf027', 
                '\xf026', 
                '\xf028', 
                '\xf071', 
                '\xf1d7', 
                '\xf18a', 
                '\xf1d7', 
                '\xf193', 
                '\xf17a', 
                '\xf159', 
                '\xf19a', 
                '\xf0ad', 
                '\xf168', 
                '\xf169', 
                '\xf19e', 
                '\xf157', 
                '\xf167', 
                '\xf16a', 
                '\xf166' };
    }
}
