#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\EditableSkin\ViewModels\VMKeyboardEditable.cs) is part of CiviKey. 
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
using CK.WPF.ViewModel;
using System.Windows;
using System.Windows.Media;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CK.Windows.Helpers;
using CK.Core;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;

namespace KeyboardEditor.ViewModels
{
    public class VMKeyboardEditable : VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>
    {
        ObservableCollection<VMZoneEditable> _zones;
        ObservableCollection<VMKeyEditable> _keys;
        VMContextEditable _holder;
        IKeyboard _keyboard;
        bool _isSelected;

        public VMKeyboardEditable( VMContextEditable ctx, IKeyboard kb )
            : base( ctx )
        {
            _zones = new ObservableCollection<VMZoneEditable>();
            _keys = new ObservableCollection<VMKeyEditable>();
            _keyboard = kb;
            _holder = ctx;
            Model = kb;
            _keyboardModes = new ObservableCollection<VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>>();

            foreach( IZone zone in _keyboard.Zones )
            {
                var vmz = Context.Obtain( zone );
                // TODO: find a better way....
                if( zone.Name == "Prediction" ) Zones.Insert( 0, vmz );
                else Zones.Add( vmz );

                foreach( IKey key in zone.Keys )
                {
                    _keys.Add( Context.Obtain( key ) );
                }
            }

            RegisterEvents();

            RefreshModes();
        }

        private void RegisterEvents()
        {
            _keyboard.KeyCreated += new EventHandler<KeyEventArgs>( OnKeyCreated );
            _keyboard.KeyMoved += new EventHandler<KeyMovedEventArgs>( OnKeyMoved );
            _keyboard.KeyDestroyed += new EventHandler<KeyEventArgs>( OnKeyDestroyed );
            _keyboard.Zones.ZoneCreated += new EventHandler<ZoneEventArgs>( OnZoneCreated );
            _keyboard.Zones.ZoneDestroyed += new EventHandler<ZoneEventArgs>( OnZoneDestroyed );
            _keyboard.Layouts.LayoutSizeChanged += new EventHandler<LayoutEventArgs>( OnLayoutSizeChanged );

            Context.Config.ConfigChanged += OnConfigChanged;
            Model.AvailableModeChanged += OnModeChanged;
            Model.CurrentModeChanged += OnModeChanged;
        }


        private void UnregisterEvents()
        {
            _keyboard.KeyCreated -= new EventHandler<KeyEventArgs>( OnKeyCreated );
            _keyboard.KeyMoved -= new EventHandler<KeyMovedEventArgs>( OnKeyMoved );
            _keyboard.KeyDestroyed -= new EventHandler<KeyEventArgs>( OnKeyDestroyed );
            _keyboard.Zones.ZoneCreated -= new EventHandler<ZoneEventArgs>( OnZoneCreated );
            _keyboard.Zones.ZoneDestroyed -= new EventHandler<ZoneEventArgs>( OnZoneDestroyed );
            _keyboard.Layouts.LayoutSizeChanged -= new EventHandler<LayoutEventArgs>( OnLayoutSizeChanged );

            Context.Config.ConfigChanged -= OnConfigChanged;
            Model.AvailableModeChanged -= OnModeChanged;
            Model.CurrentModeChanged -= OnModeChanged;
        }

        protected override void OnDispose()
        {
            foreach( VMZoneEditable zone in Zones )
            {
                zone.Dispose();
            }
            _zones.Clear();
            _keys.Clear();

            UnregisterEvents();

            base.OnDispose();
        }

        public void TriggerPropertyChanged()
        {
            OnTriggerPropertyChanged();
            OnPropertyChanged( "Keys" );
            OnPropertyChanged( "BackgroundImagePath" );
        }

        #region Methods

        private void OnModeChanged( object sender, KeyboardModeChangedEventArgs e )
        {
            RefreshModes();
        }

        private void RefreshModes()
        {
            _keyboardModes.Clear();
            foreach( var item in AvailableModes )
            {
                _keyboardModes.Add( new VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>( _holder, item ) );
            }
            OnPropertyChanged( "KeyboardModes" );
        }

        /// <summary>
        /// Initialization's second step.
        /// Used to make sure configuration accessors and service (like the PointerDeviceDriver) are available before using them
        /// </summary>
        public void Initialize()
        {

        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current layout used by the current keyboard.
        /// </summary>
        public ILayout Layout { get { return _keyboard.CurrentLayout; } }

        /// <summary>
        /// Gets or sets the width of the current layout.
        /// </summary>
        public int W
        {
            get { return _keyboard.CurrentLayout.W; }
            set { _keyboard.CurrentLayout.W = value; }
        }

        /// <summary>
        /// Gets or sets the height of the current layout.
        /// </summary>
        public int H
        {
            get { return _keyboard.CurrentLayout.H; }
            set { _keyboard.CurrentLayout.H = value; }
        }

        /// <summary>
        /// Gets the available modes, concatenated as one, seperated by "+" characters.
        /// </summary>
        public IKeyboardMode Modes { get { return _keyboard.AvailableMode; } }

        /// <summary>
        /// Gets the viewmodels for each <see cref="IZone"/> of the linked <see cref="IKeyboard"/>
        /// </summary>
        public ObservableCollection<VMZoneEditable> Zones { get { return _zones; } }

        /// <summary>
        /// Gets the viewmodels for each  <see cref="IKey"/> of the linked <see cref="IKeyboard"/>
        /// </summary>
        public ObservableCollection<VMKeyEditable> Keys { get { return _keys; } }


        #region Modes

        /// <summary>
        /// Gets the current <see cref="IKeyboardMode"/>'s AtomicModes, wrapped in <see cref="VMKeyboardMode"/>.
        /// </summary>
        public ObservableCollection<VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>> KeyboardModes { get { return _keyboardModes; } }

        ObservableCollection<VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>> _keyboardModes;

        private IEnumerable<IKeyboardMode> AvailableModes { get { return Model.AvailableMode.AtomicModes; } }

        /// <summary>
        /// Gets or sets the current mode of the edited keyboard.
        /// </summary>
        public IKeyboardMode CurrentMode
        {
            get { return Model.CurrentMode; }
            set { Model.CurrentMode = value; }
        }

        #endregion

        public override VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> Parent
        {
            get { return null; }
        }

        public override IKeyboardElement LayoutElement
        {
            get { return Layout; }
        }

        /// <summary>
        /// Gets the name of the underlying <see cref="IKeyboard"/>
        /// </summary>
        public string Name { get { return Model.Name; } }

        /// <summary>
        /// Gets the model linked to this ViewModel
        /// </summary>
        public IKeyboard Model { get; private set; }


        /// <summary>
        /// Gets whether this element is being edited. 
        /// An element is being edited if it IsSelected or one of its parents is.
        /// Therefore with this implementation, IsBeingEdited is the same as IsSelected
        /// </summary>
        public override bool IsBeingEdited
        {
            get { return IsSelected; }
        }

        public override bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                Context.SelectedElement = this;
                OnPropertyChanged( "IsBeingEdited" );
                OnPropertyChanged( "IsSelected" );
                foreach( var item in Zones )
                {
                    item.TriggerOnPropertyChanged( "IsBeingEdited", true );
                }
            }
        }

        public Brush InsideBorderColor
        {
            get
            {
                if( Context.Config[Layout]["InsideBorderColor"] != null )
                    return new SolidColorBrush( (Color)Context.Config[Layout]["InsideBorderColor"] );
                return null;
            }
        }

        ImageSourceConverter imsc;
        public object BackgroundImagePath
        {
            get
            {
                if( imsc == null ) imsc = new ImageSourceConverter();
                return imsc.ConvertFromString( Context.Config[Layout].GetOrSet( "KeyboardBackground", "pack://application:,,,/SimpleSkin;component/Images/skinBackground.png" ) );
            }
        }

        #endregion

        #region OnXXX

        void OnKeyCreated( object sender, KeyEventArgs e )
        {
            VMKeyEditable kvm = Context.Obtain( e.Key );
            Context.Obtain( e.Key.Zone ).Keys.Add( kvm );
            _keys.Add( kvm );
        }

        void OnKeyMoved( object sender, KeyMovedEventArgs e )
        {
            Context.Obtain( e.Key ).PositionChanged();
        }

        void OnKeyDestroyed( object sender, KeyEventArgs e )
        {
            Context.Obtain( e.Key.Zone ).Keys.Remove( Context.Obtain( e.Key ) );
            _keys.Remove( Context.Obtain( e.Key ) );
            Context.OnModelDestroy( e.Key );
        }

        void OnZoneCreated( object sender, ZoneEventArgs e )
        {
            //TODO
            var vmz = Context.Obtain( e.Zone );
            if( e.Zone.Name == "Prediction" ) Zones.Insert( 0, vmz );
            else Zones.Add( vmz );
        }

        void OnZoneDestroyed( object sender, ZoneEventArgs e )
        {
            var zone = Context.Obtain( e.Zone );
            if( zone != null )
            {
                foreach( var k in e.Zone.Keys )
                {
                    var mk = Context.Obtain( k );
                    Keys.Remove( mk );
                    Context.OnModelDestroy( k );
                }

                Zones.Remove( zone );
                Context.OnModelDestroy( e.Zone );
                OnTriggerZoneDestroyed();
            }
        }

        protected virtual void OnTriggerZoneDestroyed()
        {
        }

        void OnLayoutSizeChanged( object sender, LayoutEventArgs e )
        {
            if( e.Layout == _keyboard.CurrentLayout )
            {
                OnPropertyChanged( "W" );
                OnPropertyChanged( "H" );
                OnTriggerLayoutSizeChanged();
            }
        }

        protected virtual void OnTriggerLayoutSizeChanged()
        {
        }

        public override void OnKeyDownAction( int keyCode, int delta )
        {
            switch( keyCode )
            {
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

        void MoveUp( int pixels )
        {
            foreach( VMZoneEditable zone in Zones )
            {
                zone.MoveUp( pixels );
            }
        }

        void MoveLeft( int pixels )
        {
            foreach( VMZoneEditable zone in Zones )
            {
                zone.MoveLeft( pixels );
            }
        }

        void MoveDown( int pixels )
        {
            foreach( VMZoneEditable zone in Zones )
            {
                zone.MoveDown( pixels );
            }
        }

        void MoveRight( int pixels )
        {
            foreach( VMZoneEditable zone in Zones )
            {
                zone.MoveRight( pixels );
            }
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.Obj == Layout )
            {
                switch( e.Key )
                {
                    case "KeyboardBackground":
                        OnPropertyChanged( "BackgroundImagePath" );
                        break;
                    case "InsideBorderColor":
                        OnPropertyChanged( "InsideBorderColor" );
                        break;
                }
            }
        }

        protected override void OnDispose()
        {
            _keyboardModes.Clear();
            base.OnDispose();
        }

        #endregion

        #region Commands

        VMCommand<IKeyboardMode> _addKeyboardModeCommand;
        public VMCommand<IKeyboardMode> AddKeyboardModeCommand
        {
            get
            {
                if( _addKeyboardModeCommand == null )
                {
                    _addKeyboardModeCommand = new VMCommand<IKeyboardMode>( ( k ) =>
                    {
                        Debug.Assert( k.IsAtomic, "Should not add a non-atomic IKeyboardMode to the Current keyboard mode of the keyboard editor. Mode is : " + k.ToString() );
                        if( !CurrentMode.ContainsOne( k ) )
                        {
                            CurrentMode = CurrentMode.Add( k );
                            OnPropertyChanged( "CurrentMode" );
                        }
                    } );
                }
                return _addKeyboardModeCommand;
            }
        }

        VMCommand<IKeyboardMode> _removeKeyboardModeCommand;
        public VMCommand<IKeyboardMode> RemoveKeyboardModeCommand
        {
            get
            {
                if( _removeKeyboardModeCommand == null )
                {
                    _removeKeyboardModeCommand = new VMCommand<IKeyboardMode>( ( k ) =>
                    {
                        if( !CurrentMode.ContainsOne( k ) ) throw new KeyNotFoundException( String.Format( "Trying to remove the {0} mode, which can't be found in the {1} mode", k.ToString(), CurrentMode.ToString() ) );
                        {
                            CurrentMode = CurrentMode.Remove( k );
                            OnPropertyChanged( "CurrentMode" );
                        }
                    } );
                }
                return _removeKeyboardModeCommand;
            }
        }

        VMCommand<string> _createZoneCommand;
        public VMCommand<string> CreateZoneCommand
        {
            get
            {
                if( _createZoneCommand == null )
                {
                    _createZoneCommand = new VMCommand<string>( ( name ) =>
                    {
                        Model.Zones.Create( name );
                    } );
                }

                return _createZoneCommand;
            }
        }

        #endregion
    }
}