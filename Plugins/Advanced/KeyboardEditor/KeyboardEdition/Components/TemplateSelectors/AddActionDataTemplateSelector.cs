#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\Components\TemplateSelectors\AddActionDataTemplateSelector.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using BasicCommandHandlers;
using ProtocolManagerModel;

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
