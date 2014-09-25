#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\VMISelectableElement.cs) is part of CiviKey. 
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
using CK.WPF.ViewModel;

namespace CK.Plugins.ObjectExplorer
{
    public class VMISelectableElement : VMIBase, ISelectableElement
    {
        public bool IsSelected
        {
            get { return VMIContext.SelectedElement == this; }
            set { if( value ) VMIContext.SelectedElement = this; }
        }

        public void SelectedChanged()
        {
            OnPropertyChanged("IsSelected");
        }

        ICommand _gotoCommand;
        public ICommand GoTo
        {
            get
            {
                if (_gotoCommand == null)
                {
                    _gotoCommand = new VMCommand<VMISelectableElement>(
                    (e) =>
                    {
                        if( e is VMAlias<VMIService> ) VMIContext.SelectedElement = (VMIService)((VMAlias<VMIService>)e).Data;
                        else if( e is VMAlias<VMIPlugin> ) VMIContext.SelectedElement = (VMIPlugin)((VMAlias<VMIPlugin>)e).Data;
                        else if( e is VMIService ) VMIContext.SelectedElement = (VMIService)e;
                        else VMIContext.SelectedElement = e;
                    });
                }
                return _gotoCommand;
            }
        }

        public VMISelectableElement( VMIContextViewModel ctx, VMIBase parent )
            : base( ctx, parent )
        {
        }
    }
}
