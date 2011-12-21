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
    public class ConfigItemCollection : BindableCollection<object>
    {
        ConfigItem _holder;

        public ConfigItemCollection( ConfigItem holder )
        {
            _holder = holder;
            _holder.PropertyChanged += OnHolderPropertyChanged;
        }

        void OnHolderPropertyChanged( object o, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "Enabled" )
            {
                foreach( var c in Items.OfType<ConfigItem>() ) c.Enabled = _holder.Enabled;
            }
        }

        protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
        {
            IEnumerable source = null;
            if( e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace ) source = e.NewItems;
            else if( e.Action == NotifyCollectionChangedAction.Reset ) source = Items;
            if( source != null ) foreach( var c in source.OfType<ConfigItem>() ) c.Enabled = _holder.Enabled;
            base.OnCollectionChanged( e );
        }
    }
}
