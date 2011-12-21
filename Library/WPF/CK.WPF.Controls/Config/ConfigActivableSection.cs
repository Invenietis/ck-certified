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
    public class ConfigActivableSection : ConfigItemProperty<bool>, IConfigActivableSection
    {
        public ConfigActivableSection( ConfigManager configManager, ValueProperty<bool> prop, INotifyPropertyChanged monitor )
            : base( configManager, prop, monitor )
        {
            Items = new ConfigItemCollection( this );
        }

        public ConfigActivableSection( ConfigManager configManager, object o, PropertyInfo p )
            : this( configManager, new ValueProperty<bool>( o, p ), o as INotifyPropertyChanged )
        {
        }

        public ConfigActivableSection( ConfigManager configManager, object o, PropertyInfo p, INotifyPropertyChanged monitor )
            : this( configManager, new ValueProperty<bool>( o, p ), monitor )
        {
        }

        public IObservableCollection<object> Items { get; private set; }
    }
}
