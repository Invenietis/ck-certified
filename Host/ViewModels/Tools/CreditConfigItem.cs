using System.ComponentModel;
using CK.Windows.Config;

namespace Host.VM
{
    public class CreditConfigItem : ConfigItemLink
    {
        public CreditConfigItem( ConfigManager configManager, ConfigPage target, INotifyPropertyChanged monitor )
            : base( configManager, target, monitor )
        {
        }
    }
}
