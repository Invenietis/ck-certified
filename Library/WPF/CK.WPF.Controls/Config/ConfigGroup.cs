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
    public class ConfigGroup : ConfigItem, IConfigItemContainer
    {
        public ConfigGroup( ConfigManager configManager )
            : base( configManager )
        {
            Items = new ConfigItemCollection( this );
        }

        public IObservableCollection<object> Items { get; private set; }

    }
}
