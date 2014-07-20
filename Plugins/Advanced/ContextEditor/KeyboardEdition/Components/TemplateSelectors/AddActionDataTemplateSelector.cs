using BasicCommandHandlers;
using ProtocolManagerModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace KeyboardEditor.ViewModels
{
    public class AddActionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SendStringDataTemplate { get; set; }
        public DataTemplate MoveMouseDataTemplate { get; set; }
        public DataTemplate ComboBoxDataTemplate { get; set; }
        public DataTemplate FileLauncherDataTemplate { get; set; }
        public DataTemplate ModeDataTemplate { get; set; }
        public DataTemplate DefaultDataTemplate { get; set; }
        public DataTemplate MonitorOnceDataTemplate { get; set; }
        public DataTemplate KeySequenceDataTemplate { get; set; }
        public DataTemplate TextTemplateDataTemplate { get; set; }
        public DataTemplate IntegerDataTemplate { get; set; }

        public override System.Windows.DataTemplate SelectTemplate( object item, System.Windows.DependencyObject container )
        {
            if( item == null ) return null;
            if( !( item is IProtocolParameterManager ) ) throw new ArgumentException( "The DataContext of the AddActionDataTemplateSelector must be a IProtocolParameterManager" );

            if( item is ChangeKeyboardCommandParameterManager ) return ComboBoxDataTemplate;
            if( item is SendKeyCommandParameterManager ) return ComboBoxDataTemplate;
            if( item is ClickCommandParameterManager ) return ComboBoxDataTemplate;
            if( item is HelpCommandParameterManager ) return ComboBoxDataTemplate;
            if( item is DynCommandParameterManager ) return ComboBoxDataTemplate;

            if( item is PauseCommandParameterManager ) return IntegerDataTemplate;

            if( item is SimpleKeyCommandParameterManager ) return SendStringDataTemplate;
            if( item is MoveMouseCommandParameterManager ) return MoveMouseDataTemplate;
            if( item is FileLauncherCommandParameterManager ) return FileLauncherDataTemplate;

            if( item is ModeCommandParameterManager ) return ModeDataTemplate;

            if( item is MonitorOnceCommandParameterManager ) return MonitorOnceDataTemplate;

            if( item is KeySequenceCommandParameterManager ) return KeySequenceDataTemplate;
            if( item is TextTemplateCommandParameterManager ) return TextTemplateDataTemplate;

            return DefaultDataTemplate;
        }
    }
}
