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

namespace ContextEditor.ViewModels
{
    public class VMKeyboardEditable : VMKeyboard<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>
    {
        VMContextEditable _holder;
        bool _isSelected;

        public VMKeyboardEditable( VMContextEditable ctx, IKeyboard kb )
            : base( ctx, kb )
        {
            _holder = ctx;
            Model = kb;
            _keyboardModes = new ObservableCollection<VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>>();

            Model.AvailableModeChanged += ( s, e ) => RefreshModes();
            Model.CurrentModeChanged += ( s, e ) => RefreshModes();
            RefreshModes();
        }

        #region Methods

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
        /// Used to make sure configuration accessors and service (like th PointerDeviceDriver) are available before using them
        /// </summary>
        public void Initialize()
        {
            Context.Config.ConfigChanged += new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );

            foreach( VMZoneEditable zone in Zones )
            {
                zone.Initialize();
            }
        }

        #endregion

        #region Properties

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

        public override VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> Parent
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
                return imsc.ConvertFromString( Context.Config[Layout].GetOrSet( "KeyboardBackground", "pack://application:,,,/EditableSkin;component/Images/skinBackground.png" ) );
            }
        }

        #endregion

        #region OnXXX

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
            Context.Config.ConfigChanged -= new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );
            Model.AvailableModeChanged -= ( s, e ) => RefreshModes();
            Model.CurrentModeChanged -= ( s, e ) => RefreshModes();
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
                        Debug.Assert( k.IsAtomic, "Should not remove a non-atomic IKeyboardMode to the Current keyboard mode of the keyboard editor. Mode is : " + k.ToString() );
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

        VMCommand<object> _createZoneCommand;
        public VMCommand<object> CreateZoneCommand 
        { 
            get 
            {
                if( _createZoneCommand == null )
                {
                    _createZoneCommand = new VMCommand<object>( ( name ) => 
                    {
                        Console.Out.WriteLine( "creating zone named : " + name.ToString() );
                         Model.Zones.Create( name.ToString() );
                    } );
                }

                return _createZoneCommand;
            } 
        }

        #endregion
    }
}