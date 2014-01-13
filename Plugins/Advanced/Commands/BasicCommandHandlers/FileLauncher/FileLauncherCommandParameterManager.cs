using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Animation;
using CK.WPF.ViewModel;
using CommonServices;
using ProtocolManagerModel;
using CK.Plugin.Config;

namespace BasicCommandHandlers
{
    public class FileLauncherCommandParameterManager : IProtocolParameterManager
    {
        public IProtocolEditorRoot Root { get; set; }

        readonly string[] _trustedCompanies = { "Adobe", "Microsoft", "Google", "Mozilla", "Apple" };
        readonly IFileLauncherService _fileLauncher;
        readonly IPluginConfigAccessor _skinConfigAccessor;

        VMCommand _openFileDialog;
        IWildFile _selectedWildFile;

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

        //private bool _isCurrentImageFromIcon;
        //bool _useFileIcon;
        //public bool UseFileIcon
        //{
        //    get { return _useFileIcon; }
        //    set
        //    {
        //        if( _useFileIcon != value )
        //        {
        //            _useFileIcon = value;
        //            NotifyPropertyChanged( "UseFileIcon" );
        //            if( value && _selectedWildFile != null )
        //            {
        //                //Set the icon
        //                _skinConfigAccessor[Root.EditedKeyMode].Set( "Image", _selectedWildFile.Icon );
        //                _skinConfigAccessor[Root.EditedKeyMode].Set( "DisplayType", "Image" );
        //                _isCurrentImageFromIcon = true;
        //            }
        //            else
        //            {
        //                //Remove the icon
        //            }
        //        }
        //    }
        //}

        public FileLauncherCommandParameterManager( IFileLauncherService fileLauncher, IPluginConfigAccessor skinConfigAccessor )
        {
            _fileLauncher = fileLauncher;
            _skinConfigAccessor = skinConfigAccessor;
            Apps = _fileLauncher.FileLocator.RegistryApps;

            Apps.Sort( ( a, b ) =>
            {
                var vA = FileVersionInfo.GetVersionInfo( a.Path );
                var vB = FileVersionInfo.GetVersionInfo( b.Path );
                if( vA == null || vA.ProductName == null ) return 1;
                if( vB == null || vB.ProductName == null ) return -1;

                foreach( string company in _trustedCompanies )
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
                if( _selectedWildFile != null )
                {
                    return _selectedWildFile.Lookup == FileLookup.Registry ? _selectedWildFile : null;
                }
                return _selectedWildFile;
            }
            set
            {
                SetSelectedFile( value );
            }
        }

        public IWildFile SelectedFile
        {
            get { return _selectedWildFile; }
            set
            {
                SetSelectedFile( value );
            }
        }

        private void SetSelectedFile( IWildFile value )
        {
            _selectedWildFile = value;
            //if( UseFileIcon )
            //{
            _skinConfigAccessor[Root.EditedKeyMode].Set( "Image", _selectedWildFile.Icon );
            _skinConfigAccessor[Root.EditedKeyMode].Set( "DisplayType", "Image" );
            //}

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
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Tous le fichiers (*.*)|*.*" };

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
            _fileLauncher.LoadFromCommand( parameter, ( file ) =>
            {
                SelectedApp = file;
                if( file.Lookup != FileLookup.Registry ) ShowApp = false;
                else
                {
                    ShowApp = true;
                    SelectedApp = Apps.FirstOrDefault( f => f.CompareTo( file ) == 0 );
                    if( _selectedWildFile == null ) SelectedApp = file;
                }
            } );
            return;
        }

        public string GetParameterString()
        {
            return _fileLauncher.FileLocator.GetLocationCommand( _selectedWildFile );
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
