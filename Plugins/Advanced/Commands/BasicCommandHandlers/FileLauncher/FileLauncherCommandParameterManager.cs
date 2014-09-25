#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\FileLauncher\FileLauncherCommandParameterManager.cs) is part of CiviKey. 
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
using System.ComponentModel;
using System.Linq;
using CK.Plugin.Config;
using CK.WPF.ViewModel;
using CommonServices;
using ProtocolManagerModel;

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
        string _appPath;

        public FileLauncherCommandParameterManager Manager { get; set; }
        public FileLauncherType Type { get; private set; }
        public string Content { get; private set; }
        public string AppPath 
        {
            get { return _appPath; }
            set 
            { 
                _appPath = value;
                if( string.IsNullOrEmpty( _appPath ) ) Manager.SelectedFile = null;
                
                NotifyPropertyChanged( "AppPath" );
                Manager.SetUrlApp(_appPath);
            } 
        }

        public FileLauncherTypeSelection( FileLauncherCommandParameterManager manager, FileLauncherType type, string content )
        {
            Manager = manager;
            Type = type;
            Content = content;
            _appPath = "";
        }

        public List<IWildFile> Apps 
        {
            get { return Manager._fileLauncher.FileLocator.RegistryApps; } 
        }

        public bool IsSelected 
        {
            get { return _isSelected; } 
            set
            {
                _isSelected = value;
                if( value )
                {
                    Manager.SelectedFileLauncherType = this;
                    if( Type == FileLauncherType.Url ) Manager.SetUrlApp(AppPath);
                }
                else
                {
                    if( Type == FileLauncherType.Url ) Manager.SelectedFile = null;
                }
                
                
                NotifyPropertyChanged( "IsSelected" );
            }
        }
        public void FLush()
        {

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
            SelectedFileLauncherType = new FileLauncherTypeSelection( this, FileLauncherType.Url, "URL" );
            SelectedFileLauncherType.IsSelected = true;
            TypeSelections.Add( SelectedFileLauncherType );
            TypeSelections.Add( new FileLauncherTypeSelection (this, FileLauncherType.Registry, "Application installée"));
            TypeSelections.Add( new FileLauncherTypeSelection( this, FileLauncherType.Browse, "Choisir un fichier" ) );
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

        internal void SetUrlApp(string url)
        {
            url = url.Trim().ToLowerInvariant();
            if( !(url.StartsWith( "http://" ) 
                || url.StartsWith( "https://" ) 
                || url.StartsWith( "file://" ) 
                || url.StartsWith( "ftp://" )) )
            {
                url = "http://" + url;
            }

            SetSelectedFile(_fileLauncher.FileLocator.CreateWildFile(url, false), false);
        }

        private void SetSelectedFile( IWildFile value, bool useImg = true )
        {
            _selectedWildFile = value;

            if( useImg && value != null)
            {
                _skinConfigAccessor[Root.EditedKeyMode].Set( "Image", _selectedWildFile.Icon );
                _skinConfigAccessor[Root.EditedKeyMode].Set( "DisplayType", "Image" );
            }

            NotifyPropertyChanged( "SelectedApp" );
            NotifyPropertyChanged( "SelectedFile" );
            NotifyPropertyChanged( "IsValid" );
        }

        internal void SetSelectedFile( string path )
        {
            SetSelectedFile( _fileLauncher.FileLocator.CreateWildFile( path, false ) );
        }

        internal void NotifyPropertyChanged( string peropertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( peropertyName ) );
        }

        #region IProtocolParameterManager Members

        public void FillFromString( string parameter )
        {
            _fileLauncher.LoadFromCommand( parameter, ( file ) =>
            {
                SelectedApp = file;
                TypeSelections.FirstOrDefault( x => x.IsSelected == true ).IsSelected = false;

                if( file.Lookup == FileLookup.Registry )
                {
                    SelectedFileLauncherType = TypeSelections.FirstOrDefault( t => t.Type == FileLauncherType.Registry);
                    SelectedApp = _fileLauncher.FileLocator.RegistryApps.FirstOrDefault( f => f.CompareTo( file ) == 0 );
                    if( _selectedWildFile == null ) SelectedApp = file;
                }
                else if(file.Lookup == FileLookup.Url)
                {
                    SelectedFileLauncherType = TypeSelections.FirstOrDefault( t => t.Type == FileLauncherType.Url );
                    SelectedFileLauncherType.AppPath = file.Path;
                }
                else
                {
                    SelectedFileLauncherType = TypeSelections.FirstOrDefault( t => t.Type == FileLauncherType.Browse );
                    SelectedFile = file;
                }
                SelectedFileLauncherType.IsSelected = true;
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
                    || SelectedFileLauncherType.Type == FileLauncherType.Browse &&  SelectedFile != null
                    || SelectedFileLauncherType.Type == FileLauncherType.Url && SelectedFile != null && !string.IsNullOrEmpty( SelectedFileLauncherType.AppPath );
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

}
