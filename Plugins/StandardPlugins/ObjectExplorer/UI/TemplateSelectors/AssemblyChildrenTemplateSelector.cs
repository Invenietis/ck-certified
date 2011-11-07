using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace CK.StandardPlugins.ObjectExplorer.UI.TemplateSelectors
{
    public class TreeNodeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PluginAliasTemplate { get; set; }
        public DataTemplate ServiceAliasTemplate { get; set; }

        public override DataTemplate SelectTemplate( object item, System.Windows.DependencyObject container )
        {
            if( item is VMAlias<VMIPlugin> ) return PluginAliasTemplate;
            else if( item is VMAlias<VMIService> ) return ServiceAliasTemplate;
            else return base.SelectTemplate( item, container );
        }
    }
}
