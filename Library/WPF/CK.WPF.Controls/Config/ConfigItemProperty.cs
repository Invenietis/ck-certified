using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Reflection;
using System.ComponentModel;
using CK.Reflection;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections;

namespace CK.WPF.Controls
{
    public interface IValueGetterSetter<T>
    {
        void Set( T value );
        T Get();
    }

    public struct ValueProperty<T> : IValueGetterSetter<T>
    {
        readonly object _o;
        readonly PropertyInfo _p;

        public ValueProperty( object o, PropertyInfo p )
        {
            _o = o;
            _p = p;
        }

        public PropertyInfo PropertyInfo { get { return _p; } }

        public void Set( T value ) { _p.SetValue( _o, value, null ); }

        public T Get() { return (T)_p.GetValue( _o, null ); }
    }
    
    public class ConfigItemProperty<T> : ConfigItem, IConfigItemProperty<T>
    {
        ValueProperty<T> _value;
        INotifyPropertyChanged _monitor;

        public ConfigItemProperty( ConfigManager configManager, ValueProperty<T> prop, INotifyPropertyChanged monitor )
            : base( configManager )
        {
            _value = prop;
            _monitor = monitor;
            if( _monitor != null ) _monitor.PropertyChanged += new PropertyChangedEventHandler( _monitor_PropertyChanged );
            DisplayName = _value.PropertyInfo.Name;
        }

        public ConfigItemProperty( ConfigManager configManager, object o, PropertyInfo p )
            : this( configManager, new ValueProperty<T>( o, p ), o as INotifyPropertyChanged )
        {
        }

        public ConfigItemProperty( ConfigManager configManager, object o, PropertyInfo p, INotifyPropertyChanged monitor )
            : this( configManager, new ValueProperty<T>( o, p ), monitor )
        {
        }

        void _monitor_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == _value.PropertyInfo.Name ) NotifyOfPropertyChange( "Value" );
        }
        
        public T Value
        {
            set 
            {
                if( OnSetValue( value ) )
                {
                    _value.Set( value );
                    if( _monitor == null ) NotifyOfPropertyChange( "Value" );
                }
            }
            get { return _value.Get(); }
        }

        protected virtual bool OnSetValue( T value )
        {
            return true;
        }

        public void ValueRefresh( object source, EventArgs e )
        {
            NotifyOfPropertyChange( "Value" );
        }

    }

}
