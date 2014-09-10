#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\ViewModels\VMKeyEditable\VMKeyEditable.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using CK.Storage;
using CK.Windows.App;
using CK.WPF.ViewModel;
using CommonServices;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    [Flags]
    public enum FallbackVisibility
    {
        /// <summary>
        /// Enables the Fallback on the nearest <see cref="ILayoutKeyMode"/>
        /// </summary>
        FallbackOnLayout = 1,

        /// <summary>
        /// Enables the Fallback on the nearest <see cref="IKeyMode"/>
        /// </summary>
        FallbackOnKeyMode = 2
    }

    public partial class VMKeyEditable : VMContextElementEditable
    {
        Dictionary<string, ActionSequence> _actionsOnPropertiesChanged;
        VMContextEditable _context;
        ICommand _keyPressedCmd;
        ICommand _keyDownCmd;
        ICommand _keyUpCmd;
        bool _isSelected;
        IKey _key;

        public VMKeyEditable( VMContextEditable ctx, IKey k )
            : base( ctx )
        {
            //By default, we show the fallback.
            ShowFallback = FallbackVisibility.FallbackOnLayout | FallbackVisibility.FallbackOnKeyMode;

            _context = ctx;
            _key = k;

            _actionsOnPropertiesChanged = new Dictionary<string, ActionSequence>();

            _context = ctx;
            KeyDownCommand = new CK.Windows.App.VMCommand( () => _context.SelectedElement = this );
            _currentKeyModeModeVM = new VMKeyboardMode( _context, k.Current.Mode );
            _currentLayoutKeyModeModeVM = new VMKeyboardMode( _context, k.CurrentLayout.Current.Mode );

            _layoutKeyModes = new ObservableCollection<VMLayoutKeyModeEditable>();
            _keyModes = new ObservableCollection<VMKeyModeEditable>();

            RefreshKeyModeCollection();
            RefreshLayoutKeyModeCollection();

            GetImageSourceCache();

            RegisterEvents();
        }

        #region Properties

        /// <summary>
        /// Gets if the current <see cref="IKeyMode"/> is a fallback or not.
        /// </summary>
        public bool IsKeyModeFallback { get { return _key.Current.IsFallBack; } }

        /// <summary>
        /// Gets if the current <see cref="ILayoutKeyMode"/> is a fallback or not.
        /// </summary>
        public bool IsLayoutKeyModeFallback { get { return _key.CurrentLayout.Current.IsFallBack; } }

        /// <summary>
        /// Gets the command called when the user releases the left click on the key
        /// </summary>
        public ICommand KeyUpCommand { get { return _keyUpCmd; } set { _keyUpCmd = value; } }

        /// <summary>
        /// Gets the command called when the user pushes the left click on the key
        /// </summary>
        public ICommand KeyDownCommand { get { return _keyDownCmd; } set { _keyDownCmd = value; } }

        /// <summary>
        /// Gets the command called when the user (lef-click) presses the key
        /// </summary>
        public ICommand KeyPressedCommand { get { return _keyPressedCmd; } set { _keyPressedCmd = value; } }

        ObservableCollection<VMLayoutKeyModeEditable> _layoutKeyModes;
        public ObservableCollection<VMLayoutKeyModeEditable> LayoutKeyModes { get { return _layoutKeyModes; } }

        ObservableCollection<VMKeyModeEditable> _keyModes;
        public ObservableCollection<VMKeyModeEditable> KeyModes { get { return _keyModes; } }

        public override VMContextElementEditable Parent
        {
            get { return Context.Obtain( Model.Zone ); }
        }
        private VMZoneEditable ActualParent { get { return Parent as VMZoneEditable; } }

        public string Name
        {
            get
            {
                return KeyModeVM.UpLabel;
            }
            set
            {
                if( !String.IsNullOrWhiteSpace( value ) )
                    KeyModeVM.UpLabel = value;
                IsBeingRenamed = false;
            }
        }

        /// <summary>
        /// Gets whether this element is selected.
        /// </summary>
        public override bool IsSelected
        {
            get { return _isSelected || LayoutKeyModeVM.IsSelected || KeyModeVM.IsSelected; }
            set
            {
                if( _isSelected != value )
                {
                    _isSelected = false;

                    if( value )
                    {
                        ZIndex = 100;
                        Parent.IsExpanded = value;

                        //When selecting the layoutkeymode or the keymode via the KeyEditionTemplate, the key (this element) is selected in the treeview. The two way binding triggers this set.
                        //We need to avoid setting the current element in this case.
                        if( Context.SelectedElement != KeyModeVM && Context.SelectedElement != LayoutKeyModeVM )
                        {
                            if( Context.CurrentlyDisplayedModeType == ModeTypes.Mode ) Context.SelectedElement = this.KeyModeVM;
                            else if( Context.CurrentlyDisplayedModeType == ModeTypes.Layout ) Context.SelectedElement = this.LayoutKeyModeVM;
                            else { _isSelected = true; Context.SelectedElement = this; }
                        }
                    }
                    else ZIndex = 1;

                    if( !value )
                    {
                        this.KeyModeVM.IsSelected = false;
                        this.LayoutKeyModeVM.IsSelected = false;
                    }

                    OnPropertyChanged( "IsSelected" );
                    OnPropertyChanged( "IsBeingEdited" );
                    OnPropertyChanged( "Opacity" );
                }

                LayoutKeyModeVM.TriggerPropertyChanged( "IsBeingEdited" );
                LayoutKeyModeVM.TriggerPropertyChanged( "IsSelected" );

                KeyModeVM.TriggerPropertyChanged( "IsBeingEdited" );
                KeyModeVM.TriggerPropertyChanged( "IsSelected" );
            }
        }

        /// <summary>
        /// Gets the model linked to this ViewModel
        /// </summary>
        public IKey Model { get { return _key; } }

        FallbackVisibility _showFallback;
        public FallbackVisibility ShowFallback
        {
            get { return _showFallback; }
            set
            {
                _showFallback = value;
                OnPropertyChanged( "ShowFallback" );
            }
        }

        public VMKeyModeEditable KeyModeVM { get { return Context.Obtain( _key.Current ); } }
        public VMLayoutKeyModeEditable LayoutKeyModeVM { get { return Context.Obtain( LayoutKeyMode ); } }

        /// <summary>
        /// If there is no <see cref="IKeyMode"/> for the underlying <see cref="IKey"/> on the current <see cref="IKeyboardMode"/>, gets whether propeties of the nearest <see cref="IKeyMode"/> should be displayed.
        /// </summary>
        public bool ShowKeyModeFallback { get { return (ShowFallback & FallbackVisibility.FallbackOnKeyMode) == FallbackVisibility.FallbackOnKeyMode; } }

        /// <summary>
        /// If there is no <see cref="ILayoutKeyMode"/> for the underlying <see cref="IKey"/> on the current <see cref="IKeyboardMode"/>, gets whether propeties of the nearest <see cref="ILayoutKeyMode"/> should be displayed.
        /// </summary>
        public bool ShowLayoutFallback { get { return (ShowFallback & FallbackVisibility.FallbackOnLayout) == FallbackVisibility.FallbackOnLayout; } }

        /// <summary>
        /// Gets the current actualKey layout.
        /// </summary>
        public ILayoutKeyMode LayoutKeyMode
        {
            get { return _key.CurrentLayout.Current; }
        }

        public ILayoutKey LayoutKey
        {
            get { return _key.CurrentLayout; }
        }

        int _index = -1;
        /// <summary>
        /// Gets the logical position of the <see cref="IKey"/> in the zone.
        /// </summary>
        public int Index
        {
            get { return _key.Index; }
            set { _key.Index = value; }
        }

        #region Layout Properties

        ImageSource _imageSource;
        public ImageSource ImageSource { get { return _imageSource; } }

        Image _image;
        /// <summary>
        /// Gets the image associated with the underlying <see cref="ILayoutKeyMode"/>, for the current <see cref="IKeyboardMode"/>
        /// </summary>
        public Image Image
        {
            get
            {
                if( _image == null )
                {
                    Image image = new Image();
                    GetImageSourceCache();
                    image.Source = _imageSource;
                    return image;
                }
                else
                {
                    Image image = new Image();
                    image.Source = _image.Source;
                    return image;
                }
            }
            set
            {
                if( value != _image )
                {
                    _image = value;
                    _imageSource = (_image == null) ? null : _image.Source;
                    OnPropertyChanged( "Image" );
                    OnPropertyChanged( "ImageSource" );
                }
                //_context.Config[_key.Current]["Image"] = value;
                //GetImageSourceCache();
            }
        }

        /// <summary>
        /// We save the bitmapImage that is the source of the image set to this key.
        /// Thanks to that, we can call the Image property from multiple components.
        /// If we save the Image itself in a cache, it can only be used in one component at a time.
        /// </summary>
        private void GetImageSourceCache()
        {
            object o = _context.SkinConfiguration[_key.Current]["Image"];
            if( o != null )
            {
                var source = o as ImageSource;
                if( source != null )
                    _imageSource = source;
                else
                    _imageSource = WPFImageProcessingHelper.ProcessImage( o ).Source;
            }
            else _imageSource = null;
        }

        /// <summary>
        /// Gets the X coordinate of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int X
        {
            get { return _key.CurrentLayout.Current.X; }
            set
            {
                _key.CurrentLayout.Current.X = value;
                OnPropertyChanged( "X" );
            }
        }

        /// <summary>
        /// Gets the Y coordinate of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int Y
        {
            get { return _key.CurrentLayout.Current.Y; }
            set
            {
                _key.CurrentLayout.Current.Y = value;
                OnPropertyChanged( "Y" );
            }
        }

        /// <summary>
        /// Gets or sets the width of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int Width
        {
            get { return _key.CurrentLayout.Current.Width; }
            set
            {
                _key.CurrentLayout.Current.Width = value;
                OnPropertyChanged( "Width" );
            }
        }

        /// <summary>
        /// Gets or sets the height of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int Height
        {
            get { return _key.CurrentLayout.Current.Height; }
            set
            {
                _key.CurrentLayout.Current.Height = value;
                OnPropertyChanged( "Height" );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this actual key is visible or not, for the current <see cref="IKeyMode"/>.
        /// </summary>
        public Visibility Visible
        {
            get { return IsVisible ? Visibility.Visible : Visibility.Collapsed; }
            set
            {
                IsVisible = (value == Visibility.Visible);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this actual key is visible or not, for the current <see cref="IKeyMode"/>.
        /// </summary>
        public bool IsVisible
        {
            get { return LayoutKeyMode.Visible; }
            set
            {
                LayoutKeyMode.Visible = value;
                OnPropertyChanged( "IsVisible" );
                OnPropertyChanged( "Visible" );
            }
        }

        #endregion

        #region KeyMode Properties

        /// <summary>
        /// Gets a value indicating wether the current keymode is enabled or not.
        /// </summary>
        public bool Enabled { get { return _key.Current.Enabled; } }

        /// <summary>
        /// Makes sure there is a KeyMode on this key for the current mode.
        /// </summary>
        private void EnsureKeyMode()
        {
            //If the user is modifying a property linked to the KeyMode and that there is no KeyMode for this mode; we create one.
            if( _key.Current.IsFallBack )
            {
                _key.KeyModes.Create( _key.Keyboard.CurrentMode );
            }
        }

        /// <summary>
        /// Gets or sets the description of the current <see cref="IKeyMode"/> of this Key
        /// </summary>
        public string Description
        {
            get { return _key.Current.Description; }
            set { _key.Current.Description = value; }
        }

        #endregion

        #endregion

        #region Methods

        internal void TriggerOnPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );
        }

        public void TriggerMouseEvent( KeyboardEditorMouseEvent eventType, PointerDeviceEventArgs args )
        {
            switch( eventType )
            {
                case KeyboardEditorMouseEvent.MouseMove:
                    OnMouseMove( args );
                    break;
                case KeyboardEditorMouseEvent.PointerButtonUp:
                    OnPointerButtonUp( args );
                    break;
                case KeyboardEditorMouseEvent.PointerButtonDown:
                    //Console.Out.WriteLine("Down from context");
                    break;
                default: //ButtonDown is handler by a Command, we don't use the pointer device driver for that. (yet ?)
                    break;
            }
        }

        #endregion

        #region OnXXX

        private void OnLayoutKeyModelCollectionChanged( object sender, LayoutKeyModeEventArgs e )
        {
            RefreshLayoutKeyModeCollection();
        }

        private void RefreshLayoutKeyModeCollection()
        {
            _layoutKeyModes.Clear();

            foreach( var lkm in Model.CurrentLayout.LayoutKeyModes )
            {
                _layoutKeyModes.Add( Context.Obtain( lkm ) );
            }
        }

        private void OnKeyModelCollectionChanged( object sender, KeyModeEventArgs e )
        {
            RefreshKeyModeCollection();
        }

        private void RefreshKeyModeCollection()
        {
            _keyModes.Clear();

            foreach( var km in Model.KeyModes )
            {
                _keyModes.Add( Context.Obtain( km ) );
            }
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( _key.Current.GetPropertyLookupPath().Contains( e.Obj ) )
            {
                if( String.IsNullOrWhiteSpace( e.Key ) )
                {
                    OnPropertyChanged( "Image" );
                    OnPropertyChanged( "ImageSource" );
                    OnPropertyChanged( "ShowLabel" );
                    OnPropertyChanged( "ShowImage" );
                    OnPropertyChanged( "ShowIcon" );
                }
                else
                {
                    switch( e.Key )
                    {
                        case "Image":
                            GetImageSourceCache();
                            OnPropertyChanged( "Image" );
                            OnPropertyChanged( "ImageSource" );
                            break;
                        case "DisplayType":
                            OnPropertyChanged( "ShowImage" );
                            OnPropertyChanged( "ShowLabel" );
                            OnPropertyChanged( "ShowIcon" );
                            OnPropertyChanged( "FontFamily" );
                            LayoutKeyModeVM.TriggerPropertyChanged( "ShowLabel" );
                            KeyModeVM.TriggerPropertyChanged( "FontFamily" );
                            KeyModeVM.TriggerPropertyChanged( "Icon" );
                            break;
                    }
                }
            }

            if( LayoutKeyMode.GetPropertyLookupPath().Contains( e.Obj ) )
            {
                //Console.Out.WriteLine( e.Key );
                if( String.IsNullOrWhiteSpace( e.Key ) )
                {
                    OnPropertyChanged( "Opacity" );
                    OnPropertyChanged( "FontSize" );
                    OnPropertyChanged( "FontStyle" );
                    OnPropertyChanged( "FontWeight" );
                    OnPropertyChanged( "FontFamily" );
                    OnPropertyChanged( "Background" );
                    OnPropertyChanged( "LetterColor" );
                    OnPropertyChanged( "HoverBackground" );
                    OnPropertyChanged( "TextDecorations" );
                    OnPropertyChanged( "PressedBackground" );
                    OnPropertyChanged( "HighlightBackground" );
                    OnPropertyChanged( "HighlightFontColor" );
                }
                else
                {
                    switch( e.Key )
                    {
                        case "Visible":
                            OnPropertyChanged( "Visible" );
                            LayoutKeyModeVM.TriggerPropertyChanged( "Visible" );
                            break;
                        default:
                            OnPropertyChanged( e.Key );
                            break;
                    }
                }
            }
        }

        private void OnPropertyChangedTriggered( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsBeingEdited" )
            {
                OnPropertyChanged( "Opacity" );
            }
        }

        void OnModeChanged( object sender, KeyboardModeChangedEventArgs e )
        {
            OnTriggerModeChanged();
        }

        public void OnKeyPropertyChanged( object sender, KeyPropertyChangedEventArgs e )
        {
            if( _actionsOnPropertiesChanged.ContainsKey( e.PropertyName ) )
                _actionsOnPropertiesChanged[e.PropertyName].Run();
        }

        private void RegisterEvents()
        {
            RegisterOnPropertyChanged();

            PropertyChanged += OnPropertyChangedTriggered;
            Context.Config.ConfigChanged += OnConfigChanged;
            Context.SkinConfiguration.ConfigChanged += OnConfigChanged;

            _key.KeyPropertyChanged += OnKeyPropertyChanged;
            _key.Keyboard.CurrentModeChanged += OnModeChanged;

            Model.KeyModes.KeyModeCreated += OnKeyModelCollectionChanged;
            Model.KeyModes.KeyModeDestroyed += OnKeyModelCollectionChanged;

            Model.CurrentLayout.LayoutKeyModes.LayoutKeyModeCreated += OnLayoutKeyModelCollectionChanged;
            Model.CurrentLayout.LayoutKeyModes.LayoutKeyModeDestroyed += OnLayoutKeyModelCollectionChanged;
        }

        private void UnregisterEvents()
        {
            _actionsOnPropertiesChanged.Clear();

            PropertyChanged -= OnPropertyChangedTriggered;
            Context.Config.ConfigChanged -= OnConfigChanged;
            Context.SkinConfiguration.ConfigChanged -= OnConfigChanged;

            _key.KeyPropertyChanged -= OnKeyPropertyChanged;
            _key.Keyboard.CurrentModeChanged -= OnModeChanged;

            Model.KeyModes.KeyModeCreated -= OnKeyModelCollectionChanged;
            Model.KeyModes.KeyModeDestroyed -= OnKeyModelCollectionChanged;

            Model.CurrentLayout.LayoutKeyModes.LayoutKeyModeCreated -= OnLayoutKeyModelCollectionChanged;
            Model.CurrentLayout.LayoutKeyModes.LayoutKeyModeDestroyed -= OnLayoutKeyModelCollectionChanged;
        }

        private void RegisterOnPropertyChanged()
        {
            SetActionOnPropertyChanged( "CurrentLayout", () =>
            {
                DispatchPropertyChanged( "HighlightBackground", "LayoutKeyMode" );
                DispatchPropertyChanged( "HighlightFontColor", "LayoutKeyMode" );
                DispatchPropertyChanged( "PressedBackground", "LayoutKeyMode" );
                DispatchPropertyChanged( "HoverBackground", "LayoutKeyMode" );
                DispatchPropertyChanged( "TextDecorations", "LayoutKeyMode" );
                DispatchPropertyChanged( "LetterColor", "LayoutKeyMode" );
                DispatchPropertyChanged( "FontWeight", "LayoutKeyMode" );
                DispatchPropertyChanged( "FontFamily", "LayoutKeyMode" );
                DispatchPropertyChanged( "Background", "LayoutKeyMode" );
                DispatchPropertyChanged( "FontStyle", "LayoutKeyMode" );
                DispatchPropertyChanged( "ShowLabel", "LayoutKeyMode" );
                DispatchPropertyChanged( "FontSize", "LayoutKeyMode" );
                OnPropertyChanged( "Opacity" );
                OnPropertyChanged( "Image" );
                OnPropertyChanged( "ImageSource" );
            } );

            SetActionOnPropertyChanged( "Current", () =>
            {
                DispatchPropertyChanged( "Enabled", "KeyMode" );
                DispatchPropertyChanged( "Description", "KeyMode" );
            } );

            SetActionOnPropertyChanged( "X", () => { OnPropertyChanged( "X" ); DispatchPropertyChanged( "X", "LayoutKeyMode" ); } );
            SetActionOnPropertyChanged( "Y", () => { OnPropertyChanged( "Y" ); DispatchPropertyChanged( "Y", "LayoutKeyMode" ); } );
            SetActionOnPropertyChanged( "W", () => { OnPropertyChanged( "Width" ); DispatchPropertyChanged( "Width", "LayoutKeyMode" ); } );
            SetActionOnPropertyChanged( "H", () => { OnPropertyChanged( "Height" ); DispatchPropertyChanged( "Height", "LayoutKeyMode" ); } );
            SetActionOnPropertyChanged( "Width", () => DispatchPropertyChanged( "Width", "LayoutKeyMode" ) );
            SetActionOnPropertyChanged( "Height", () => DispatchPropertyChanged( "Height", "LayoutKeyMode" ) );
            SetActionOnPropertyChanged( "Enabled", () => DispatchPropertyChanged( "Enabled", "KeyMode" ) );
            SetActionOnPropertyChanged( "Description", () => DispatchPropertyChanged( "Description", "KeyMode" ) );

            SetActionOnPropertyChanged( "Visible", () =>
            {
                DispatchPropertyChanged( "IsVisible", "LayoutKeyMode" );
                DispatchPropertyChanged( "Visible", "LayoutKeyMode" );
            } );
        }

        internal override void Dispose()
        {
            foreach( var item in _layoutKeyModes )
            {
                item.Dispose();
            }
            _layoutKeyModes.Clear();

            foreach( var item in _keyModes )
            {
                item.Dispose();
            }
            _keyModes.Clear();

            UnregisterEvents();
            base.Dispose();
        }

        #endregion

        //Dispatches the property changed to the LayoutKeyMode if necessary
        private void DispatchPropertyChanged( string propertyName, string target )
        {
            OnPropertyChanged( propertyName );

            if( target == "LayoutKeyMode" )
            {
                if( LayoutKeyModeVM != null )
                {
                    LayoutKeyModeVM.TriggerPropertyChanged( propertyName );
                }
            }
            else if( target == "KeyMode" )
            {
                if( KeyModeVM != null )
                {
                    KeyModeVM.TriggerPropertyChanged( propertyName );
                }
            }
        }

        protected void SetActionOnPropertyChanged( string propertyName, Action action )
        {
            if( !_actionsOnPropertiesChanged.ContainsKey( propertyName ) )
            {
                if( action != null )
                {
                    ActionSequence actions = new ActionSequence();
                    actions.Append( action );
                    _actionsOnPropertiesChanged.Add( propertyName, actions );
                }
            }
            else
            {
                if( action != null )
                    _actionsOnPropertiesChanged[propertyName].Append( action );
            }
        }

        internal void IndexChanged( int previousIndex )
        {
            ActualParent.KeyIndexChanged( previousIndex );
            OnPropertyChanged( "Index" );
        }

        void SetCommands()
        {
            _keyDownCmd = new CK.Windows.App.VMCommand( () => { if( !_key.IsDown )_key.Push(); } );
            _keyUpCmd = new CK.Windows.App.VMCommand( () => { if( _key.IsDown ) _key.Release(); } );
            _keyPressedCmd = new CK.Windows.App.VMCommand( () => { if( _key.IsDown )_key.Release( true ); } );
        }

        CK.Windows.App.VMCommand _deleteKeyCommand;
        public CK.Windows.App.VMCommand DeleteKeyCommand
        {
            get
            {
                if( _deleteKeyCommand == null )
                {
                    _deleteKeyCommand = new CK.Windows.App.VMCommand( () =>
                    {
                        DeleteKey();
                    } );
                }
                return _deleteKeyCommand;
            }
        }

        internal void DeleteKey()
        {
            ModalViewModel mvm = new ModalViewModel( R.DeleteKey, R.DeleteKeyConfirmation, false, String.Empty, CustomMsgBoxIcon.Warning, 1 );
            mvm.Buttons.Add( new ModalButton( mvm, R.Yes, ModalResult.Yes ) );
            mvm.Buttons.Add( new ModalButton( mvm, R.No, ModalResult.No ) );

            CustomMsgBox msg = new CustomMsgBox( ref mvm );
            msg.ShowDialog();

            if( mvm.ModalResult == ModalResult.Yes )
            {
                Context.SelectedElement = Parent;
                Model.Destroy();
            }
        }

        CK.Windows.App.VMCommand _duplicateKeyCommand;
        public CK.Windows.App.VMCommand DuplicateKeyCommand
        {
            get
            {
                if( _duplicateKeyCommand == null )
                {
                    _duplicateKeyCommand = new CK.Windows.App.VMCommand( () =>
                    {
                        DuplicateKey();
                    } );
                }
                return _duplicateKeyCommand;
            }
        }

        internal void DuplicateKey()
        {
            Model.Zone.Keys.CreateCopy( Model );
        }
    }
}
