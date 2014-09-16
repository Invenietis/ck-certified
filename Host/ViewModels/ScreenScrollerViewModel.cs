#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\ScreenScrollerViewModel.cs) is part of CiviKey. 
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

using CK.Plugin.Config;
using CK.Windows.Config;

namespace Host.VM
{
    public class ScreenScrollerViewModel : ConfigBase
    {
        public ScreenScrollerViewModel( AppViewModel app )
            : base( "{AE25D80B-B927-487E-9274-48362AF95FC0}", "Configuraton du défilement d'écran", app )
        {
        }

        protected override void NotifyOfPropertiesChange()
        {
            NotifyOfPropertyChange( () => ClickDepth );
            NotifyOfPropertyChange( () => BackgroundColor );
            NotifyOfPropertyChange( () => SquareSize );
            NotifyOfPropertyChange( () => MaxLapCount );
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            NotifyOfPropertiesChange();
        }

        public int SquareSize
        {
            get { return Config != null ? Config.GetOrSet( "SquareSize", 2 ) : 2; }
            set
            {
                if( Config != null ) Config.Set( "SquareSize", value );
            }
        }

        public int ClickDepth
        {
            get { return Config != null ? Config.GetOrSet( "ClickDepth", 5 ) : 5; }
            set
            {
                if( Config != null ) Config.Set( "ClickDepth", value );
            }
        }

        public int MaxLapCount
        {
            get { return Config != null ? Config.GetOrSet( "MaxLapCount", 2 ) : 2; }
            set
            {
                if( Config != null ) Config.Set( "MaxLapCount", value );
            }
        }

        public string BackgroundColor
        {
            get { return Config != null ? Config.GetOrSet( "BackgroundColor", "Black" ) : "Black"; }
            set
            {
                if( Config != null ) Config.Set( "BackgroundColor", value );
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            var g = this.AddGroup();

            g.AddProperty( "Profondeur", this, h => ClickDepth );
            g.AddProperty( "Taille des grilles", this, h => SquareSize );
            g.AddProperty( "Nombre de tours", this, h => MaxLapCount );
            g.AddProperty( "Couleur du fond", this, h => BackgroundColor );
        }
    }
}
