using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace CK.Plugins.ObjectExplorer.UI.TemplateSelectors
{
    public class ShortDisplay : DataTemplateSelector
    {
        public DataTemplate CoreElementShortDisplay { get; set; }
        public DataTemplate FolderShortDisplay { get; set; }

        public override DataTemplate SelectTemplate( object item, System.Windows.DependencyObject container )
        {
            if( item is VMICoreElement )
                return CoreElementShortDisplay;
            return base.SelectTemplate( item, container );
        }
    }
}
