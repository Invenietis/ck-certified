using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using CK.Reflection;
using CK.Windows.App;
using CK.Windows.Config;

namespace Host.VM
{
    public class CancelConfigItem<T> : ConfigItem
    {
        ValueProperty<T> _value;
        INotifyPropertyChanged _monitor;
        T _defaultValue;


        public CancelConfigItem( ConfigManager configManager, object o, PropertyInfo p, INotifyPropertyChanged monitor, T defaultValue )
            : base( configManager )
        {
            _defaultValue = defaultValue;
            _resetValueCommand = new VMCommand( ResetValue );

            _value = new ValueProperty<T>( o, p );
            _monitor = monitor;
            if( _monitor != null ) _monitor.PropertyChanged += new PropertyChangedEventHandler( _monitor_PropertyChanged );
            DisplayName = _value.PropertyInfo.Name;
        }

        public CancelConfigItem( ConfigManager configManager, object o, PropertyInfo p, T defaultValue )
            : this( configManager, o, p, o as INotifyPropertyChanged, defaultValue )
        {
        }

        public CancelConfigItem( ConfigManager configManager, ConfigItemProperty<T> item, T defaultValue )
            : this( configManager, item, ReflectionHelper.GetPropertyInfo( item, i => i.Value ), item, defaultValue )
        {
            ContentItem = item;
        }

        void _monitor_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == _value.PropertyInfo.Name ) NotifyOfPropertyChange( "Value" );
        }

        ConfigItem _contentItem;
        public ConfigItem ContentItem
        {
            get { return _contentItem; }
            set
            {
                if( _contentItem != value )
                {
                    _contentItem = value;
                    if( _monitor == null ) NotifyOfPropertyChange( "ContentItem" );
                }
            }
        }

        public T DefaultValue
        {
            set
            {
                if( OnSetDefaultValue( value ) )
                {
                    _defaultValue = value;
                    if( _monitor == null ) NotifyOfPropertyChange( "DefaultValue" );
                }
            }
            get { return _defaultValue; }
        }

        void ResetValue()
        {
            if( OnResetValue( _value.Get(), _defaultValue ) )
            {
                _value.Set( _defaultValue );
                if( _monitor == null ) NotifyOfPropertyChange( "Value" );
            }
        }

        protected virtual bool OnResetValue( T currentValue, T defaultValue )
        {
            return true;
        }
        protected virtual bool OnSetDefaultValue( T value )
        {
            return true;
        }


        VMCommand _resetValueCommand;
        public ICommand ResetValueCommand
        {
            get { return _resetValueCommand; }
        }
    }
}
