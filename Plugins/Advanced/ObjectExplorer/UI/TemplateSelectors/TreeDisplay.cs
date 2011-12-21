using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace CK.Plugins.ObjectExplorer.UI.TemplateSelectors
{
    public class CommentTreeDisplay : DataTemplateSelector
    {
        public DataTemplate CoreElementTreeDisplay { get; set; }
        public DataTemplate FolderTreeDisplay { get; set; }

        public override DataTemplate SelectTemplate( object item, System.Windows.DependencyObject container )
        {
            if( item is VMICoreElement )
                return CoreElementTreeDisplay;
            return base.SelectTemplate( item, container );
        }
    }
}
