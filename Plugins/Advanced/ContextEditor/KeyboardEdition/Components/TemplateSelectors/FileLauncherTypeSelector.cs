using System;
using System.Windows;
using System.Windows.Controls;
using BasicCommandHandlers;

namespace KeyboardEditor.ViewModels
{
    public class FileLauncherTypeSelector : DataTemplateSelector
    {
        public DataTemplate UrlTemplate { get; set; }
        public DataTemplate RegistryTemplate { get; set; }
        public DataTemplate BrowseTemplate { get; set; }

        public override DataTemplate SelectTemplate( object item,
          DependencyObject container )
        {
            if( item == null ) return RegistryTemplate;

            FileLauncherTypeSelection selection = (FileLauncherTypeSelection) item;
            if( selection == null ) throw new InvalidOperationException("Expected a FileLauncherTypeSelection but was something else");
            
            if( selection.Type == FileLauncherType.Url ) return UrlTemplate;
            if( selection.Type == FileLauncherType.Registry ) return RegistryTemplate;
            return BrowseTemplate;
        }
    }
}
