using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using CommonServices;
using System.Drawing.Drawing2D;
using HighlightModel;
using System.Diagnostics;
namespace MouseRadar
{
    public enum RadarStep
    {
        Paused = 0,
        Rotating = 1,
        Translating = 2
    }

    public partial class Radar : IDisposable
    {
        IPointerDeviceDriver _mouseDriver;
        //int _translationDirection = 1;
        //int _rotationDirection = 1;
        int _originX;
        int _originY;
        int _rayon;

        ScreenBound _previousCollision = ScreenBound.None;
        DispatcherTimer _timerRotate;
        DispatcherTimer _timerTranslate;
        internal double Radian { get; private set; }

        /// <summary>
        /// Gets or sets the number of ticks required to move the mouse to a total of 300 pixels
        /// </summary>
        internal int TranslationSpeed { get; set; }

        /// <summary>
        /// Gets or sets the number of ticks required to turn full circle
        /// </summary>
        internal int RotationSpeed { get; set; }

        public bool SnakeMode { get; set; }
        internal RadarStep CurrentStep { get; private set; }

        internal RadarViewModel ViewModel { get; private set; }

        internal Radar( IPointerDeviceDriver pdd )
            : this()
        {
            ViewModel = new RadarViewModel();
            ViewModel.SetCircleColor( Color.FromRgb( 0, 0, 0 ) );
            ViewModel.SetArrowColor( Color.FromRgb( 0, 0, 0 ) );
            ViewModel.Opacity = 1;
            ViewModel.RadarSize = 100;
            SnakeMode = false;
            DataContext = ViewModel;
            _mouseDriver = pdd;
            Left = pdd.CurrentPointerXLocation - ViewModel.WindowSize / 2;
            Top = pdd.CurrentPointerYLocation - ViewModel.WindowSize / 2;
            RotationSpeed = 1;
            TranslationSpeed = 1;

            CurrentStep = RadarStep.Paused;

            _timerRotate = new DispatcherTimer( DispatcherPriority.Send );
            _timerRotate.Interval = new TimeSpan( 10000 * 17 );  //60 fps -> 60° per second

            _timerTranslate = new DispatcherTimer( DispatcherPriority.Send );
            _timerTranslate.Interval = new TimeSpan( 10000 * 17 ); //60 fps -> 60px per second  

            _mouseDriver.PointerMove += OnMouseLocationChanged;
            ViewModel.PropertyChanged += ( o, e ) =>
            {
                if( e.PropertyName == "WindowSize" )
                {
                    Width = ViewModel.WindowSize;
                    Height = ViewModel.WindowSize;
                }
            };
        }

        internal void Initialize()
        {
            PresentationSource source = PresentationSource.FromVisual( this );
            ViewModel.ScreenScale = new Point( source.CompositionTarget.TransformToDevice.M11, source.CompositionTarget.TransformToDevice.M22 );
            StartRotation(); //Do we really need to start the rotation if we're goind to Pause right after ? (see MouseRadar.cs)
            UpdateLocation( _mouseDriver.CurrentPointerXLocation, _mouseDriver.CurrentPointerYLocation );
        }

        internal void Tick( BeginScrollingInfo scrollingInfo )
        {
            if( CurrentStep == RadarStep.Rotating )
            {
                ProcessRotation();
            }
            else if( CurrentStep == RadarStep.Translating )
            {
                ProcessTranslation();
            }
        }

        internal void Pause()
        {
            CurrentStep = RadarStep.Paused;
            ViewModel.LapCount = 0;
        }

        internal RadarStep ToNextStep()
        {
            if( CurrentStep == RadarStep.Paused )
            {
                StartRotation();
            }
            else if( CurrentStep == RadarStep.Rotating )
            {
                StopRotation();
                StartTranslation();
            }
            else if(CurrentStep == RadarStep.Translating)
            {
                StopTranslation();
                StartRotation();
            }

            //Each time the input is triggered, we reset the lapcount and the starting angle of the lap count. (thanks to that, we release the scroller in an homogenous way : X laps after the last call to SelectElement)
            ViewModel.LapCount = 0;
            ViewModel.StartingAngle = ViewModel.Angle;

            return CurrentStep;
        }

        #region Rotation

        void StartRotation()
        {
            CurrentStep = RadarStep.Rotating;
        }

        void StopRotation()
        {

        }

        void ProcessRotation()
        {
            ViewModel.Angle += (float)RotationSpeed / 1f;
        }

        #endregion

        #region Translation

        void StartTranslation()
        {
            _originX = _mouseDriver.CurrentPointerXLocation;
            _originY = _mouseDriver.CurrentPointerYLocation;
            Radian = ViewModel.Angle / 180 * Math.PI;
            _rayon = 0;

            CurrentStep = RadarStep.Translating;
        }

        void StopTranslation()
        {

        }

        void ProcessTranslation()
        {
            int moveX = _mouseDriver.CurrentPointerXLocation;
            int moveY = _mouseDriver.CurrentPointerYLocation;
            var p = GetTranslation( moveX, moveY );
            Screen curScreen = Screen.FromPoint( new System.Drawing.Point( (int)p.X, (int)p.Y ) );

            ScreenBound collision = CheckBoundCollision( p );

            //stuck the cursor on screen bounds on collision 
            switch( collision )
            {
                default:
                    moveX = (int)p.X;
                    moveY = (int)p.Y;
                    break;
                case ScreenBound.Left:
                    if( SnakeMode )
                    {
                        //Reset the translation properties then recompute the translation
                        _rayon = 0;
                        _originX = moveX = curScreen.Bounds.Right - 1;
                        _originY = moveY = curScreen.Bounds.Bottom - 1 - moveY;

                        p = GetTranslation( moveX, moveY );

                        moveY = (int)p.Y;
                        moveX = (int)p.X;
                        collision = ScreenBound.None;
                    }
                    else
                    {
                        moveX = 0;
                        moveY = (int)p.Y;
                    }
                    break;
                case ScreenBound.Top:
                    if( SnakeMode )
                    {
                        //Reset the translation properties then recompute the translation
                        _rayon = _rayon - curScreen.Bounds.Bottom - 1;
                        p = GetTranslation( moveX, moveY );

                        moveY = (int)p.Y;
                        moveX = (int)p.X;
                        collision = ScreenBound.None;
                    }
                    else
                    {
                        moveY = 0;
                        moveX = (int)p.X;
                    }

                    break;
                case ScreenBound.Right:
                    if( SnakeMode )
                    {
                        //Reset the translation properties then recompute the translation
                        _rayon = _rayon - curScreen.Bounds.Right - 1;

                        p = GetTranslation( moveX, moveY );

                        moveY = (int)p.Y;
                        moveX = (int)p.X;
                        collision = ScreenBound.None;
                    }
                    else
                    {
                        moveX = curScreen.Bounds.Right;
                        moveY = (int)p.Y;
                    }
                    break;
                case ScreenBound.Bottom:
                    if( SnakeMode )
                    {
                        //Reset the translation properties then recompute the translation
                        _rayon = _rayon - curScreen.Bounds.Bottom - 1;
                        p = GetTranslation( moveX, moveY );

                        moveY = (int)p.Y;
                        moveX = (int)p.X;
                        collision = ScreenBound.None;
                    }
                    else
                    {
                        moveY = curScreen.Bounds.Bottom;
                        moveX = (int)p.X;
                    }

                    break;
            }

            if( collision != ScreenBound.None ) ScreenBoundCollide( collision );
            _mouseDriver.MovePointer( moveX, moveY );
        }

        Point GetTranslation( int x, int y )
        {
            double rad = ViewModel.Angle / 180 * Math.PI;
            _rayon += TranslationSpeed;
            return new Point( _originX + _rayon * Math.Cos( rad ), _originY + _rayon * Math.Sin( rad ) );
        }

        #endregion

        #region Collisions

        void OnMouseLocationChanged( object sender, PointerDeviceEventArgs e )
        {
            UpdateLocation( e.X, e.Y );
        }

        public void UpdateLocation( int x, int y )
        {
            Left = ( x / ViewModel.ScreenScale.X ) - ( ViewModel.WindowSize / 2 );
            Top = ( y / ViewModel.ScreenScale.Y ) - ( ViewModel.WindowSize / 2 );

            if( _previousCollision != ScreenBound.None && CheckBoundCollision( new Point( x, y ) ) == ScreenBound.None )
                ScreenBoundCollide( ScreenBound.None );
        }

        ScreenBound CheckBoundCollision( Point p, int precision = 0 )
        {
            Screen current = Screen.FromPoint( new System.Drawing.Point( (int)p.X, (int)p.Y ) );

            ScreenBound collision = ScreenBound.None;
            if( p.X - precision <= current.Bounds.Left ) collision = ScreenBound.Left;
            if( p.X + precision >= current.Bounds.Right - 1 ) //X can't be equal to current.Bounds.Right (current.Bounds.Right - 1 max)
                collision = ScreenBound.Right;
            if( p.Y - precision <= current.Bounds.Top ) collision = ScreenBound.Top;
            if( p.Y + precision >= current.Bounds.Bottom - 1 )
                collision = ScreenBound.Bottom; //Y can't be equal to current.Bounds.Bottom (current.Bounds.Bottom - 1 max)
            return collision;
        }

        void ScreenBoundCollide( ScreenBound bound )
        {
            _previousCollision = bound;

            switch( bound )
            {
                case ScreenBound.Left:
                    ViewModel.AngleMin = 270;
                    ViewModel.AngleMax = 90;
                    break;
                case ScreenBound.Top:
                    ViewModel.AngleMin = 0;
                    ViewModel.AngleMax = 180;
                    break;
                case ScreenBound.Right:
                    ViewModel.AngleMin = 90;
                    ViewModel.AngleMax = 270;
                    break;
                case ScreenBound.Bottom:
                    ViewModel.AngleMin = 180;
                    ViewModel.AngleMax = 360;
                    break;
                default:
                    ViewModel.AngleMin = 0;
                    ViewModel.AngleMax = 360;
                    break;
            }

            if( bound != ScreenBound.None )
            {
                StopTranslation();
                StartRotation();
            }
        }

        #endregion

        public void Dispose()
        {
            _timerRotate.Stop();
            _timerTranslate.Stop();
            
            _mouseDriver.PointerMove -= OnMouseLocationChanged;
            this.Close();
        }

    }

    public enum ScreenBound
    {
        None,
        Left,
        Top,
        Right,
        Bottom
    }

}
