using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace CK.StandardPlugins.ObjectExplorer.UI.TemplateSelectors
{
    public class DetailsDisplay : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate( object item, System.Windows.DependencyObject container )
        {
            VMIBase inspector = item as VMIBase;
            if( inspector != null && inspector.DetailsTemplateName != null )
            {
                var finder = container as FrameworkElement;
                if( finder != null )
                    return finder.FindResource( inspector.DetailsTemplateName ) as DataTemplate;
            }
            return base.SelectTemplate( item, container );
        }
    }
}
