using BasicCommandHandlers;
using ProtocolManagerModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace KeyboardEditor.ViewModels
{
    public class AddActionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SendStringDataTemplate { get; set; }
        public DataTemplate MoveMouseDataTemplate { get; set; }
        public DataTemplate ComboBoxDataTemplate { get; set; }
        public DataTemplate FileLauncherDataTempalte { get; set; }
        public DataTemplate ModeDataTemplate { get; set; }
        public DataTemplate DefaultTemplate { get; set; }

        public override System.Windows.DataTemplate SelectTemplate( object item, System.Windows.DependencyObject container )
        {
            if( item == null ) return null;
            if( !( item is IProtocolParameterManager ) ) throw new ArgumentException( "The DataContext of the AddActionDataTemplateSelector must be a IProtocolParameterManager" );
            
            if( item is ChangeKeyboardCommandParameterManager ) return ComboBoxDataTemplate;
            if( item is SendKeyCommandParameterManager ) return ComboBoxDataTemplate;
            if( item is ClickCommandParameterManager ) return ComboBoxDataTemplate;
            if( item is HelpCommandParameterManager ) return ComboBoxDataTemplate;
            if( item is DynCommandParameterManager ) return ComboBoxDataTemplate;

            if( item is SimpleKeyCommandParameterManager ) return SendStringDataTemplate;
            if( item is MoveMouseCommandParameterManager ) return MoveMouseDataTemplate;
            if( item is FileLauncherCommandParameterManager ) return FileLauncherDataTempalte;
        
            if( item is ModeCommandParameterManager ) return ModeDataTemplate;

            //throw new ArgumentException( String.Format( "The Bound object of the AddActionDataTemplateSelector : \"{0}\" is not of a recognized type.", item.GetType() ) );
            return DefaultTemplate;
        }
    }
}
