using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Windows.Config;

namespace Host.VM
{
    public class TextItem : ConfigItem
    {
        public TextItem( ConfigManager configManager, string text )
            : base( configManager )
        {
            Text = text;
        }

        public string Text { get; set; }
    }
}
