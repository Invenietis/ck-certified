using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.WPF.ViewModel;
using CommonServices;
using ProtocolManagerModel;

namespace BasicCommandHandlers
{
    public class FileLauncherCommandParameterManager : IProtocolParameterManager
    {
        VMCommand _openFileDialog;
        string _appCommand;
        string[] trustedCompanies = {"Adobe", "Microsoft", "Google", "Mozilla", "Apple"};
        IWildFile _selectedApp { get; set; }
        IFileLauncherService _fileLauncher;
        public List<IWildFile> Apps { get; set; }
        bool _showApp;

        public bool ShowApp 
        {
            get { return _showApp; }
            set
            {
                _showApp = value;
                NotifyPropertyChanged( "ShowApp" );
                NotifyPropertyChanged( "IsValid" );
            }
        }

        public FileLauncherCommandParameterManager(IFileLauncherService fileLauncher)
        {
            _fileLauncher = fileLauncher;
            Apps = _fileLauncher.FileLocator.RegistryApps;
            Apps.Sort( ( a, b ) => {
                var vA = FileVersionInfo.GetVersionInfo( a.Path );
                var vB = FileVersionInfo.GetVersionInfo( b.Path );
                if( vA == null || vA.ProductName == null ) return 1;
                if( vB == null || vB.ProductName == null ) return -1;

                foreach( string company in trustedCompanies )
                {
                    if( vA.ProductName.Contains( company ) ) return -1;
                    if( vB.ProductName.Contains( company ) ) return 1;
                }
                return 0;
            } );

            ShowApp = true;
        }

        public IWildFile SelectedApp
        {
            get 
            {
                if( _selectedApp != null )
                {
                    return _selectedApp.Lookup == FileLookup.Registry ? _selectedApp : null;
                }
                return _selectedApp; 
            }
            set
            {
                SetSelectedFile( value );
            }
        }

        public IWildFile SelectedFile
        {
            get { return _selectedApp; }
            set 
            {
                SetSelectedFile( value );
            }
        }

        private void SetSelectedFile( IWildFile value )
        {
            _selectedApp = value;
            NotifyPropertyChanged( "SelectedApp" );
            NotifyPropertyChanged( "SelectedFile" );
            NotifyPropertyChanged( "IsValid" );
        }
    

        public VMCommand OpenFileDialog
        {
            get
            {
                if( _openFileDialog == null ) _openFileDialog = new VMCommand( (Action)OpenDialog );
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
            if( result == true )
            {
                // Open document 
                SelectedApp = _fileLauncher.FileLocator.CreateWildFile( dlg.FileName, false );
            }
        }


        void NotifyPropertyChanged( string peropertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( peropertyName ) );
        }

        #region IProtocolParameterManager Members

        public void FillFromString( string parameter )
        {
            _fileLauncher.LoadFromCommand( parameter, (file) => {
                SelectedApp = file;
                if( file.Lookup != FileLookup.Registry ) ShowApp = false;
                else
                {
                    ShowApp = true;
                    SelectedApp = Apps.FirstOrDefault( f => f.CompareTo(file) == 0 );
                    if( _selectedApp == null ) SelectedApp = file;
                }
            } );
            return;
        }

        public string GetParameterString()
        {
            return _fileLauncher.FileLocator.GetLocationCommand( _selectedApp );
        }

        public bool IsValid
        {
            get { return ShowApp && SelectedApp != null || !ShowApp && SelectedFile != null; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

}
