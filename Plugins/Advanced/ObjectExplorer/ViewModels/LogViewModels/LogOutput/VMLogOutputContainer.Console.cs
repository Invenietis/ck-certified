#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\LogViewModels\LogOutput\VMLogOutputContainer.Console.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WPF.ViewModel;
using System.Windows.Input;
using CK.Windows.App;

namespace CK.Plugins.ObjectExplorer.ViewModels.LogViewModels
{
    public class VMLogOutputCategory : VMBase
    {
        public VMLogOutputCategory( VMLogOutputContainer holder, string name )
        {
            _holder = holder;
            _name = name;
            _isVisible = true;
        }

        VMCommand _toggleFilterCommand;
        VMLogOutputContainer _holder;
        bool _isVisible;
        string _name;

        public string Name { get { return _name; } }
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                OnPropertyChanged( "IsVisible" );
                _holder.PropagateVisibilityChanged( this );
            }
        }
        public ICommand ToggleFilterCommand
        {
            get
            {
                if( _toggleFilterCommand == null )
                {
                    _toggleFilterCommand = new VMCommand( () =>
                    {
                        if( _isVisible )
                            IsVisible = false;
                        else
                            IsVisible = true;
                    } );
                }
                return _toggleFilterCommand;
            }
        }
    }

}
