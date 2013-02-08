using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ContextEditor.ViewModels
{
    public class KeyboardEditionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate KeyEditionTemplate { get; set; }
        public DataTemplate ZoneEditionTemplate { get; set; }
        public DataTemplate KeyboardEditionTemplate { get; set; }

        public override DataTemplate SelectTemplate( object item,
          DependencyObject container )
        {
            if( item as VMKeyEditable != null ) return KeyEditionTemplate;
            else if( item as VMZoneEditable != null ) return ZoneEditionTemplate;
            else return KeyboardEditionTemplate;
        }
    }
}
