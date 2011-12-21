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

namespace CK.WPF.Controls
{

    public class ConfigPage : Screen, IConfigItemContainer
    {
        ConfigManager _configManager;
        string _description;
        BindableCollection<object> _items;

        public ConfigPage( ConfigManager configManager )
        {
            _configManager = configManager;
            _items = new BindableCollection<object>();
        }

        public ConfigManager ConfigManager { get { return _configManager; } }

        public string Description
        {
            get { return _description; }
            set
            {
                if( _description != value )
                {
                    _description = value;
                    NotifyOfPropertyChange( "Description" );
                }
            }
        }

        public IObservableCollection<object> Items { get { return _items; } }

    }


}
