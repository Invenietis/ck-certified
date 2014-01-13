#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\LogViewModels\LogOutput\VMLogOutputContainer.cs) is part of CiviKey. 
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

using System.Windows.Input;
using CK.WPF.ViewModel;
using CK.Plugins.ObjectExplorer.UI.UserControls;
using System.ComponentModel;

namespace CK.Plugins.ObjectExplorer.ViewModels.LogViewModels
{
    public partial class VMLogOutputContainer : VMBase
    {
        LogConsoleWindow _logConsoleWindow;
        ICommand _clearOutputConsoleCommand;
        ICommand _toggleMaximizedCommand;
        bool _consoleWindowIsClosed = true;
        public bool IsMaximized { get { return _logConsoleWindow.Visibility == System.Windows.Visibility.Visible; } }

        void OnLogConsoleWindowClosing( object sender, CancelEventArgs e )
        {
            e.Cancel = true;
            _logConsoleWindow.Visibility = System.Windows.Visibility.Collapsed;
            _consoleWindowIsClosed = true;
            OnPropertyChanged( "IsMaximized" );
        }

        public ICommand ToggleMaximizeCommand
        {
            get
            {
                if( _toggleMaximizedCommand == null )
                {
                    _toggleMaximizedCommand = new CK.Windows.App.VMCommand( () =>
                    {
                        if( _consoleWindowIsClosed )
                        {
                            _logConsoleWindow.Show();
                            _consoleWindowIsClosed = false;
                        }
                        else
                        {
                            if( _logConsoleWindow.Visibility == System.Windows.Visibility.Visible ) _logConsoleWindow.Visibility = System.Windows.Visibility.Collapsed;
                            else _logConsoleWindow.Visibility = System.Windows.Visibility.Visible;
                        }
                        OnPropertyChanged( "IsMaximized" );
                    } );
                }
                return _toggleMaximizedCommand;
            }
        }
    }
}
