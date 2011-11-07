using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace CK.WPF.Controls
{
    public class ConfigItemMillisecondProperty : ConfigItemProperty<int>
    {
        public ConfigItemMillisecondProperty( ConfigManager configManager, ValueProperty<int> prop, INotifyPropertyChanged monitor )
            : base( configManager, prop, monitor )
        {
        }

        public ConfigItemMillisecondProperty( ConfigManager configManager, object o, PropertyInfo p )
            : this( configManager, new ValueProperty<int>( o, p ), o as INotifyPropertyChanged )
        {
        }

        public ConfigItemMillisecondProperty( ConfigManager configManager, object o, PropertyInfo p, INotifyPropertyChanged monitor )
            : this( configManager, new ValueProperty<int>( o, p ), monitor )
        {
        }
    }
}
