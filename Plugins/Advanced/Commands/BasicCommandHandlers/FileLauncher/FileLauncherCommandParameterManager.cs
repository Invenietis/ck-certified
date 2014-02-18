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
    public enum FileLauncherType
    {
        Url,
        Registry,
        Browse
    }

    public class FileLauncherTypeSelection : INotifyPropertyChanged
    {
        bool _isSelected;
        VMCommand _openFileDialog;

        public FileLauncherCommandParameterManager Manager { get; set; }
        public FileLauncherType Type { get; set; }
        public string Content { get; set; }
        public List<IWildFile> Apps { get; set; }

        public bool IsSelected 
        {
            get { return _isSelected; } 
            set
            {
                _isSelected = value;
                if( value ) Manager.SelectedFileLauncherType = this;
                NotifyPropertyChanged( "IsSelected" );
            }
        }

        public VMCommand OpenFileDialog
        {
            get
            {
                if( _openFileDialog == null ) _openFileDialog = new VMCommand( (Action)OpenDialog );
                return _openFileDialog;
            }
        }
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        void NotifyPropertyChanged( string peropertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( peropertyName ) );
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
                Manager.SetSelectedFile(dlg.FileName);
            }
        }
    }

    public class FileLauncherCommandParameterManager : IProtocolParameterManager
    {
        public IProtocolEditorRoot Root { get; set; }

        readonly string[] _trustedCompanies = { "Adobe", "Microsoft", "Google", "Mozilla", "Apple" };
        internal readonly IFileLauncherService _fileLauncher;
        readonly IPluginConfigAccessor _skinConfigAccessor;
        FileLauncherTypeSelection _selectedFileLauncherType;
        
        IWildFile _selectedWildFile;
        public List<FileLauncherTypeSelection> TypeSelections { get; set; }
        public FileLauncherTypeSelection SelectedFileLauncherType 
        {
            get { return _selectedFileLauncherType; }
            set
            {
                _selectedFileLauncherType = value;
                NotifyPropertyChanged( "SelectedFileLauncherType" );
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
            TypeSelections = new List<FileLauncherTypeSelection>();
            SelectedFileLauncherType = new FileLauncherTypeSelection { Type = FileLauncherType.Registry, Apps = _fileLauncher.FileLocator.RegistryApps, Manager = this};
            TypeSelections.Add( SelectedFileLauncherType );
            TypeSelections.Add( new FileLauncherTypeSelection { Type = FileLauncherType.Registry, Apps = _fileLauncher.FileLocator.RegistryApps, Manager = this } );
            TypeSelections.Add( new FileLauncherTypeSelection { Type = FileLauncherType.Browse, Apps = _fileLauncher.FileLocator.RegistryApps, Manager = this } );
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

        internal void SetSelectedFile( string path )
        {
            SetSelectedFile( _fileLauncher.FileLocator.CreateWildFile( path, false ) );
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
                if( file.Lookup == FileLookup.Registry )
                {
                    SelectedFileLauncherType = TypeSelections.FirstOrDefault( t => t.Type == FileLauncherType.Registry);
                    SelectedApp = _fileLauncher.FileLocator.RegistryApps.FirstOrDefault( f => f.CompareTo( file ) == 0 );
                    if( _selectedWildFile == null ) SelectedApp = file;
                }
                else if(file.Lookup == FileLookup.Url)
                {
                    SelectedFileLauncherType = TypeSelections.FirstOrDefault( t => t.Type == FileLauncherType.Url );
                }
                else
                {
                    SelectedFileLauncherType = TypeSelections.FirstOrDefault( t => t.Type == FileLauncherType.Browse );
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
            get 
            { 
                return SelectedFileLauncherType.Type == FileLauncherType.Registry && SelectedApp != null
                    || SelectedFileLauncherType.Type == FileLauncherType.Browse &&  SelectedFile != null;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

}
