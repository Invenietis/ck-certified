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

            _evCurrentKeyboardChanged = new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
            _evUserConfigurationChanged = new PropertyChangedEventHandler( OnUserConfigurationChanged );
            _evKeyboardDestroyed = new EventHandler<KeyboardEventArgs>( OnKeyboardDestroyed );
            _evKeyboardCreated = new EventHandler<KeyboardEventArgs>( OnKeyboardCreated );

            _ctx.ConfigManager.UserConfiguration.PropertyChanged += _evUserConfigurationChanged;
            _kbctx.Keyboards.KeyboardDestroyed += _evKeyboardDestroyed;
            _kbctx.CurrentKeyboardChanged += _evCurrentKeyboardChanged;
            _kbctx.Keyboards.KeyboardCreated += _evKeyboardCreated;
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
    }
}
