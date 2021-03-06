#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ViewModels\ElementViewModels\ContextViewModel.cs) is part of CiviKey. 
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

using Caliburn.Micro;
using CK.Plugin.Config;

namespace KeyboardEditor.ViewModels
{
    public class ContextViewModel : PropertyChangedBase
    {
        IUriHistory _model;
        bool _isSelected;
        ContextListViewModel _parent;

        public ContextViewModel( ContextListViewModel parent, IUriHistory model )
        {
            _parent = parent;
            _model = model;
        }

        public bool IsSelected 
        { 
            get { return _isSelected; } 
            set { _isSelected = value; NotifyOfPropertyChange( () => IsSelected ); } 
        }

        public IUriHistory Context { get { return _model; } }
    }
}
