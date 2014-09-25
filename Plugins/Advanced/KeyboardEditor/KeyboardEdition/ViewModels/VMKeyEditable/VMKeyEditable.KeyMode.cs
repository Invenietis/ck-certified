#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\ViewModels\VMKeyEditable\VMKeyEditable.KeyMode.cs) is part of CiviKey. 
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

using System.Windows.Input;
using CK.Keyboard.Model;
using CK.WPF.ViewModel;

namespace KeyboardEditor.ViewModels
{
    public partial class VMKeyEditable : VMContextElementEditable
    {
        protected void OnTriggerModeChanged()
        {
            RefreshKeyboardModelViewModels();
        }

        internal void RefreshKeyboardModelViewModels()
        {
            _currentLayoutKeyModeModeVM = new VMKeyboardMode( _context, Model.CurrentLayout.Current.Mode );
            _currentKeyModeModeVM = new VMKeyboardMode( _context, Model.Current.Mode );

            OnPropertyChanged( "KeyModeVM" );
            OnPropertyChanged( "LayoutKeyModeVM" );

            foreach( var keyMode in KeyModes )
            {
                keyMode.TriggerPropertyChanged( "IsSelected" );
                keyMode.TriggerModeChanged();
            }

            foreach( var layoutKeyMode in LayoutKeyModes )
            {
                layoutKeyMode.TriggerPropertyChanged( "IsSelected" );
                layoutKeyMode.TriggerModeChanged();
            }

            OnPropertyChanged( "IsSelected" );
            OnPropertyChanged( "CurrentKeyModeModeVM" );
            OnPropertyChanged( "CurrentLayoutKeyModeModeVM" );
        }

        /// <summary>
        /// This regions contains overrides to the <see cref="VMKey"/> properties.
        /// It enables hiding the fallback if necessary.
        /// </summary>
        #region KeyMode properties overrides

        VMKeyboardMode _currentKeyModeModeVM;
        /// <summary>
        /// Gets the current <see cref="IKeyboardMode"/> of the underlying <see cref="IKeyMode"/>
        /// </summary>
        public VMKeyboardMode CurrentKeyModeModeVM { get { return _currentKeyModeModeVM; } }

        VMKeyboardMode _currentLayoutKeyModeModeVM;
        /// <summary>
        /// Gets the current <see cref="IKeyboardMode"/> of the underlying <see cref="IKeyMode"/>
        /// </summary>
        public VMKeyboardMode CurrentLayoutKeyModeModeVM { get { return _currentLayoutKeyModeModeVM; } }

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
                            Model.KeyModes.Create( Model.Keyboard.CurrentMode );
                        }
                        else if( type == "LayoutKeyMode" )
                        {
                            ILayoutKeyMode previousMode = Model.CurrentLayout.Current;
                            ILayoutKeyMode mode = Model.CurrentLayout.LayoutKeyModes.Create( Model.Keyboard.CurrentMode );

                            //Retrieving the previous layoutkeymode's properties values, to apply them on the new layoutkeymode.
                            mode.X = previousMode.X;
                            mode.Y = previousMode.Y;
                            mode.Height = previousMode.Height;
                            mode.Width = previousMode.Width;
                            mode.Visible = true;
                        }

                        Context.KeyboardVM.RefreshCurrentKeyMode();
                        RefreshKeyboardModelViewModels();
                    } );
                }
                return _createKeyModeCommand;
            }
        }

        ICommand _selectKeyMode;
        public ICommand SelectKeyModeCommand
        {
            get
            {
                if( _selectKeyMode == null )
                {
                    _selectKeyMode = new CK.Windows.App.VMCommand( () => 
                    {
                        KeyModeVM.IsSelected = true;
                    } );
                }
                return _selectKeyMode;
            }
        }

        #endregion

    }
}
