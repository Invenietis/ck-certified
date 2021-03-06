#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\ViewModels\VMKeyboardEditable\VMKeyboardMode.cs) is part of CiviKey. 
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

using CK.Keyboard.Model;
using CK.WPF.ViewModel;

namespace KeyboardEditor.ViewModels
{
    /// <summary>
    /// Wrapper on a <see cref="IKeyboardMode"/> that enables setting the <see cref="IsChecked"/> property, 
    /// automatically triggering a command on the holder that activates or deactivates the current mode. 
    /// Also has a command setting the holder's current mode to the one embedded in this class. (deactivates all mode that is not contained in the embedded one)
    /// </summary>
    public class VMKeyboardMode : VMBase
    {
        VMContextEditable _holder;

        /// <summary>
        /// Gets the underlying model.
        /// </summary>
        public IKeyboardMode Mode { get; private set; }

        /// <summary>
        /// Gets or sets whether the embedded <see cref="IKeyboardMode"/> is activated or deactivated on the holder
        /// </summary>
        public bool IsChecked
        {
            get { return _holder.KeyboardVM.CurrentMode.ContainsAll( Mode ); }
            set
            {
                if( value ) _holder.KeyboardVM.AddKeyboardModeCommand.Execute( Mode );
                else _holder.KeyboardVM.RemoveKeyboardModeCommand.Execute( Mode );
                OnPropertyChanged( "IsChecked" );
            }
        }

        public VMKeyboardMode( VMContextEditable holder, IKeyboardMode keyboardMode )
        {
            Mode = keyboardMode;
            _holder = holder;
        }

        /// <summary>
        /// Gets whether the embedded <see cref="IKeyboardMode"/> matches the holder's current <see cref="IKeyboardMode"/>
        /// </summary>
        public bool IsHolderCurrent { get { return _holder.KeyboardVM.CurrentMode.ContainsAll( Mode ) && Mode.ContainsAll( _holder.KeyboardVM.CurrentMode ); } }

        CK.Windows.App.VMCommand _applyToCurrentModeCommand;
        /// <summary>
        /// Gets a command that sets the embedded <see cref="IKeyboardMode"/> as the holder's current one.
        /// </summary>
        public CK.Windows.App.VMCommand ApplyToCurrentModeCommand
        {
            get
            {
                if( _applyToCurrentModeCommand == null )
                {
                    _applyToCurrentModeCommand = new CK.Windows.App.VMCommand( () =>
                    {
                        if( !IsHolderCurrent )
                        {
                            _holder.KeyboardVM.CurrentMode = Mode;
                        }
                    } );
                }
                return _applyToCurrentModeCommand;
            }
        }

        public string ModeName { get { return Mode.ToString(); } }
    }
}
