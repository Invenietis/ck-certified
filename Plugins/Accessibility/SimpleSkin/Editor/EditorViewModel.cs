using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using System.Collections.ObjectModel;
using CK.WPF.ViewModel;
using CK.Core;
using CK.Plugin;
using CK.Context;

namespace SimpleSkinEditor
{
    public class LayoutZoneViewModel
    {
        public ILayoutZone Model { get; private set; }

        public LayoutZoneViewModel( ILayoutZone model )
        {
            Model = model;
        }

        public string Name { get { return Model.Zone.Name; } }
    }

    public partial class EditorViewModel : Screen
    {
        IKeyboardContext _ctx;
        IPluginConfigAccessor _config;
        object _selectedHolder;

        LayoutZoneViewModel _dfZone;
        Dictionary<ILayoutZone, LayoutZoneViewModel> _zoneCache;

        public IContext Context { get; set; }

        public bool Stopping { get; set; }

        public VMCollection<LayoutZoneViewModel, ILayoutZone> Zones { get; private set; }

        // Used by the binding
        public object SelectedHolder
        {
            get { return _selectedHolder; }
            set
            {
                _selectedHolder = value;
                Refresh();
            }
        }

        // Used to find the config
        internal IKeyboardElement ConfigHolder { get { return _selectedHolder is LayoutZoneViewModel ? ((LayoutZoneViewModel)_selectedHolder).Model : (IKeyboardElement)_selectedHolder; } }

        public bool CanSelectZones { get { return SelectedHolder != _ctx.CurrentKeyboard.CurrentLayout; } }

        public EditorViewModel( IKeyboardContext ctx, IPluginConfigAccessor config )
        {
            DisplayName = "";

            _ctx = ctx;
            _config = config;
            _zoneCache = new Dictionary<ILayoutZone, LayoutZoneViewModel>();

            SelectedHolder = _ctx.CurrentKeyboard.CurrentLayout;
            Zones = new VMCollection<LayoutZoneViewModel, ILayoutZone>( _ctx.CurrentKeyboard.CurrentLayout.LayoutZones, FindOrCreate );

            _ctx.CurrentKeyboardChanged += new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
            _config.ConfigChanged += new EventHandler<ConfigChangedEventArgs>( OnLayoutConfigChanged );
        }

        void OnLayoutConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( ( c ) => String.Compare(c.UniqueId.ToString(), "36C4764A-111C-45E4-83D6-E38FC1DF5979", true ) == 0 ) )
            {
                switch( e.Key )
                {
                    case "Background":
                    case "HoverBackground":
                    case "PressedBackground":
                    case "FontSize":
                    case "FontWeight":
                    case "FontSizes":
                    case "FontStyle":
                    case "TextDecorations":                      
                    case "FontColor":
                        NotifyOfPropertyChange( () => Background );
                        NotifyOfPropertyChange( () => HoverBackground );
                        NotifyOfPropertyChange( () => PressedBackground );
                        NotifyOfPropertyChange( () => FontSize );
                        NotifyOfPropertyChange( () => FontWeight );
                        NotifyOfPropertyChange( () => FontSizes );
                        NotifyOfPropertyChange( () => FontStyle );    
                        NotifyOfPropertyChange( () => TextDecorations );
                        NotifyOfPropertyChange( () => FontColor );
                        break;
                    default:
                        break;
                }
            }
        }

        LayoutZoneViewModel FindOrCreate( ILayoutZone model )
        {
            LayoutZoneViewModel vm;
            if( !_zoneCache.TryGetValue( model, out vm ) )
            {
                vm = new LayoutZoneViewModel( model );
                if( model.Zone.Name == string.Empty ) _dfZone = vm;
                _zoneCache.Add( model, vm );
            }
            return vm;
        }

        void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            if( e.Previous != null ) e.Previous.CurrentLayoutChanged -= new EventHandler<KeyboardCurrentLayoutChangedEventArgs>( OnCurrentLayoutChanged );
            if( e.Current != null )
            {
                e.Current.CurrentLayoutChanged += new EventHandler<KeyboardCurrentLayoutChangedEventArgs>( OnCurrentLayoutChanged );

                _zoneCache.Clear();
                Zones.Refresh();
                Zones = new VMCollection<LayoutZoneViewModel, ILayoutZone>( _ctx.CurrentKeyboard.CurrentLayout.LayoutZones, FindOrCreate );
                ToggleCurrentKeyboardAsHolder();
                NotifyOfPropertyChange( () => Zones );
                Refresh();
            }
        }

        void OnCurrentLayoutChanged( object sender, KeyboardCurrentLayoutChangedEventArgs e )
        {
            _zoneCache.Clear();
            Zones.Refresh();
            Refresh();
        }

        public void ToggleCurrentKeyboardAsHolder()
        {
            if( SelectedHolder == _ctx.CurrentKeyboard.CurrentLayout )
                SelectedHolder = _dfZone;
            else
                SelectedHolder = _ctx.CurrentKeyboard.CurrentLayout;
        }

        public VMCommand<string> ClearPropertyCmd { get { return _clearCmd == null ? _clearCmd = new VMCommand<string>( ClearProperty, CanClearProperty ) : _clearCmd; } }

        void ClearProperty( string propertyName )
        {
            string[] names = propertyName.Split( ',' );
            foreach( var pname in names ) _config[ConfigHolder].Remove( pname );
            Refresh();
        }

        bool CanClearProperty( string propertyName )
        {
            string[] names = propertyName.Split( ',' );
            // We can clear property if the property owns directly a value.
            foreach( var pname in names ) if( _config[ConfigHolder][pname] != null ) return true;
            return false;
        }

        protected override void OnDeactivate( bool close )
        {
            if( close && !Stopping )
            {
                Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( SimpleSkinEditor.PluginIdString ), ConfigUserAction.Stopped );
                Context.GetService<ISimplePluginRunner>( true ).Apply();
            }
            base.OnDeactivate( close );
        }
    }
}
