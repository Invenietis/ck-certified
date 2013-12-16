using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace KeyboardEditor.ViewModels
{
    public class BreadcrumbTemplateSelector : DataTemplateSelector
    {
        public DataTemplate KeyModeBaseTemplate { get; set; }
        public DataTemplate OtherTemplate { get; set; }

        public override DataTemplate SelectTemplate( object item,
          DependencyObject container )
        {
            if( item is VMKeyModeBase ) return KeyModeBaseTemplate;
            else return OtherTemplate;
        }
    }
}
