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
        public VMKeyboardEditable( VMContextEditable ctx, IKeyboard kb )
            : base( ctx, kb )
        {
            Model = kb;
            Model.AvailableModeChanged += ( s, e ) => OnPropertyChanged( "AvailableModes" );
            Context.Config.ConfigChanged += new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );
            Model.CurrentModeChanged += ( s, e ) => OnPropertyChanged( "CurrentMode" );

            _keyboardModes = new List<VMKeyboardMode>();
            RefreshModes();
        }

        private void RefreshModes()
        {
            _keyboardModes.Clear();
            foreach( var item in AvailableModes )
            {
                _keyboardModes.Add( new VMKeyboardMode( this, item, Model.CurrentMode.ContainsAll( item ) ) );
            }
            OnPropertyChanged( "KeyboardModes" );
        }

        public IEnumerable<IKeyboardMode> AvailableModes { get { return Model.AvailableMode.AtomicModes; } }

        IList<VMKeyboardMode> _keyboardModes;
        public IEnumerable<VMKeyboardMode> KeyboardModes { get { return _keyboardModes; } }

        protected override void OnDispose()
        {
            Context.Config.ConfigChanged -= new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );
            base.OnDispose();
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.Obj == Layout )
            {
                switch( e.Key )
                {
                    case "KeyboardBackground":
                        OnPropertyChanged( "Background" );
                        break;
                    case "InsideBorderColor":
                        OnPropertyChanged( "InsideBorderColor" );
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the model linked to this ViewModel
        /// </summary>
        public IKeyboard Model { get; private set; }

        /// <summary>
        /// Gets or sets the current mode of the edited keyboard.
        /// </summary>
        public IKeyboardMode CurrentMode
        {
            get { return Model.CurrentMode; }
            set { Model.CurrentMode = value; }
        }

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
    }

    public class VMKeyboardMode : INotifyPropertyChanged
    {
        VMKeyboardEditable _holder;

        public IKeyboardMode Mode { get; private set; }
        bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged( "IsChecked" );
                if( _isChecked ) _holder.AddKeyboardModeCommand.Execute( Mode );
                else _holder.RemoveKeyboardModeCommand.Execute( Mode );
            }
        }

        public VMKeyboardMode( VMKeyboardEditable holder, IKeyboardMode keyboardMode, bool isChecked )
        {
            Mode = keyboardMode;
            _holder = holder;
            _isChecked = isChecked;
        }

        public void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
