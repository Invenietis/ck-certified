#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\Tools\ConfigImplementationSelectorItem.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows.App;
using CK.Windows.Config;

namespace Host.VM
{
    public class ConfigImplementationSelectorItem : ConfigItem, INotifySelectionChanged
    {
        PluginCluster _cluster;
        Guid _pluginEditor = Guid.Empty;
        string _groupName = string.Empty;
        ConfigPage _configPage;

        public ConfigImplementationSelectorItem( ConfigManager configManager, PluginCluster pluginCluster )
            : base( configManager )
        {
            _cluster = pluginCluster;
            SelectItem = new VMCommand( OnSelectItem, CanSelectItem );
            OpenEditor = new VMCommand( StartPluginEditor, () => CanOpenEditor );
        }

        public ConfigImplementationSelectorItem( ConfigManager configManager, PluginCluster pluginCluster, string groupName )
            : this( configManager, pluginCluster )
        {
            _groupName = groupName;
        }

        public ConfigImplementationSelectorItem( ConfigManager configManager, PluginCluster pluginCluster, Guid pluginEditor )
            : this( configManager, pluginCluster )
        {
            _pluginEditor = pluginEditor;
        }

        public ConfigImplementationSelectorItem( ConfigManager configManager, PluginCluster pluginCluster, ConfigPage configPage )
            : this( configManager, pluginCluster )
        {
            _configPage = configPage;
        }

        public ConfigImplementationSelectorItem( ConfigManager configManager, PluginCluster pluginCluster, Guid pluginEditor, string groupName )
            : this( configManager, pluginCluster, pluginEditor )
        {
            _groupName = groupName;
        }

        public ConfigImplementationSelectorItem( ConfigManager configManager, PluginCluster pluginCluster, ConfigPage configPage, string groupName )
            : this( configManager, pluginCluster, configPage )
        {
            _groupName = groupName;
        }

        public Action SelectAction { get; set; }
        public Action DeselectAction { get; set; }

        public PluginCluster PluginCluster { get { return _cluster; } }

        bool _isSelected;
        public bool IsSelected
        {
            get 
            {
                return _isSelected;
            }
            set
            {
                if( _isSelected != value )
                {
                    CallActions( value );
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        bool _isDefaultItem;
        public bool IsDefaultItem 
        {
            get { return _isDefaultItem; }
            set
            {
                if( _isDefaultItem != value )
                {
                    _isDefaultItem = value;
                    _isSelected = value;
                }
            }
        }

        public bool IsRadioButton
        {
            get { return !string.IsNullOrEmpty( _groupName ); }
        }

        public string GroupName
        { 
            get { return _groupName; }
        }

        private void CallActions(bool newValue)
        {
            if( newValue && SelectAction != null ) SelectAction();
            else if( DeselectAction != null ) DeselectAction();
        }

        #region PluginEditor Members

        public ICommand OpenEditor
        {
            get;
            internal set;
        }

        public bool CanOpenEditor
        {
            get { return ( _pluginEditor != Guid.Empty && !_editorIsOpen ) || _configPage != null; }
        }

        bool _editorIsOpen;
        public bool EditorIsOpen 
        { 
            get { return _editorIsOpen; }
            private set
            {
                if( _editorIsOpen != value )
                {
                    _editorIsOpen = value;
                    OnPropertyChanged();
                }
            }
        }

        private void StartPluginEditor()
        {
            if( _pluginEditor != Guid.Empty )
            {
                _cluster.UserConfig.LiveUserConfiguration.SetAction( _pluginEditor, ConfigUserAction.Started );
                _cluster.UserConfig.LiveUserConfiguration.Changed += LiveUserConfiguration_Changed;
                _cluster.ApplyNewConfig();
            }
            else
            {
                ConfigManager.ActivateItem( _configPage );
            }
        }

        private void StopPluginEditor()
        {
            _cluster.UserConfig.LiveUserConfiguration.SetAction( _pluginEditor, ConfigUserAction.Stopped );
            _cluster.ApplyNewConfig();
            _cluster.UserConfig.LiveUserConfiguration.Changed -= LiveUserConfiguration_Changed;
        }

        private void LiveUserConfiguration_Changed( object sender, LiveUserConfigurationChangedEventArgs e )
        {
            if( e.PluginID == _pluginEditor ) EditorIsOpen = e.Action == ConfigUserAction.Started;
        }

        private void CloseEditor( bool newValue )
        {
            if( !newValue && _editorIsOpen ) StopPluginEditor();
        }

        #endregion PluginEditor Members

        public ICommand SelectItem
        {
            get;
            private set;
        }

        private bool CanSelectItem()
        {
            return true;
        }

        private void OnSelectItem()
        {
            if( !Enabled ) return;

            if( IsRadioButton && IsSelected ) return;
            IsSelected = !_isSelected;
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if( handler != null )
            {
                var e = new PropertyChangedEventArgs( propertyName );
                handler( this, e );
            }
        }

        #endregion INotifyPropertyChanged Members
    }
}
