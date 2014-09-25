using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using CK.Windows.Config;
using CommonServices;

namespace Host.VM
{
    public class RecordConfigItem : ConfigItemProperty<ITrigger>
    {
        AppViewModel _app;
        Guid _triggerGuid = Guid.Parse( "{14FE0383-2BE4-43A1-9627-A66C2CA775A6}" ); //InputTrigger
        ITriggerService _triggerService;
        ITriggerService TriggerService
        {
            get
            {
                if( _triggerService != null ) return _triggerService;

                _triggerService = (ITriggerService)_app.PluginRunner.ServiceHost.GetProxy( typeof(ITriggerService) );
                return _triggerService;
            }
        }

        public RecordConfigItem( AppViewModel app, ValueProperty<ITrigger> prop, INotifyPropertyChanged monitor )
            : base( app.ConfigManager, prop, monitor )
        {
            _app = app;
        }

        public RecordConfigItem( AppViewModel app, object o, PropertyInfo p )
            : this( app, new ValueProperty<ITrigger>( o, p ), o as INotifyPropertyChanged )
        {
        }

        public RecordConfigItem( AppViewModel app, object o, PropertyInfo p, INotifyPropertyChanged monitor )
            : this( app, new ValueProperty<ITrigger>( o, p ), monitor )
        {
        }

        private bool _isRecording;
        public bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                _isRecording = value;

                if( value )
                {
                    TriggerService.InputListener.Record( ( ITrigger trigger ) =>
                    {
                        Value = trigger;
                        IsRecording = false;
                        NotifyOfPropertyChange( () => IsRecording );
                        NotifyOfPropertyChange( () => SelectedKey );
                    } );
                }
            }
        }

        public string SelectedKey
        {
            get
            {
                if( TriggerService != null )
                {
                    var selectedKey = Value ?? TriggerService.DefaultTrigger;

                    if( selectedKey != null )
                    {
                        if( selectedKey.Source == TriggerDevice.Keyboard )
                        {
                            Keys keyName = (Keys)selectedKey.KeyCode;

                            return string.Format( Scroller.Resources.R.Listening, keyName.ToString() );
                        }
                        else if( selectedKey.Source == TriggerDevice.Pointer )
                            return string.Format( Scroller.Resources.R.PointerListening, MouseClicFromCode( selectedKey.KeyCode ) );
                    }
                }
                return Scroller.Resources.R.NothingSelected;
            }
        }

        string _toggleContent;
        public string ToggleContent
        {
            get { return _toggleContent ?? Scroller.Resources.R.StartRecording; }
            set
            {
                if( _toggleContent != value )
                {
                    _toggleContent = value;
                    NotifyOfPropertyChange( "ToggleContent" );
                }
            }
        }

        string _description;
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

        string MouseClicFromCode( int code )
        {
            switch( code )
            {
                case 1:
                    return Scroller.Resources.R.LeftClick;
                case 2:
                    return Scroller.Resources.R.RightClick;
                case 3:
                    return Scroller.Resources.R.MiddleClick;
            }

            return String.Empty;
        }
    }
}
