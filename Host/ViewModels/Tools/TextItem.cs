using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Windows.Config;

namespace Host.VM
{
    public class TextItem : ConfigItem
    {
        public TextItem( ConfigManager configManager, string text, int fontSize = 13 )
            : base( configManager )
        {
            FontSize = fontSize;
            Text = text;
        }

        public int FontSize { get; private set; }
        public string Text { get; set; }
    }
}
