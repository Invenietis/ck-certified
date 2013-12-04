using CK.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FileLauncher
{
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        VMCommand _openFileDialog;
        string _appCommand;
        WildFile _selectedApp { get; set; }

        public List<WildApp> Apps { get; set; }

        public WildFile SelectedApp 
        {
            get { return _selectedApp; }
            set 
            { 
                _selectedApp = value; NotifyPropertyChanged("SelectedApp");
                _appCommand = FileLocator.GetLocationCommand(_selectedApp);

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
            dlg.DefaultExt = ".exe"; // Default file extension
            dlg.Filter = "Applications (.exe)|*.exe"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                SelectedApp = FileLocator.GetFileFromPath(dlg.FileName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string peropertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(peropertyName));
        }
    }
}
