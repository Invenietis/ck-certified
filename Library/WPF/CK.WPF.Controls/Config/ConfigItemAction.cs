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
    public class ConfigItemAction : ConfigItem
    {
        public ConfigItemAction( ConfigManager configManager, ICommand cmd )
            : base( configManager )
        {
            ActionCommand = cmd;
        }

        public ICommand ActionCommand { get; set; }
    }
}
