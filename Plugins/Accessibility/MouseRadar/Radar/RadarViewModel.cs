#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\MouseRadar\Radar\RadarViewModel.cs) is part of CiviKey. 
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
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MouseRadar
{
    public class RadarViewModel : INotifyPropertyChanged
    {
        public const int ARROW_LENGTH = 15;

        float _angle;
        SolidColorBrush _circleColor;
        SolidColorBrush _arrowColor;
        int _radarSize;
        float _opacity;
        public float StartingAngle { get; set; }
        public float AngleMin { get; set; }
        public float AngleMax { get; set; }
        public Point ScreenScale { get; set; }
        public int LapCount { get; internal set; }

        public RadarViewModel()
        {
            _opacity = 1;
            AngleMin = 0;
            AngleMax = 360;
            ScreenScale = new Point( 1, 1 );
        }

        public float Opacity
        {
            get { return _opacity; }
            set
            {
                _opacity = value;
                FirePropertyChanged( "Opacity" );
            }
        }
        public int RadarSize
        {
            get { return _radarSize; }
            set
            {
                _radarSize = value;
                FirePropertyChanged( "RadarSize" );
                FirePropertyChanged( "WindowSize" );
                FirePropertyChanged( "PointerPositionX" );
                FirePropertyChanged( "PointerPositionY" );
                FirePropertyChanged( "RotationOriginX" );
                FirePropertyChanged( "RotationOriginY" );
            }
        }

        public int WindowSize
        {
            get { return RadarSize + 50; }
        }

        public int PointerPositionY
        {
            get { return RadarSize / 2 - ARROW_LENGTH / 2; }
        }

        public int PointerPositionX
        {
            get { return RadarSize - ARROW_LENGTH / 2; }
        }

        public int RotationOriginX
        {
            get { return -RadarSize / 2 + ARROW_LENGTH / 2; }
        }

        public int RotationOriginY
        {
            get { return ARROW_LENGTH / 2; }
        }

        public SolidColorBrush CircleColor
        {
            get { return _circleColor; }
            set
            {
                _circleColor = value;
                FirePropertyChanged( "CircleColor" );
            }
        }

        public SolidColorBrush ArrowColor
        {
            get { return _arrowColor; }
            set
            {
                _arrowColor = value;
                FirePropertyChanged( "ArrowColor" );
            }
        }

        public float Angle
        {
            get { return _angle; }
            set
            {
                if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

                //brain workout
                if( AngleMin > AngleMax )
                {
                    //Jump to AngleMin if Angle beetween max and min
                    if( value >= AngleMax && value <= AngleMin )
                    {
                        _angle = value - AngleMax + AngleMin;
                        LapCount++;
                    }
                    else if( value >= 360 ) _angle = value - 360; //When angle > 360 restart to 0
                    else _angle = value;
                }
                else
                {
                    //Back to AngleMin if Angle > max 
                    if( value >= AngleMax )
                    {
                        _angle = value - AngleMax + AngleMin;
                        LapCount++;
                    }
                    else if( value < AngleMin )
                    {
                        _angle = value + AngleMax;//Jump to AngleMax when Angle < AngleMin
                    }
                    else _angle = value;
                }

                FirePropertyChanged( "Angle" );
            }
        }

        public void SetCircleColor( Color c )
        {
            CircleColor = new SolidColorBrush( c );
        }

        public void SetArrowColor( Color c )
        {
            ArrowColor = new SolidColorBrush( c );
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void FirePropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}
