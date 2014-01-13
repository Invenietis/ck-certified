#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\AutoClickViewModel.cs) is part of CiviKey. 
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

//using CK.WPF.Controls;
using CK.Plugin.Config;
using CK.Windows.Config;

namespace Host.VM
{
    public class MouseRadarViewModel : ConfigBase
    {
        public MouseRadarViewModel( AppViewModel app )
            : base( "{390AFE83-C5A2-4733-B5BC-5F680ABD0111}", "Configuraton du Radar", app )
        {
        }

        protected override void NotifyOfPropertiesChange()
        {
            NotifyOfPropertyChange( () => RadarSize );
            NotifyOfPropertyChange( () => Opacity );
            NotifyOfPropertyChange( () => RadarColor );
            NotifyOfPropertyChange( () => ArrowColor );
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            NotifyOfPropertiesChange();
        }

        public int RadarSize
        {
            get { return Config != null ? Config.GetOrSet( "RadarSize", 100 ) : 0; }
            set
            {
                if( Config != null ) Config.Set( "RadarSize", value );
            }
        }

        public int Opacity
        {
            get { return Config != null ? Config.GetOrSet( "Opacity", 255 ) : 255; }
            set
            {
                if( Config != null ) Config.Set( "Opacity", value );
            }
        }

        public int RotationSpeed
        {
            get { return Config != null ? Config.GetOrSet( "RotationSpeed", 1 ) : 1; }
            set
            {
                if( Config != null ) Config.Set( "RotationSpeed", value );
            }
        }

        public int TranslationSpeed
        {
            get { return Config != null ? Config.GetOrSet( "TranslationSpeed", 1 ) : 1; }
            set
            {
                if( Config != null ) Config.Set( "TranslationSpeed", value );
            }
        }

        public string RadarColor
        {
            get { return Config != null ? Config.GetOrSet( "RadarColor", "" ) : ""; }
            set
            {
                if( Config != null ) Config.Set( "RadarColor", value );
            }
        }

        public string ArrowColor
        {
            get { return Config != null ? Config.GetOrSet( "ArrowColor", "" ) : ""; }
            set
            {
                if( Config != null ) Config.Set( "ArrowColor", value );
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            var g = this.AddGroup( );

            g.AddProperty( "Taille du radar (rayon)", this, h => RadarSize );

            g.AddProperty( "Opacité du radar", this, h => Opacity );
            g.AddProperty( "Couleur du radar", this, h => RadarColor );
            g.AddProperty( "Couleur du la flèche", this, h => ArrowColor );

            g.AddProperty( "Vitesse de rotation du radar", this, h => RotationSpeed );
            g.AddProperty( "Vitesse de déplacement de la souris", this, h => TranslationSpeed );


        }
    }
}
