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

    public class ConfigItem : PropertyChangedBase
    {
        ConfigManager _configManager;
        string _displayName;
        string _description;
        string _imagePath;
        bool _visible;
        bool _enabled;

        public ConfigItem( ConfigManager configManager )
        {
            _configManager = configManager;
            _visible = _enabled = true;
        }

        public ConfigManager ConfigManager { get { return _configManager; } }

        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                if( _displayName != value )
                {
                    _displayName = value;
                    NotifyOfPropertyChange( "DisplayName" );
                }
            }
        }
        
        public string Description
        {
            get { return _description; }
            set
            {
                if( _description != value )
                {
                    _description = value;
                    NotifyOfPropertyChange( "Description" );
                }
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if( _enabled != value )
                {
                    _enabled = value;
                    OnEnabledChange();
                }
            }
        }

        public bool Visible
        {
            get { return _visible; }
            set
            {
                if( _visible != value )
                {
                    _visible = value;
                    OnVisibleChange();
                }
            }
        }

        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                if( _imagePath != value )
                {
                    _imagePath = value;
                    NotifyOfPropertyChange( "ImagePath" );
                }
            }
        }

        protected virtual void OnEnabledChange()
        {
            NotifyOfPropertyChange( "Enabled" );
        }
        
        protected virtual void OnVisibleChange()
        {
            NotifyOfPropertyChange( "Visible" );
        }

    }


}
