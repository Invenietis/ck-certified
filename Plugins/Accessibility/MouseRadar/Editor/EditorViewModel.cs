using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CK.Context;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Core;
using System.Windows.Media;
using CommonServices;
using KeyScroller;
using MouseRadar.Resources;

namespace MouseRadar.Editor
{
    public class EditorViewModel : Screen
    {
        IPluginConfigAccessor _config;
        public EditorViewModel( IPluginConfigAccessor conf )
        {
            _config = conf;
            base.DisplayName = R.RadarConfiguration;
        }

        public IContext Context { get; set; }
        public bool Stopping { get; set; }
        
        public string Title
        {
            get { return R.Radar; }
        }

        public int RadarSize
        {
            get { return _config.User.GetOrSet( "RadarSize", 50 ); }
            set
            {
                _config.User["RadarSize"] = value;
                NotifyOfPropertyChange( "RadarSize" );
            }
        }

        protected override void OnDeactivate( bool close )
        {
            if( close && !Stopping )
            {
                Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( MouseRadarEditor.PluginIdString ), ConfigUserAction.Stopped );
                Context.GetService<ISimplePluginRunner>( true ).Apply();
            }
            
            base.OnDeactivate( close );
        }

        public int Opacity
        {
            get { return  _config.User.GetOrSet ( "Opacity", 100 ); }
            set
            {
                _config.User["Opacity"] = value ;
                NotifyOfPropertyChange( "Opacity" );
                NotifyOfPropertyChange( "FormatedOpacity" );
            }
        }
        public void Close() 
        {

        }
        public string FormatedOpacity
        {
            get { return Opacity + "%"; }
        }
        public int RotationSpeed
        {
            get { return  _config.User.GetOrSet( "RotationSpeed", 1 ); }
            set
            {
                _config.User["RotationSpeed"] = value;
                NotifyOfPropertyChange( "RotationSpeed" );
                NotifyOfPropertyChange( "FormatedRotationSpeed" );
            }
        }

        public int TranslationSpeed
        {
            get { return _config.User.GetOrSet( "TranslationSpeed", 1 ); }
            set
            {
                _config.User["TranslationSpeed"] = value;
                NotifyOfPropertyChange( "TranslationSpeed" );
                NotifyOfPropertyChange( "FormatedTranslationSpeed" );
            }
        }

        public string FormatedTranslationSpeed
        {
            get 
            {
                if( TranslationSpeed < 5 ) return R.LowSpeed;
                if( TranslationSpeed < 10 ) return R.MediumSpeed;
                else return R.HighSpeed;
            }
        }

        public string FormatedRotationSpeed
        {
            get
            {
                if( RotationSpeed < 5 ) return R.LowSpeed;
                if( RotationSpeed < 10 ) return R.MediumSpeed;
                else return R.HighSpeed;
            }
        }

        public Color CircleColor
        {
            get { return _config.User.GetOrSet( "CircleColor", Color.FromRgb( 0, 0, 0 ) ); }
            set
            {
                _config.User["CircleColor"] = value;
                NotifyOfPropertyChange( "CircleColor" );
            }
        }

        public Color ArrowColor
        {
            get { return _config.User.GetOrSet( "ArrowColor", Color.FromRgb( 0, 0, 0 ) ); }
            set
            {
                _config.User["ArrowColor"] = value;
                NotifyOfPropertyChange( "ArrowColor" );
            }
        }
    }
}
