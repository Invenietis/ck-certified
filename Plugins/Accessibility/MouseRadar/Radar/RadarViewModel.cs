using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using CK.Context;

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
            ScreenScale = new Point();
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
