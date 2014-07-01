using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using CK.Windows.Config;

namespace Host.VM
{
    public class SliderConfigItem :  ConfigItemProperty<int>
    {
        public SliderConfigItem( ConfigManager configManager, ValueProperty<int> prop, INotifyPropertyChanged monitor )
            : base( configManager, prop, monitor )
        {
            _func = i => string.Empty;
            _formattedText = string.Empty;
        }

        public SliderConfigItem( ConfigManager configManager, object o, PropertyInfo p )
            : this( configManager, new ValueProperty<int>( o, p ), o as INotifyPropertyChanged )
        {
        }

        public SliderConfigItem( ConfigManager configManager, object o, PropertyInfo p, INotifyPropertyChanged monitor )
            : this( configManager, new ValueProperty<int>( o, p ), monitor )
        {
        }

        /// <summary>
        /// Updates the FormattedText when a new value is set.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>When overridden in a derived class, only call base when return true</remarks>
        protected override bool OnSetValue( int value )
        {
            FormattedText = _func( value );
            return base.OnSetValue( value );
        }

        string _formattedText;
        public string FormattedText
        {
            get { return _formattedText; }
            set
            {
                if( _formattedText != value )
                {
                    _formattedText = value;
                    NotifyOfPropertyChange( "FormattedText" );
                }
            }
        }

        Func<int,string> _func;
        public void SetFormatFunction( Func<int, string> formatFunc )
        {
            if( formatFunc == null ) throw new ArgumentNullException("formatFunc");
            _func = formatFunc;
            FormattedText = _func( Value );
        }

        int _minimum;
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                if( _minimum != value )
                {
                    _minimum = value;
                    NotifyOfPropertyChange( "Minimum" );
                }
            }
        }

        int _maximum;
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if( _maximum != value )
                {
                    _maximum = value;
                    NotifyOfPropertyChange( "Maximum" );
                }
            }
        }

        int _interval;
        public int Interval
        {
            get { return _interval; }
            set
            {
                if( _interval != value )
                {
                    _interval = value;
                    NotifyOfPropertyChange( "Interval" );
                }
            }
        }
    }
}
