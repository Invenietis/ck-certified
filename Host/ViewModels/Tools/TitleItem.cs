using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Windows.Config;

namespace Host.VM
{
    public class TitleItem : TextItem
    {
        public TitleItem( ConfigManager configManager, string text, int fontSize = 18 )
            : base( configManager, text, fontSize )
        {
        }
    }
}
