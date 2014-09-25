#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\ViewModels\VMModes\VMLayoutKeyModeEditable.cs) is part of CiviKey. 
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

using System;
using System.Windows;
using System.Windows.Input;
using CK.Keyboard.Model;
using CK.WPF.ViewModel;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public class VMLayoutKeyModeEditable : VMKeyModeBase, IModeViewModel
    {
        ILayoutKeyMode _model;
        string _sectionName;
        string _modeName;

        public VMLayoutKeyModeEditable( VMContextEditable context, ILayoutKeyMode model )
            : base( context, model )
        {
            _model = model;
            _modeName = String.IsNullOrWhiteSpace( _model.Mode.ToString() ) ? R.DefaultMode : _model.Mode.ToString();
            _sectionName = R.DesignSection;
        }

        #region Properties

        /// <summary>
        /// Gets the X coordinate of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int X
        {
            get { return _model.X; }
            set
            {
                _model.X = value;
                OnPropertyChanged( "X" );
            }
        }

        /// <summary>
        /// Gets the Y coordinate of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int Y
        {
            get { return _model.Y; }
            set
            {
                _model.Y = value;
                OnPropertyChanged( "Y" );
            }
        }

        /// <summary>
        /// Gets or sets the width of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int Width
        {
            get { return _model.Width; }
            set
            {
                _model.Width = value;
                OnPropertyChanged( "Width" );
            }
        }

        /// <summary>
        /// Gets or sets the height of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int Height
        {
            get { return _model.Height; }
            set
            {
                _model.Height = value;
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
                IsVisible = ( value == Visibility.Visible );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this actual key is visible or not, for the current <see cref="IKeyMode"/>.
        /// </summary>
        public bool IsVisible
        {
            get { return _model.Visible; }
            set
            {
                _model.Visible = value;
                OnPropertyChanged( "IsVisible" );
                OnPropertyChanged( "Visible" );
            }
        }

        bool _isSelected;
        /// <summary>
        /// Gets whether the element is selected.
        /// A VMKeyModeEditable is selected if its parent is selected and that the keyboard's current mode correspond to the Mode associated with this VMLayoutKeyModeEditable
        /// </summary>
        public override bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if( value )
                {
                    if( Context.SelectedElement != this )
                    {
                        Context.SelectedElement = this;
                        Context.CurrentlyDisplayedModeType = ModeTypes.Layout;
                    }
                }

                _isSelected = value;
                OnPropertyChanged( "IsSelected" );

                ActualParent.TriggerOnPropertyChanged( "IsSelected" );
                ActualParent.TriggerOnPropertyChanged( "IsBeingEdited" );
                ActualParent.TriggerOnPropertyChanged( "Opacity" );
            }
        }
        public bool ShowLabel
        {
            get { return _model.GetPropertyValue<bool>( Context.Config, "ShowLabel", true ); }
        }

        public string SectionName { get { return _sectionName; } }

        public string ModeName { get { return _modeName; } }

        #endregion

        ICommand _deleteLayoutKeyModeCommand;
        /// <summary>
        /// Gets a Command that deletes the <see cref="IKeyMode"/> corresponding to the current <see cref="IKeyboardMode"/>, for the underlying <see cref="IKey"/>
        /// </summary>
        public ICommand DeleteLayoutKeyModeCommand
        {
            get
            {
                if( _deleteLayoutKeyModeCommand == null )
                {
                    _deleteLayoutKeyModeCommand = new CK.Windows.App.VMCommand( () =>
                    {
                        Context.KeyboardVM.CurrentMode = Context.KeyboardContext.EmptyMode;
                        VMKeyEditable parent = ActualParent; //Keeping a ref to the parent, since the model will be detached from its parent when destroyed
                        _model.Destroy();
                        parent.RefreshKeyboardModelViewModels();

                    } );
                }
                return _deleteLayoutKeyModeCommand;
            }
        }

        protected override void OnModeChangedTriggered()
        {
            OnPropertyChanged( "ShowLabel" );
            OnPropertyChanged( "IsSelected" );
            OnPropertyChanged( "IsVisible" );
            OnPropertyChanged( "ModeName" );
            OnPropertyChanged( "Height" );
            OnPropertyChanged( "Width" );
            OnPropertyChanged( "Name" );
            OnPropertyChanged( "X" );
            OnPropertyChanged( "Y" );
        }

        internal override void Dispose()
        {
            base.Dispose();
        }
    }
}
