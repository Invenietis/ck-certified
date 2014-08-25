#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\ScreenScroller\Editor\EditorViewModel.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using CK.Plugin;
using CK.Core;
using System.ComponentModel;
using CK.Plugin.Config;
using CK.Context;
using ScreenScroller.Resources;

namespace ScreenScroller.Editor
{
    public class EditorViewModel : INotifyPropertyChanged
    {
        IPluginConfigAccessor _config;
        public EditorViewModel( IPluginConfigAccessor conf, IContext ctx )
        {
            _config = conf;
            _context = ctx;
        }

        public string Title
        {
            get { return R.ScreenScrollerConfiguration; }
        }


        public int SquareSize
        {
            get { return _config.User.GetOrSet( "SquareSize", 2 ); }
            set
            {
                _config.User.Set( "SquareSize", value );
            }
        }

        public int ClickDepth
        {
            get { return _config.User.GetOrSet( "ClickDepth", 5 ); }
            set
            {
                _config.User.Set( "ClickDepth", value );
            }
        }

        public int MaxLapCount
        {
            get { return _config.User.GetOrSet( "MaxLapCount", 2 ); }
            set
            {
                _config.User.Set( "MaxLapCount", value );
            }
        }

        public string BackgroundColor
        {
            get { return _config.User.GetOrSet( "BackgroundColor", "Black" ); }
            set
            {
                _config.User.Set( "BackgroundColor", value );
            }
        }

        public string FormatedSquareSize
        {
            get
            {
                if( SquareSize < 3 ) return R.LowSpeed;
                if( SquareSize < 7 ) return R.MediumSpeed;
                else return R.HighSpeed;
            }
        }

        public string formatedClickDepth
        {
            get
            {
                if( ClickDepth < 3 ) return R.LowSpeed;
                if( ClickDepth > 3 ) return R.HighSpeed;
                else return R.MediumSpeed;
            }
        }

        IContext _context;
        bool _isClosing = false;

        public void Close()
        {
            if( !_isClosing )
            {
                _context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( ScreenScrollerEditor.PluginIdString ), ConfigUserAction.Stopped );
                _context.GetService<ISimplePluginRunner>( true ).Apply();
            }
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
