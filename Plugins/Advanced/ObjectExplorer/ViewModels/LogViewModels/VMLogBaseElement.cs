#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\LogViewModels\VMLogBaseElement.cs) is part of CiviKey. 
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

using CK.WPF.ViewModel;
using System;
using System.ComponentModel;

namespace CK.Plugins.ObjectExplorer.ViewModels.LogViewModels
{
    public class VMLogBaseElement : VMBase
    {        
        public bool IsBound { get; private set; }
        internal bool _doLog;
        public virtual bool DoLog 
        { 
            get { return _doLog; }
            set { _doLog = value; OnPropertyChanged("DoLog"); OnLogConfigChanged("DoLog");} 
        }
        public event EventHandler<PropertyChangedEventArgs> LogConfigChanged;

        public string Name { get; private set; }

        public VMLogBaseElement(string name, bool isBound)
        {
            Name = name;
            IsBound = isBound;
        }

        protected void OnLogConfigChanged(string name)
        {
            if (LogConfigChanged != null)
            {
                LogConfigChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}