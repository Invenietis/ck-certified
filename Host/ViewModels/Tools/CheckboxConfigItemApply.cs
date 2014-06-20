#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\Tools\CheckboxConfigItemApply.cs) is part of CiviKey. 
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
using System.Windows.Input;
using CK.Windows.Config;
using System.Linq;
using CK.WPF.ViewModel;
using System.Diagnostics;
using System.ComponentModel;

namespace Host.VM
{
    /// <summary>
    /// Handles an ICommand and an array of INotifySelectionChanged objects.
    /// This class keeps track of the selected object among its array.
    /// When the selected changes, the button is enabled. otherwise it is disabled.
    /// Note : if no objects are linked, works as a simple button triggering the action set as parameter of the constructor.
    /// </summary>
    public class CheckboxConfigItemApply : ConfigItemAction
    {
        readonly INotifySelectionChanged[] _linkedItems;

        public CheckboxConfigItemApply( ConfigManager configManager, ICommand cmd, params INotifySelectionChanged[] linkedItems )
            : base( configManager, cmd )
        {
            if( linkedItems.Length > 0 )
            {
                _linkedItems = linkedItems;
                foreach( var item in linkedItems )
                {
                    item.PropertyChanged += item_PropertyChanged;
                }
                _previous = _linkedItems.Single( i => i.IsSelected );
            }
            else _linkedItems = new ConfigImplementationSelectorItem[0];
        }

        void item_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsSelected" )
            {
                Debug.Assert( sender != null && sender is INotifySelectionChanged );

                INotifySelectionChanged s = (INotifySelectionChanged)sender;
                if( s.IsSelected )
                {
                    _current = s;
                    NotifyOfPropertyChange( () => IsEnabled );
                }
            }
        }

        object _current;
        object _previous;

        ICommand _actionCommand;
        public new ICommand ActionCommand
        {
            get
            {
                if( _actionCommand == null )
                {
                    if( _linkedItems.Length > 0 )
                    {
                        _actionCommand = new VMCommand<object>( ( o ) =>
                        {
                            _previous = _current;
                            base.ActionCommand.Execute( o );
                            _current = _linkedItems.Single( i => i.IsSelected );
                            NotifyOfPropertyChange( () => IsEnabled );
                        } );
                    }
                    else
                    {
                        _actionCommand = new VMCommand<object>( ( o ) =>
                        {
                            base.ActionCommand.Execute( o );
                        } );
                    }
                }

                return _actionCommand;
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _linkedItems.Length == 0 || (_current != null && _current != _previous);
            }
        }
    }
}
