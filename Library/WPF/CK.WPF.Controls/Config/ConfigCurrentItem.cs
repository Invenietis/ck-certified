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
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows;

namespace CK.WPF.Controls
{
    public class ConfigItemCurrent<T> : ConfigItem, IConfigItemCurrent<T>
    {
        ValueProperty<T> _current;
        INotifyPropertyChanged _monitorCurrent;
        Func<object> _sourceValues;
        ICollectionView _values;

        public ConfigItemCurrent( ConfigManager configManager, ValueProperty<T> current, Func<object> valueCollection, INotifyPropertyChanged monitorCurrent )
            : base( configManager )
        {
            _current = current;
            _monitorCurrent = monitorCurrent;
            _sourceValues = valueCollection;
        }

        public ConfigItemCurrent( ConfigManager configManager, object o, PropertyInfo current, Func<object> valueCollection )
            : this( configManager, new ValueProperty<T>( o, current ), valueCollection, o as INotifyPropertyChanged )
        {
        }

        public Visibility ShowMultiple { get { return IsMoreThanOne ? Visibility.Visible : Visibility.Collapsed; } }

        public Visibility ShowOne { get { return IsMoreThanOne ? Visibility.Collapsed : Visibility.Visible; } }

        public bool IsMoreThanOne
        {
            get { return Values.SourceCollection.OfType<object>().ElementAtOrDefault( 1 ) != null; } 
        }

        public ICollectionView Values
        {
            get
            {
                if( _values == null )
                {
                    _values = CollectionViewSource.GetDefaultView( _sourceValues() );
                    _values.MoveCurrentTo( _current.Get() );
                    _values.CurrentChanged += _values_CurrentChanged;
                }
                return _values; 
            }
        }

        public void ValuesRefresh( object o, EventArgs e )
        {
            Values.Refresh();
        }

        void _values_CurrentChanged( object sender, EventArgs e )
        {
            _current.Set( (T)_values.CurrentItem );
        }



    }

}
