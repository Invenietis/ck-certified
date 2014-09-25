#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\LogViewModels\LogOutput\VMOutputLogEntry.cs) is part of CiviKey. 
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

using System.Windows;
using CK.Plugin;
using CK.WPF.ViewModel;

namespace CK.Plugins.ObjectExplorer.ViewModels.LogViewModels
{
    public class VMOutputLogEntry : VMBase
    {
        public VMOutputLogEntry( VMLogOutputContainer holder, LogEventArgs e, string message, int index )
        {
            _holder = holder;
            _logEventArgs = e;
            _isCreating = e.IsCreating;
            _message = message;
            _index = index;

            ILogInterceptionEntry logEntry = _logEventArgs as ILogInterceptionEntry;
            if( logEntry != null ) _category = logEntry.Member.DeclaringType.Name;
            else _category = "Other";
        }

        public void NotifyVisibilityChanged()
        {
            OnPropertyChanged( "IsVisible" );
            OnPropertyChanged( "LogObject" );
        }

        public Thickness Margin { get { return new Thickness( _logEventArgs.Depth * 2, 2, 0, 2 ); } }
        public string UnderlyingType { get { return _logEventArgs.EntryType.ToString(); } }
        public bool IsVisible { get { return _holder.IsCategoryFiltered( Category ); } }
        public LogEventArgs LogObject { get { return _logEventArgs; } }
        public string Category { get { return _category; } }
        public string Message { get { return _message; } }
        public int Index { get { return _index; } }
        public bool IsCreating { get { return _isCreating; } }

        VMLogOutputContainer _holder;
        LogEventArgs _logEventArgs;
        bool _isCreating;
        string _category;
        string _message;
        int _index;
    }
}
