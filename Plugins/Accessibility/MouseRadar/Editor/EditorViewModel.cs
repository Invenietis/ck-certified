#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\MouseRadar\Editor\EditorViewModel.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using Caliburn.Micro;
using CK.Context;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Core;
using System.Windows.Media;
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
                _config.User["RadarSize"] = ( value > 200 ) ? 200 : value;
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
                if( TranslationSpeed < 3 ) return R.LowSpeed;
                if( TranslationSpeed < 7 ) return R.MediumSpeed;
                else return R.HighSpeed;
            }
        }

        public string FormatedRotationSpeed
        {
            get
            {
                if( RotationSpeed < 3 ) return R.LowSpeed;
                if( RotationSpeed > 3 ) return R.HighSpeed;
                else return R.MediumSpeed;
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
