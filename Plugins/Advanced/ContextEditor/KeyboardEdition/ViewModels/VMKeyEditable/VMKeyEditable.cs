#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\EditableSkin\ViewModels\VMKeyEditable.cs) is part of CiviKey. 
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
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using System.Windows.Controls;
using System.Windows;
using CK.Plugin.Config;
using CK.Core;
using Microsoft.Win32;
using System.Windows.Input;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using CommonServices;
using System.Collections.ObjectModel;
using CK.Windows.App;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public partial class VMKeyEditable : VMKey<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>
    {
        //internal IModeViewModel SelectedModeViewModel { get; set; }

        ObservableCollection<VMLayoutKeyModeEditable> _layoutKeyModes;
        public ObservableCollection<VMLayoutKeyModeEditable> LayoutKeyModes { get { return _layoutKeyModes; } }

        ObservableCollection<VMKeyModeEditable> _keyModes;
        public ObservableCollection<VMKeyModeEditable> KeyModes { get { return _keyModes; } }

        VMContextEditable _ctx;
        bool _isSelected;

        public VMKeyEditable( VMContextEditable ctx, IKey k )
            : base( ctx, k, false )
        {
            _ctx = ctx;
            KeyDownCommand = new CK.Windows.App.VMCommand( () => _ctx.SelectedElement = this );
            _currentKeyModeModeVM = new VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>( _ctx, k.Current.Mode );
            _currentLayoutKeyModeModeVM = new VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>( _ctx, k.CurrentLayout.Current.Mode );

            _layoutKeyModes = new ObservableCollection<VMLayoutKeyModeEditable>();
            _keyModes = new ObservableCollection<VMKeyModeEditable>();

            Model.KeyModes.KeyModeCreated += OnKeyModelCollectionChanged;
            Model.KeyModes.KeyModeDestroyed += OnKeyModelCollectionChanged;

            Model.CurrentLayout.LayoutKeyModes.LayoutKeyModeCreated += OnLayoutKeyModelCollectionChanged;
            Model.CurrentLayout.LayoutKeyModes.LayoutKeyModeDestroyed += OnLayoutKeyModelCollectionChanged;

            RefreshKeyModeCollection();
            RefreshLayoutKeyModeCollection();
        }

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

        #region Properties

        public override VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> Parent
        {
            get { return Context.Obtain( Model.Zone ); }
        }

        public string Name { get { return KeyModeVM.UpLabel; } }

        /// <summary>
        /// Gets whether this element is selected.
        /// </summary>
        public override bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if( _isSelected != value )
                {
                    _isSelected = value;
                    Context.SelectedElement = this;
                    if( value ) ZIndex = 100;
                    else ZIndex = 1;

                    if( value ) Parent.IsExpanded = value;
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

        #endregion

        #region Methods

        public void Initialize()
        {
            Context.Config.ConfigChanged += OnConfigChanged;
            Context.SkinConfiguration.ConfigChanged +=  OnConfigChanged;

            SetActionOnPropertyChanged( "CurrentLayout", () =>
            {
                DispatchPropertyChanged( "HighlightBackground", "LayoutKeyMode" );
                DispatchPropertyChanged( "PressedBackground", "LayoutKeyMode" );
                DispatchPropertyChanged( "HoverBackground", "LayoutKeyMode" );
                DispatchPropertyChanged( "TextDecorations", "LayoutKeyMode" );
                DispatchPropertyChanged( "LetterColor", "LayoutKeyMode" );
                DispatchPropertyChanged( "FontWeight", "LayoutKeyMode" );
                DispatchPropertyChanged( "Background", "LayoutKeyMode" );
                DispatchPropertyChanged( "FontStyle", "LayoutKeyMode" );
                DispatchPropertyChanged( "ShowLabel", "LayoutKeyMode" );
                DispatchPropertyChanged( "FontSize", "LayoutKeyMode" );
                OnPropertyChanged( "Opacity" );
                OnPropertyChanged( "Image" );
            } );

            this.PropertyChanged += OnPropertyChangedTriggered;
        }

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
                default: //ButtonDown is handler by a Command, we don't use the pointer device driver for that. (yet ?)
                    break;
            }
        }

        #endregion

        #region OnXXX

        public override void OnKeyDownAction( int keyCode, int delta )
        {
            switch( keyCode )
            {
                case VMContextEditable.suppr:
                    DeleteKey();
                    break;
                case VMContextEditable.left:
                    MoveLeft( delta );
                    break;
                case VMContextEditable.up:
                    MoveUp( delta );
                    break;
                case VMContextEditable.right:
                    MoveRight( delta );
                    break;
                case VMContextEditable.down:
                    MoveDown( delta );
                    break;
            }
        }

        public void MoveUp( int pixels )
        {
            Y -= pixels;
        }

        public void MoveLeft( int pixels )
        {
            X -= pixels;
        }

        public void MoveDown( int pixels )
        {
            Y += pixels;
        }

        public void MoveRight( int pixels )
        {
            X += pixels;
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( Model.Current.GetPropertyLookupPath().Contains( e.Obj ) )
            {
                OnPropertyChanged( "Image" );
            }
            if( LayoutKeyMode.GetPropertyLookupPath().Contains( e.Obj ) )
            {
                LayoutKeyModeVM.TriggerPropertyChanged( "HighlightBackground" );
                LayoutKeyModeVM.TriggerPropertyChanged( "PressedBackground" );
                LayoutKeyModeVM.TriggerPropertyChanged( "HoverBackground" );
                LayoutKeyModeVM.TriggerPropertyChanged( "TextDecorations" );
                LayoutKeyModeVM.TriggerPropertyChanged( "LetterColor" );
                LayoutKeyModeVM.TriggerPropertyChanged( "Background" );
                LayoutKeyModeVM.TriggerPropertyChanged( "FontWeight" );
                LayoutKeyModeVM.TriggerPropertyChanged( "FontStyle" );
                LayoutKeyModeVM.TriggerPropertyChanged( "FontSize" );

                OnPropertyChanged( "ShowImage" );
                OnPropertyChanged( "ShowLabel" );
                OnPropertyChanged( "Image" );
            }
        }

        private void OnPropertyChangedTriggered( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsBeingEdited" )
            {
                OnPropertyChanged( "Opacity" );
            }
        }

        protected override void OnDispose()
        {
            Model.KeyModes.KeyModeCreated -= ( obj, sender ) => RefreshKeyModeCollection();
            Model.KeyModes.KeyModeDestroyed -= ( obj, sender ) => RefreshKeyModeCollection();

            Model.CurrentLayout.LayoutKeyModes.LayoutKeyModeCreated -= ( obj, sender ) => RefreshLayoutKeyModeCollection();
            Model.CurrentLayout.LayoutKeyModes.LayoutKeyModeDestroyed -= ( obj, sender ) => RefreshLayoutKeyModeCollection();

            Context.Config.ConfigChanged -= OnConfigChanged;
            Context.SkinConfiguration.ConfigChanged -= OnConfigChanged;

            this.PropertyChanged -= OnPropertyChangedTriggered;
            base.OnDispose();
        }

        #endregion

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

        private void DeleteKey()
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
    }
}
