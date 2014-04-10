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
using CK.Windows.Config;

namespace Host.VM
{
    public class ConfigImplementationSelectorItem : ConfigItem, INotifyPropertyChanged
    {
        PluginCluster _cluster;
        Guid _pluginEditor;
        string _groupName;
        AppViewModel _app;

        public ConfigImplementationSelectorItem( ConfigManager configManager, PluginCluster pluginCluster )
            : base( configManager )
        {
            _pluginEditor = Guid.Empty;
            _cluster = pluginCluster;
        }

        public ConfigImplementationSelectorItem( ConfigManager configManager, PluginCluster pluginCluster, Guid pluginEditor )
            : this( configManager, pluginCluster )
        {
            _pluginEditor = pluginEditor;
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
                    CloseEditor( value );
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
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

        bool CanOpenEditor()
        {
            return _pluginEditor != Guid.Empty && IsSelected && _app != null && !_editorIsOpen;
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
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( _pluginEditor, ConfigUserAction.Started );
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.Changed += LiveUserConfiguration_Changed;
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }

        private void StopPluginEditor()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( _pluginEditor, ConfigUserAction.Stopped );
            _app.CivikeyHost.Context.PluginRunner.Apply();
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
