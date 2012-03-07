using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using CK.WPF.ViewModel;
using CK.Plugin;
using CK.Plugins.ObjectExplorer.UI.UserControls;
using System.ComponentModel;

namespace CK.Plugins.ObjectExplorer.ViewModels.LogViewModels
{
    public partial class VMLogOutputContainer : VMBase
    {
        LogConsoleWindow _logConsoleWindow;
        ICommand _clearOutputConsoleCommand;
        ICommand _toggleMaximizedCommand;
        bool _consoleWindowIsClosed = true;
        public bool IsMaximized { get { return _logConsoleWindow.Visibility == System.Windows.Visibility.Visible; } }

        void OnLogConsoleWindowClosing( object sender, CancelEventArgs e )
        {
            e.Cancel = true;
            _logConsoleWindow.Visibility = System.Windows.Visibility.Collapsed;
            _consoleWindowIsClosed = true;
            OnPropertyChanged( "IsMaximized" );
        }

        public ICommand ToggleMaximizeCommand
        {
            get
            {
                if( _toggleMaximizedCommand == null )
                {
                    _toggleMaximizedCommand = new VMCommand( () =>
                    {
                        if( _consoleWindowIsClosed )
                        {
                            _logConsoleWindow.Show();
                            _consoleWindowIsClosed = false;
                        }
                        else
                        {
                            if( _logConsoleWindow.Visibility == System.Windows.Visibility.Visible ) _logConsoleWindow.Visibility = System.Windows.Visibility.Collapsed;
                            else _logConsoleWindow.Visibility = System.Windows.Visibility.Visible;
                        }
                        OnPropertyChanged( "IsMaximized" );
                    } );
                }
                return _toggleMaximizedCommand;
            }
        }
    }
}
