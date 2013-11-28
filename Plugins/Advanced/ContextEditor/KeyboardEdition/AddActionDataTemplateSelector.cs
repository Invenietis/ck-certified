﻿using BasicCommandHandlers;
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
        public DataTemplate SendKeyDataTemplate { get; set; }
        public DataTemplate SwitchKeyboardDataTemplate { get; set; }
        public DataTemplate MoveMouseDataTemplate { get; set; }

        public override System.Windows.DataTemplate SelectTemplate( object item, System.Windows.DependencyObject container )
        {
            if( item == null ) return null;
            if( !( item is IProtocolParameterManager ) ) throw new ArgumentException( "The DataContext of the AddActionDataTemplateSelector must be a KeyCommandViewModel" );
            IProtocolParameterManager keyCommandViewModel = item as IProtocolParameterManager;

            if( keyCommandViewModel is SimpleKeyCommandParameterManager ) return SendStringDataTemplate;
            if( keyCommandViewModel is SendKeyCommandParameterManager ) return SendKeyDataTemplate;
            if( keyCommandViewModel is ChangeKeyboardCommandParameterManager ) return SwitchKeyboardDataTemplate;
            if( keyCommandViewModel is MoveMouseCommandParameterManager ) return MoveMouseDataTemplate;

            throw new ArgumentException( String.Format( "The Bound object of the AddActionDataTemplateSelector : \"{0}\" is not of a recognized type.", keyCommandViewModel.GetType() ) );
        }
    }
}
