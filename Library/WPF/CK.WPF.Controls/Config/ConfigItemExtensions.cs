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
    public static class ConfigItemExtensions
    {
        public static ConfigGroup AddGroup( this IConfigItemContainer c )
        {
            ConfigGroup g = new ConfigGroup( c.ConfigManager );
            c.Items.Add( g );
            return g;
        }

        public static ConfigPage AddLink( this IConfigItemContainer c, ConfigPage page )
        {
            c.Items.Add( new ConfigItemLink( c.ConfigManager, page, null ) );
            return page;
        }

        public static ConfigItemProperty<T> AddProperty<T, THolder>( this IConfigItemContainer c, THolder o, Expression<Func<THolder, T>> prop )
        {
            ConfigItemProperty<T> p = new ConfigItemProperty<T>( c.ConfigManager, o, Helper.GetPropertyInfo( prop ) );
            c.Items.Add( p );
            return p;
        }

        public static ConfigItemProperty<T> AddProperty<T, THolder>( this IConfigItemContainer c, string displayName, THolder o, Expression<Func<THolder, T>> prop )
        {
            return AddProperty<T, THolder>( c, displayName, null, o, prop );
        }

        public static ConfigItemProperty<T> AddProperty<T, THolder>( this IConfigItemContainer c, string displayName, string description, THolder o, Expression<Func<THolder, T>> prop )
        {
            ConfigItemProperty<T> p = new ConfigItemProperty<T>( c.ConfigManager, o, Helper.GetPropertyInfo( o, prop ) );
            p.DisplayName = displayName;
            p.Description = description;
            c.Items.Add( p );
            return p;
        }

        public static ConfigActivableSection AddActivableSection<THolder>( this IConfigItemContainer c, string displayName, string description, THolder o, Expression<Func<THolder, bool>> prop, INotifyPropertyChanged propertyMonitor )
        {
            ConfigActivableSection s = new ConfigActivableSection( c.ConfigManager, o, Helper.GetPropertyInfo( prop ), propertyMonitor );
            s.DisplayName = displayName;
            s.Description = description;
            c.Items.Add( s );
            return s;
        }

        public static ConfigItemAction AddAction( this IConfigItemContainer c, string displayName, System.Action action )
        {
            return AddAction( c, displayName, null, action );
        }

        public static ConfigItemAction AddAction( this IConfigItemContainer c, string displayName, string description, System.Action action )
        {
            ConfigItemAction a = new ConfigItemAction( c.ConfigManager, new SimpleCommand( action ) ) { DisplayName = displayName, Description = description };
            c.Items.Add( a );
            return a;
        }

        public static ConfigItemCurrent<T> AddCurrentItem<T, THolder>( this IConfigItemContainer c, string displayName, string description, THolder o, Expression<Func<THolder, T>> prop, Func<THolder,object> valueCollection )
        {
            ConfigItemCurrent<T> a = new ConfigItemCurrent<T>( c.ConfigManager, o, Helper.GetPropertyInfo( o, prop ), () => valueCollection( o ) ) { DisplayName = displayName, Description = description };
            c.Items.Add( a );
            return a;
        }

    }

}
