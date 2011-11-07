using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Windows;

namespace Host
{
    public class AppViewModel : Conductor<IScreen>
    {
        bool _forceClose;
        bool _closing;

        public AppViewModel()
        {
            CivikeyHost.Context.ApplicationExited += ( o, e ) => ExitHost( e.HostShouldExit );
            CivikeyHost.Context.ApplicationExiting += new EventHandler<CK.Context.ApplicationExitingEventArgs>( OnBeforeExitApplication );
        }

        public CivikeyStandardHost CivikeyHost { get { return CivikeyStandardHost.Instance; } }

        void ExitHost( bool hostShouldExit )
        {
            if( hostShouldExit )
            {
                var thisView = GetView( null ) as Window;
                if( thisView != null ) thisView.Hide();
            }

            CivikeyHost.SaveContext();
            CivikeyHost.SaveUserConfig();
            CivikeyHost.SaveSystemConfig();

            if( hostShouldExit )
            {
                _forceClose = true;
                this.TryClose();
            }
        }

        void OnBeforeExitApplication( object sender, CK.Context.ApplicationExitingEventArgs e )
        {
            if( !_closing )
            {
                _closing = true;
                Window thisView = GetView( null ) as Window;
                Window bestParent = App.Current.GetTopWindow();
                e.Cancel = System.Windows.MessageBox.Show( bestParent, "Are you sure to exit application ?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question ) != MessageBoxResult.Yes;
                if( bestParent != thisView ) thisView.Activate();
                _closing = !e.Cancel;
            }
            else
                e.Cancel = true;
        }
    }
}
