#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\VMApplicationInfo.cs) is part of CiviKey. 
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
using System.Windows.Input;
using CK.WPF.ViewModel;
using System.IO;
using System.Diagnostics;
using CK.Context;
using System.Windows;
using CK.Windows.App;
namespace CK.Plugins.ObjectExplorer
{
    public class VMApplicationInfo : VMISelectableElement, IDisposable
    {
        IHostInformation _hostInfo;

        public VMApplicationInfo( VMIContextViewModel ctx )
            : base( ctx, null )
        {
            VMIContext.Context.ConfigManager.UserConfiguration.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler( OnUserConfigurationChanged );
            VMIContext.Context.ConfigManager.SystemConfiguration.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler( OnSystemConfigurationChanged );
            _hostInfo = ((IHostInformation)VMIContext.Context.ServiceContainer.GetService( typeof( IHostInformation ) ));
        }

        VMCommand<string> _openFileCmd;
        public VMCommand<string> OpenFileCommand
        {
            get
            {
                if( _openFileCmd == null )
                {
                    FileInfo f;
                    _openFileCmd = new VMCommand<string>( ( s ) =>
                        {
                            f = new FileInfo( s );
                            if( f.Exists )
                            {
                                if( f.Directory != null && f.Directory.Exists )
                                    Process.Start( f.Directory.FullName );
                                else
                                    Process.Start( f.FullName );
                            }
                        } );
                }
                return _openFileCmd;
            }
        }

        void OnSystemConfigurationChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "CurrentUserProfile" ) OnPropertyChanged( "UserConfigurationPath" );
        }

        void OnUserConfigurationChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "CurrentContextProfile" ) OnPropertyChanged( "ContextPath" );
        }

        ICommand _forceGCCommand;
        public ICommand ForceGCCommand
        {
            get
            {
                if( _forceGCCommand == null )
                {
                    _forceGCCommand = new CK.Windows.App.VMCommand( () =>
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            GC.Collect();
                        } );
                }
                return _forceGCCommand;
            }
        }

        public string DistributionName { get { return _hostInfo.SubAppName; } }

        public string Version { get { return _hostInfo.AppVersion.ToString(); } }

        public string ApplicationId { get { return _hostInfo.ApplicationUniqueId.UniqueId.ToString(); } }

        public string SystemConfigurationPath { get { return _hostInfo != null ? _hostInfo.GetSystemConfigAddress().AbsolutePath : ""; } }

        public string UserConfigurationPath { get { return VMIContext.Context.ConfigManager.SystemConfiguration.CurrentUserProfile.Address.AbsolutePath; } }

        public string ContextPath { get { return VMIContext.Context.ConfigManager.UserConfiguration.CurrentContextProfile.Address.AbsolutePath; } }

        public object Data { get { return this; } }

        public new void Dispose()
        {
            VMIContext.Context.ConfigManager.UserConfiguration.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler( OnUserConfigurationChanged );
            VMIContext.Context.ConfigManager.SystemConfiguration.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler( OnSystemConfigurationChanged );
            base.Dispose();
        }
    }
}