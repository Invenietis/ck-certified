#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\MouseRadar\Radar\Radar.cs) is part of CiviKey. 
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
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using CommonServices;
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
        public static readonly int DEFAULT_ROTATION_DELAY = 5000; //Default max rotation time
        public int RotationDelay { get; private set; } //the setted max rotation time
        int _rotationMaxIntervals; //Max _timerRotate intervals. Used when RotationDelay > DEFAULT_ROTATION_DELAY
        int _rotationIntervalCount; //The _timerRotate intervals counter

        IPointerDeviceDriver _mouseDriver;
        //int _translationDirection = 1;
        //int _rotationDirection = 1;
        int _originX;
        int _originY;
        int _rayon;
        int _rotationSpeed;

        Stopwatch _watch;

        ScreenBound _previousCollision = ScreenBound.None;
        internal DispatcherTimer _timerRotate;
        internal DispatcherTimer _timerTranslate;
        internal double Radian { get; private set; }

        /// <summary>
        /// Fired when the rotation delay is reached
        /// </summary>
        public event EventHandler RotationDelayExpired;
        
        /// <summary>
        /// Gets or sets the number of ticks required to move the mouse to a total of 300 pixels
        /// </summary>
        internal int TranslationSpeed { get; set; }

        /// <summary>
        /// Gets or sets the number of ticks required to turn full circle
        /// </summary>
        internal int RotationSpeed
        {
            get { return _rotationSpeed; }
            set
            {
                _rotationSpeed = value;
                UpdateRotationDelay();
            }
        }

        public bool SnakeMode { get; set; }
        internal RadarStep CurrentStep { get; private set; }

        internal RadarViewModel ViewModel { get; private set; }

        internal Radar( IPointerDeviceDriver pdd )
            : this()
        {
            ViewModel = new RadarViewModel();
            ViewModel.SetCircleColor( Colors.Black );
            ViewModel.SetArrowColor( Colors.Black );
            ViewModel.Opacity = 1;
            ViewModel.RadarSize = 100;
            SnakeMode = false;
            DataContext = ViewModel;
            _mouseDriver = pdd;
            Left = pdd.CurrentPointerXLocation - ViewModel.WindowSize / 2;
            Top = pdd.CurrentPointerYLocation - ViewModel.WindowSize / 2;
            RotationSpeed = 10;
            TranslationSpeed = 1;

            CurrentStep = RadarStep.Paused;

            _timerRotate = new DispatcherTimer( DispatcherPriority.Send );
            _timerRotate.Interval = new TimeSpan( 1000 );
            _timerRotate.Tick += ProcessRotation;

            _timerTranslate = new DispatcherTimer( DispatcherPriority.Normal );
            _timerTranslate.Interval = new TimeSpan( 1000 );
            _timerTranslate.Tick += ProcessTranslation;

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

        double weight = 40;

        void ProcessScrollingTick( double newScrollingTick )
        {
            newScrollingTick /= 10000;

            //Making sure the inner timers don't get a tick interval < 1 ms
            if( newScrollingTick < weight ) newScrollingTick = (double)weight;

            if( newScrollingTick != _previousScrollingTick )
            {
                _previousScrollingTick = newScrollingTick;

                double rotateNanoTickTime = newScrollingTick / weight * Math.Pow( 10, 4 ); //interval between two ticks for the rotation timer
                _timerRotate.Interval = new TimeSpan( (long)rotateNanoTickTime );

                double translateNanoTickTime = newScrollingTick / weight * Math.Pow( 10, 4 ); //interval between two ticks for the translation timer
                _timerTranslate.Interval = new TimeSpan( (int)translateNanoTickTime );

                UpdateRotationDelay();
            }
        }

        double _previousScrollingTick = 0;
        internal void Tick( BeginScrollingInfo scrollingInfo )
        {
            ProcessScrollingTick( scrollingInfo.TickInterval );
        }

        internal void Pause()
        {
            CurrentStep = RadarStep.Paused;
            StopRotation();
            StopTranslation();
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
            else if( CurrentStep == RadarStep.Translating )
            {
                StopTranslation();
                StartRotation();
            }

            //Each time the input is triggered, we reset the lapcount and the starting angle of the lap count. (thanks to that, we release the scroller in an homogenous way : X laps after the last call to SelectElement)
            //Console.Out.WriteLine( "LapCount = 0" );
            ViewModel.LapCount = 0;
            ViewModel.StartingAngle = ViewModel.Angle;

            return CurrentStep;
        }
        
        void UpdateRotationDelay()
        {
            int revolutionTime = _timerRotate == null ? 0 : (int)(_timerRotate.Interval.TotalMilliseconds * 360f / (double)RotationSpeed);
            _rotationMaxIntervals = 360 / RotationSpeed;

            RotationDelay = revolutionTime > DEFAULT_ROTATION_DELAY ? revolutionTime : DEFAULT_ROTATION_DELAY;
        }

        #region Rotation

        void StartRotation()
        {
            _watch = Stopwatch.StartNew();
            _rotationIntervalCount = 0;
            CurrentStep = RadarStep.Rotating;
            _timerRotate.Start();
        }

        void StopRotation()
        {
            _timerRotate.Stop();
            _watch.Stop();
        }

        void ProcessRotation( object sender, EventArgs e )
        {
            if( RotationDelay <= DEFAULT_ROTATION_DELAY && _watch.ElapsedMilliseconds >= RotationDelay || RotationDelay > DEFAULT_ROTATION_DELAY && _rotationIntervalCount > _rotationMaxIntervals )
            {
                FireRotationDelayExpired();
                _rotationIntervalCount = 0;
                _watch = Stopwatch.StartNew();
            }

            ViewModel.Angle += (float)RotationSpeed / 1f;
            ++_rotationIntervalCount;
        }

        private void FireRotationDelayExpired()
        {
            if (RotationDelayExpired != null) RotationDelayExpired(this, new EventArgs());
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
            _timerTranslate.Start();
        }

        void StopTranslation()
        {
            _timerTranslate.Stop();
        }

        void ProcessTranslation( object sender, EventArgs e )
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
                        moveX = curScreen.Bounds.Bottom;
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
                        moveY = curScreen.Bounds.Top;
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
            CK.InputDriver.MouseProcessor.MoveMouseToAbsolutePosition( moveX, moveY );
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
            if( p.X - precision <= current.Bounds.Left ) 
                collision = ScreenBound.Left;
            if( p.X + precision >= current.Bounds.Right ) //X can't be equal to current.Bounds.Right (current.Bounds.Right - 1 max)
                collision = ScreenBound.Right;
            if( p.Y - precision <= current.Bounds.Top ) 
                collision = ScreenBound.Top;
            if( p.Y + precision >= current.Bounds.Bottom )
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
