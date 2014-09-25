using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;

namespace SharedData
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion )]
    public class SharedDataPlugin : IPlugin, ISharedData
    {
        #region Plugin description

        const string PluginGuidString = "{BCD4DE84-E6C9-47C3-B29D-3EAA0D50B14C}";
        const string PluginVersion = "1.0.0";
        const string PluginPublicName = "Shared Data Plugin";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        /// <summary>
        /// Never use this property to set a config property, because this shotcut the changing/changed mecanism.
        /// </summary>
        public IPluginConfigAccessor Config { get; set; }
        
        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            Config.ConfigChanged += Config_ConfigChanged;
        }

        void Config_ConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            OnSharedPropertyChanged( e.Key );
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
        }

        #endregion

        #region ISharedData Members

        public double WindowOpacity
        {
            get
            {
                return Config.User.GetOrSet<double>( "WindowOpacity", 1 );
            }
            set
            {
                if( value != WindowOpacity )
                {
                    if( OnSharedPropertyChanging() )
                    {
                        Config.User.Set( "WindowOpacity", value );
                        OnSharedPropertyChanged();
                    }
                }
            }
        }

        public int WindowBorderThickness
        {
            get
            {
                return Config.User.GetOrSet<int>( "WindowBorderThickness", 5 );
            }
            set
            {
                if( value != WindowBorderThickness )
                {
                    if( OnSharedPropertyChanging() )
                    {
                        Config.User.Set( "WindowBorderThickness", value );
                        OnSharedPropertyChanged();
                    }
                }
            }
        }

        public Color WindowBorderBrush
        {
            get
            {
                return Config.User.GetOrSet<Color>( "WindowBorderBrush", (Color)ColorConverter.ConvertFromString( "#4F4F4F" ) );
            }
            set
            {
                if( value != WindowBorderBrush )
                {
                    if( OnSharedPropertyChanging() )
                    {
                        Config.User.Set( "WindowBorderBrush", value );
                        OnSharedPropertyChanged();
                    }
                }
            }
        }

        public Color WindowBackgroundColor
        {
            get
            {
                return Config.User.GetOrSet<Color>( "WindowBackgroundColor", (Color)ColorConverter.ConvertFromString( "#4F4F4F" ) );
            }
            set
            {
                if( value != WindowBackgroundColor )
                {
                    if( OnSharedPropertyChanging() )
                    {
                        Config.User.Set( "WindowBackgroundColor", value );
                        OnSharedPropertyChanged();
                    }
                }
            }
        }

        public event EventHandler<SharedPropertyChangedEventArgs> SharedPropertyChanged;

        public event EventHandler<SharedPropertyChangingEventArgs> SharedPropertyChanging;

        #endregion

        static bool _throwException;

        /// <summary>
        /// Sets whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the <see cref="CheckPropertyName"/> method.
        /// The default value is false, but it might be set to true in unit test contexts.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void SetThrowOnInvalidPropertyName( bool throwException )
        {
            _throwException = throwException;
        }

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional( "DEBUG" )]
        [DebuggerStepThrough]
        public void CheckPropertyName( string propertyName )
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if( TypeDescriptor.GetProperties( this )[propertyName] == null )
            {
                string msg = "Invalid property name: " + propertyName;
                if( _throwException ) throw new Exception( msg );
                Debug.Fail( msg );
            }
        }

        private void OnSharedPropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            var handler = this.SharedPropertyChanged;
            if( handler != null )
            {
                var e = new SharedPropertyChangedEventArgs( propertyName );
                handler( this, e );
            }
        }

        private bool OnSharedPropertyChanging( [CallerMemberName] string propertyName = "" )
        {
            var handler = this.SharedPropertyChanging;
            if( handler != null )
            {
                var e = new SharedPropertyChangingEventArgs( propertyName );
                handler( this, e );
                return e.Cancel;
            }
            return false;
        }
    }
}
