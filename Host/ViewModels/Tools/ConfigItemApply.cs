using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CK.Windows.Config;

namespace Host.VM
{
    public class ConfigItemApply : ConfigItemAction
    {
        public ConfigItemApply( ConfigManager configManager, ICommand cmd )
            : base( configManager, cmd )
        {
        }
    }
}
