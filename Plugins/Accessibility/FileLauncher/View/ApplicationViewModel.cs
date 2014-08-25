#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\FileLauncher\View\ApplicationViewModel.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.ComponentModel;

namespace FileLauncher
{
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        VMCommand _openFileDialog;
        string _appCommand;
        WildFile _selectedApp { get; set; }

        public List<WildFile> Apps { get; set; }

        public WildFile SelectedApp 
        {
            get { return _selectedApp; }
            set 
            { 
                _selectedApp = value; NotifyPropertyChanged("SelectedApp");
               // _appCommand = FileLocator.GetLocationCommand(_selectedApp);

                NotifyPropertyChanged("AppCommand"); 
            } 
        }
        
        public string AppCommand 
        {
            get { return "launch:" + _appCommand; }
            set { _appCommand = value; NotifyPropertyChanged("AppCommand"); } 
        }

        public VMCommand OpenFileDialog
        {
            get
            {
               if(_openFileDialog == null) _openFileDialog = new VMCommand((Action)OpenDialog);
                return _openFileDialog;
            }
        }

        void OpenDialog()
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "Tous le fichiers (*.*)|*.*"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                SelectedApp = new WildFile(dlg.FileName, false);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string peropertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(peropertyName));
        }
    }
}
