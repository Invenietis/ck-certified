#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\LogViewModels\LogOutput\VMLogOutputCategory.cs) is part of CiviKey. 
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
using System.Collections.ObjectModel;
using CK.WPF.ViewModel;
using CK.Plugin;
using CK.Plugins.ObjectExplorer.UI.UserControls;
using System.ComponentModel;

namespace CK.Plugins.ObjectExplorer.ViewModels.LogViewModels
{
    public partial class VMLogOutputContainer : VMBase
    {
        public VMLogOutputContainer()
        {
            _counter = 0;
            _maxCount = DEFAULTMAXCOUNT;
            Entries = new ObservableCollection<VMOutputLogEntry>();
            Categories = new ObservableCollection<VMLogOutputCategory>();
            _logConsoleWindow = new LogConsoleWindow() { DataContext = this };
            _logConsoleWindow.Closing += new CancelEventHandler( OnLogConsoleWindowClosing );
        }

        public ObservableCollection<VMOutputLogEntry> Entries { get; private set; }
        public ObservableCollection<VMLogOutputCategory> Categories { get; private set; }

        const int DEFAULTMAXCOUNT = 200;
        int _maxCount;
        int _counter;

        /// <summary>
        /// Gets or sets the maximum number of entries that can be displayed.
        /// Once this number is reached, the container starts discarding the older entries
        /// </summary>
        public int MaxCount { get { return _maxCount; } set { _maxCount = value; } }

        public void Add( LogEventArgs e, string message )
        {
            Add( new VMOutputLogEntry( this, e, message, ++_counter ) );
        }

        public bool IsCategoryFiltered( string category )
        {
            foreach( VMLogOutputCategory cat in Categories )
            {
                if( cat.Name == category )
                    return cat.IsVisible;
            }
            return false;
        }

        public void PropagateVisibilityChanged( VMLogOutputCategory category )
        {
            foreach( VMOutputLogEntry entry in Entries )
            {
                if( entry.Category == category.Name )
                    entry.NotifyVisibilityChanged();
            }
        }

        public void Add( VMOutputLogEntry entry )
        {
            while( Entries.Count >= _maxCount )
                Entries.RemoveAt( 0 );

            bool exists = false;
            foreach( VMLogOutputCategory category in Categories )
            {
                if( category.Name == entry.Category )
                {
                    exists = true;
                    break;
                }
            }

            if( !exists )
                Categories.Add( new VMLogOutputCategory( this, entry.Category ) );

            Entries.Add( entry );
        }

        public void Clear()
        {
            Entries.Clear();
        }

        public ICommand ClearOutputConsoleCommand
        {
            get
            {
                if( _clearOutputConsoleCommand == null )
                {
                    _clearOutputConsoleCommand = new VMCommand( () =>
                    {
                        Clear();
                    } );
                }
                return _clearOutputConsoleCommand;
            }
        }
    }

}
