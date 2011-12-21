using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Reflection;
using System.ComponentModel;
using CK.Reflection;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Input;

namespace CK.WPF.Controls
{

    public class ConfigItemLink : ConfigItem, ICommand
    {
        public ConfigPage _target;

        public ConfigItemLink( ConfigManager configManager, ConfigPage target, INotifyPropertyChanged monitor )
            : base( configManager )
        {
            if( target == null ) throw new ArgumentNullException( "target" );
            DisplayName = target.DisplayName;
            Description = target.Description;
            _target = target;
            if( monitor != null )
            {
                monitor.PropertyChanged += ( o, e ) =>
                {
                    if( e.PropertyName == "DisplayName" ) DisplayName = _target.DisplayName;
                    if( e.PropertyName == "Description" ) Description = _target.Description;
                };
            }

        }

        public ICommand GotoCommand { get { return this; } }

        public void Goto()
        {
            ConfigManager.ActivateItem( _target );
        }

        bool ICommand.CanExecute( object parameter )
        {
            return true;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }

        void ICommand.Execute( object parameter )
        {
            Goto();
        }

    }
}
