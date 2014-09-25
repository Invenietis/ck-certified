#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\ViewModels\VMKeyEditable\VMKeyEditable.LayoutKeyMode.cs) is part of CiviKey. 
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

using CK.Keyboard.Model;

namespace KeyboardEditor.ViewModels
{
    public partial class VMKeyEditable : VMContextElementEditable
    {
        public override IKeyboardElement LayoutElement
        {
            get { return Model.CurrentLayout.Current; }
        }

        string _displayType;
        public string DisplayType
        {
            get { return _displayType; }
            set
            {
                if( value != _displayType && ( value == "Label" || value == "Icon" || value == "Image" ) )
                {
                    _displayType = value;
                }
            }
        }

        public bool ShowLabel
        {
            get { return (_displayType != null) ? _displayType == "Label" : Context.SkinConfiguration[_key.Current].GetOrSet( "DisplayType", "Label" ) == "Label"; }
            set
            {
                //if( value ) Context.SkinConfiguration[_key.Current]["DisplayType"] = "Label";
                if( value ) _displayType = "Label";
                OnPropertyChanged();
                OnPropertyChanged( "DisplayType" );
            }
        }

        public bool ShowImage
        {
            get { return (_displayType != null) ? _displayType == "Image" : Context.SkinConfiguration[_key.Current].GetOrSet( "DisplayType", "Label" ) == "Image"; }
            set
            {
                if( value ) _displayType = "Image";
                OnPropertyChanged();
                OnPropertyChanged( "DisplayType" );
            }
        }

        public bool ShowIcon
        {
            get { return (_displayType != null) ? _displayType == "Icon" : Context.SkinConfiguration[_key.Current].GetOrSet( "DisplayType", "Label" ) == "Icon"; }
            set
            {
                if( value ) _displayType = "Icon";
                OnPropertyChanged();
                OnPropertyChanged( "DisplayType" );
                KeyModeVM.TriggerPropertyChanged( "FontFamily" );
            }
        }

        /// <summary>
        /// Gets the opacity of the key.
        /// When the key has <see cref="IsBeingEdited"/> set to false, the opacity is lowered.
        /// returns 1 otherwise.
        /// </summary>
        public double Opacity
        {
            get { return IsBeingEdited ? 1 : 0.3; }
        }

        double _zIndex = 50;
        public double ZIndex
        {
            get { return _zIndex; }
            set
            {
                _zIndex = value;
                OnPropertyChanged( "ZIndex" );
            }
        }

        CK.Windows.App.VMCommand _selectLayoutKeyMode;
        public CK.Windows.App.VMCommand SelectLayoutKeyModeCommand
        {
            get
            {
                if( _selectLayoutKeyMode == null )
                {
                    _selectLayoutKeyMode = new CK.Windows.App.VMCommand( () =>
                    {
                        LayoutKeyModeVM.IsSelected = true;
                    } );
                }
                return _selectLayoutKeyMode;
            }
        }
    }
}