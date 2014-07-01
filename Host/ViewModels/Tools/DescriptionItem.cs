using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Windows.Config;

namespace Host.VM
{
    public class DescriptionItem : TextItem
    {
        public DescriptionItem( ConfigManager configManager, string text, int fontSize = 11 )
            : base( configManager, text, fontSize )
        {
        }
    }
}
