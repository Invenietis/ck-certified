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

namespace ContextEditor.ViewModels
{
    public partial class VMKeyEditable : VMKey<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>
    {
      
        /// <summary>
        /// Gets the uplabel of the underlying <see cref="IKeyMode"/>
        /// </summary>
        public string Name { get { return UpLabel; } }

        protected override void OnTriggerModeChanged()
        {
            RefreshKeyboardModelViewModels();
        }

        private void RefreshKeyboardModelViewModels()
        {
            _currentLayoutKeyModeModeVM = new VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>( _ctx, Model.CurrentLayout.Current.Mode );
            _currentKeyModeModeVM = new VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>( _ctx, Model.Current.Mode );
            OnPropertyChanged( "CurrentLayoutKeyModeModeVM" );
            OnPropertyChanged( "CurrentKeyModeModeVM" );
        }

        /// <summary>
        /// This regions contains overrides to the <see cref="VMKey"/> properties.
        /// It enables hidding the fallback if necessary.
        /// </summary>
        #region KeyMode properties overrides

        VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> _currentKeyModeModeVM;
        /// <summary>
        /// Gets the current <see cref="IKeyboardMode"/> of the underlying <see cref="IKeyMode"/>
        /// </summary>
        public VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> CurrentKeyModeModeVM { get { return _currentKeyModeModeVM; } }

        VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> _currentLayoutKeyModeModeVM;
        /// <summary>
        /// Gets the current <see cref="IKeyboardMode"/> of the underlying <see cref="IKeyMode"/>
        /// </summary>
        public VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> CurrentLayoutKeyModeModeVM { get { return _currentLayoutKeyModeModeVM; } }

        VMCommand<string> _createKeyModeCommand;
        /// <summary>
        /// Gets a Command that creates the <see cref="IKeyMode"/> corresponding to the current <see cref="IKeyboardMode"/>, for the underlying <see cref="IKey"/>
        /// </summary>
        public VMCommand<string> CreateKeyModeCommand
        {
            get
            {
                if( _createKeyModeCommand == null )
                {
                    _createKeyModeCommand = new VMCommand<string>( ( type ) =>
                    {

                        if( type == "KeyMode" )
                        {
                            Debug.Assert( IsKeyModeFallback );
                            Model.KeyModes.Create( Model.Keyboard.CurrentMode );

                            OnPropertyChanged( "IsKeyModeFallback" );
                        }
                        else if( type == "LayoutKeyMode" )
                        {
                            Debug.Assert( IsLayoutKeyModeFallback );
                            ILayoutKeyMode previousMode = Model.CurrentLayout.Current;
                            ILayoutKeyMode mode = Model.CurrentLayout.LayoutKeyModes.Create( Model.Keyboard.CurrentMode );

                            //Retrieving the previous layoutkeymode's properties values, to apply them on the new layoutkeymode.
                            mode.X = previousMode.X;
                            mode.Y = previousMode.Y;
                            mode.Height = previousMode.Height;
                            mode.Width = previousMode.Width;
                            mode.Visible = true;

                            OnPropertyChanged( "IsLayoutKeyModeFallback" );
                        }

                        RefreshKeyboardModelViewModels();
                    } );
                }
                return _createKeyModeCommand;
            }
        }

        VMCommand<string> _deleteKeyModeCommand;
        /// <summary>
        /// Gets a Command that deletes the <see cref="IKeyMode"/> corresponding to the current <see cref="IKeyboardMode"/>, for the underlying <see cref="IKey"/>
        /// </summary>
        public VMCommand<string> DeleteKeyModeCommand
        {
            get
            {
                if( _deleteKeyModeCommand == null )
                {
                    _deleteKeyModeCommand = new VMCommand<string>( ( type ) =>
                    {
                        if( type == "KeyMode" )
                        {
                            Debug.Assert( !IsKeyModeFallback );
                            Model.Current.Destroy();

                            OnPropertyChanged( "IsKeyModeFallback" );
                        }
                        else if( type == "LayoutKeyMode" )
                        {
                            Debug.Assert( !IsLayoutKeyModeFallback );
                            Model.CurrentLayout.Current.Destroy();

                            OnPropertyChanged( "IsLayoutKeyModeFallback" );
                        }
                        RefreshKeyboardModelViewModels();

                    } );
                }
                return _deleteKeyModeCommand;
            }
        }

        ///Gets the UpLabel of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public new string UpLabel
        {
            get { return ( !IsKeyModeFallback || ShowKeyModeFallback ) ? base.UpLabel : String.Empty; }
            set { base.UpLabel = value; }
        }

        ///Gets the DownLabel of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public new string DownLabel
        {
            get { return ( !IsKeyModeFallback || ShowKeyModeFallback ) ? base.DownLabel : String.Empty; }
            set { base.DownLabel = value; }
        }

        ///Gets the Description of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public new string Description
        {
            get { return ( !IsKeyModeFallback || ShowKeyModeFallback ) ? base.Description : String.Empty; }
            set { base.Description = value; }
        }

        #endregion

    }
}
