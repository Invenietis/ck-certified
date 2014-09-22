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

        protected override void OnValueChanged()
        {
            NotifyOfPropertyChange( "FormattedText" );
        }

        string _formattedText;
        public string FormattedText
        {
            get { return _func( Value ); }
        }

        Func<int,string> _func;
        public void SetFormatFunction( Func<int, string> formatFunc )
        {
            if( formatFunc == null ) throw new ArgumentNullException("formatFunc");
            _func = formatFunc;
            NotifyOfPropertyChange( "FormattedText" );
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

        bool _isDirectionReversed;
        public bool IsDirectionReversed
        {
            get { return _isDirectionReversed; }
            set
            {
                if( _isDirectionReversed != value )
                {
                    _isDirectionReversed = value;
                    NotifyOfPropertyChange( "IsDirectionReversed" );
                }
            }
        }
    }
}
