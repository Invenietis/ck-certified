using System.Windows;
using System.Windows.Controls;

namespace KeyboardEditor.ViewModels
{
    public class KeyboardEditionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate KeyEditionTemplate { get; set; }
        public DataTemplate ZoneEditionTemplate { get; set; }
        public DataTemplate KeyboardEditionTemplate { get; set; }
        public DataTemplate KeyModeEditionTemplate { get; set; }
        public DataTemplate LayoutKeyModeEditionTemplate { get; set; }

        public override DataTemplate SelectTemplate( object item,
          DependencyObject container )
        {
            if( item is VMKeyEditable ) return KeyEditionTemplate;
            else if( item is VMZoneEditable ) return ZoneEditionTemplate;
            else if( item is VMKeyModeEditable ) return KeyModeEditionTemplate;
            else if( item is VMLayoutKeyModeEditable ) return LayoutKeyModeEditionTemplate;
            else return KeyboardEditionTemplate;
        }
    }
}
