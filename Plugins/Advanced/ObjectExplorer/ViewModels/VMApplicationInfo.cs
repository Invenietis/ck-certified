using CK.Windows.App;
using System;
namespace CK.Plugins.ObjectExplorer
{
    public class VMApplicationInfo : VMISelectableElement, IDisposable
    {
        public VMApplicationInfo( VMIContextViewModel ctx )
            : base( ctx, null )
        {
            //LEAKING
            VMIContext.Context.ConfigManager.UserConfiguration.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler( OnUserConfigurationChanged );
            VMIContext.Context.ConfigManager.SystemConfiguration.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler( OnSystemConfigurationChanged ); 
        }

        void OnSystemConfigurationChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if ( e.PropertyName == "CurrentUserProfile" ) OnPropertyChanged( "UserConfigurationPath" );
        }

        void OnUserConfigurationChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if ( e.PropertyName == "CurrentContextProfile" ) OnPropertyChanged( "ContextPath" );
        }

        public string SystemConfigurationPath { get { return System.IO.Path.Combine( CKApp.CurrentParameters.CommonApplicationDataPath, "System.config.ck" ); } }

        public string UserConfigurationPath { get { return VMIContext.Context.ConfigManager.SystemConfiguration.CurrentUserProfile.Address.AbsolutePath; } }

        public string ContextPath { get { return VMIContext.Context.ConfigManager.UserConfiguration.CurrentContextProfile.Address.AbsolutePath; } }      

        public object Data { get { return this; } }

        public void Dispose()
        {
            VMIContext.Context.ConfigManager.UserConfiguration.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler( OnUserConfigurationChanged );
            VMIContext.Context.ConfigManager.SystemConfiguration.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler( OnSystemConfigurationChanged ); 
        }
    }
}
