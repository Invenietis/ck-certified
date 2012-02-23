using CK.Windows.App;
using System;
using System.Windows.Input;
using CK.WPF.ViewModel;
using System.IO;
using System.Diagnostics;
using CK.Context;
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
            _hostInfo = ( (IHostInformation)VMIContext.Context.ServiceContainer.GetService( typeof( IHostInformation ) ) );
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
                            Process.Start( f.FullName );
                        } );
                }
                return _openFileCmd;
            }
        }

        void OnSystemConfigurationChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if ( e.PropertyName == "CurrentUserProfile" ) OnPropertyChanged( "UserConfigurationPath" );
        }

        void OnUserConfigurationChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if ( e.PropertyName == "CurrentContextProfile" ) OnPropertyChanged( "ContextPath" );
        }

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