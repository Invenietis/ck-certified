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
namespace MouseRadar
{
    public partial class Radar : IDisposable
    {
        IPointerDeviceDriver _mouseDriver;
        int _originX;
        int _originY;
        int _translationDirection = 1;
        int _rotationDirection = 1;
        int _rayon;

        ScreenBound _previousCollision = ScreenBound.None;
        DispatcherTimer _timerRotate;
        DispatcherTimer _timerTranslate;
        public event EventHandler<ScreenBoundCollideEventArgs> ScreenBoundCollide;
        public double Radian { get; private set; }
        public int RotationSpeed { get; set; }
        public int TranslationSpeed { get; set; }
        public RadarViewModel Model { get; private set; }
        
        public Radar(IPointerDeviceDriver pdd) : this()
        {
            Model = new RadarViewModel();
            Model.SetCircleColor( Color.FromRgb( 0, 0, 0) );
            Model.SetArrowColor( Color.FromRgb( 0, 0, 0 ) );
            Model.Opacity = 1;
            Model.RadarSize = 100;
            
            DataContext = Model;
            _mouseDriver = pdd;
            Left = pdd.CurrentPointerXLocation - Model.WindowSize / 2;
            Top = pdd.CurrentPointerYLocation - Model.WindowSize / 2;
            RotationSpeed = 1; 
            TranslationSpeed = 1;

            _timerRotate = new DispatcherTimer( DispatcherPriority.Send );
            _timerRotate.Interval = new TimeSpan( 10000 * 17 );  //60 fps
            _timerRotate.Tick += ( o, e ) =>
            {
                Model.Angle += (float)RotationSpeed / 1f;
            };

            _timerTranslate = new DispatcherTimer( DispatcherPriority.Send );
            _timerTranslate.Interval = new TimeSpan( 10000 * 17 ); //60 fps
            _timerTranslate.Tick += ProcessTranslation;

            _mouseDriver.PointerMove += OnMouseLocationChanged;
            Model.PropertyChanged += ( o, e ) =>
            {
                if( e.PropertyName == "WindowSize" )
                {
                    Width = Model.WindowSize;
                    Height = Model.WindowSize;
                }
            };
        }


        public void Launch()
        {
            PresentationSource source = PresentationSource.FromVisual( this );
            Model.ScreenScale = new Point( source.CompositionTarget.TransformToDevice.M11, source.CompositionTarget.TransformToDevice.M22 );
            StartRotation();
            UpdateLocation(_mouseDriver.CurrentPointerXLocation, _mouseDriver.CurrentPointerYLocation);
        }

        private ScreenBound CheckBoundCollision( Point p,  int precision = 0 )
        {
            Screen current = Screen.FromPoint( new System.Drawing.Point( (int)p.X, (int)p.Y ) );

            ScreenBound collision = ScreenBound.None;
            if( p.X - precision <= current.Bounds.Left ) collision = ScreenBound.Left;
            if( p.X + precision >= current.Bounds.Right - 1 ) //X can't be equel to current.Bounds.Right (current.Bounds.Right - 1 max)
                collision = ScreenBound.Right; 
            if( p.Y - precision <= current.Bounds.Top ) collision = ScreenBound.Top;
            if( p.Y + precision >= current.Bounds.Bottom - 1)
                collision = ScreenBound.Bottom; //Y can't be equel to current.Bounds.Bottom (current.Bounds.Bottom - 1 max)

            return collision;
        }

        void ProcessTranslation(object sender, EventArgs e)
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
                    moveX = 0;
                    moveY = (int)p.Y;
                    break;
                case ScreenBound.Top:
                    moveY = 0;
                    moveX = (int)p.X;
                    break;
                case ScreenBound.Right:
                    moveX = curScreen.Bounds.Right;
                    moveY = (int)p.Y;
                    break;
                case ScreenBound.Bottom:
                    moveY = curScreen.Bounds.Bottom;
                    moveX = (int)p.X;
                    break;
            }

            if( collision != ScreenBound.None ) FireScreenBoundCollide( collision );
            _mouseDriver.MovePointer( moveX, moveY ); 
        }

        public void StartRotation()
        {
            _timerRotate.Start();
        }
        public bool IsTranslating()
        {
            return _timerTranslate.IsEnabled;
        }
        public void EndRotation()
        {
            _timerRotate.Stop();
        }

        public void StartTranslation()
        {
            _originX =  _mouseDriver.CurrentPointerXLocation;
            _originY = _mouseDriver.CurrentPointerYLocation;
            Radian = Model.Angle / 180 * Math.PI;
            _rayon = 0;

            _timerRotate.Stop();
            _timerTranslate.Start();
        }

        public void StopTranslation(bool startRotation = true)
        {
            _timerTranslate.Stop();
            if( startRotation ) _timerRotate.Start();
        }

        void OnMouseLocationChanged( object sender, PointerDeviceEventArgs e )
        {
            UpdateLocation(e.X, e.Y );
        }

        public void UpdateLocation(int x, int y)
        {
            Left = (x / Model.ScreenScale.X) - (Model.WindowSize / 2);
            Top = (y/ Model.ScreenScale.Y) - (Model.WindowSize / 2);

            if( _previousCollision != ScreenBound.None && CheckBoundCollision( new Point( x, y ) ) == ScreenBound.None )
                FireScreenBoundCollide( ScreenBound.None );
        }

        Point GetTranslation( int x, int y )
        {

            double rad = Model.Angle / 180 * Math.PI;
            _rayon += TranslationSpeed;
            return new Point( _originX + _rayon * Math.Cos( rad ), _originY + _rayon * Math.Sin( rad ) );
        }

        void FireScreenBoundCollide(ScreenBound bound)
        {
            _previousCollision = bound;
            if( ScreenBoundCollide != null ) 
                ScreenBoundCollide(this, new ScreenBoundCollideEventArgs(bound));
        }

        #region IDisposable Members

        public void Dispose()
        {
            _timerRotate.Stop();
            _timerTranslate.Stop();
            _mouseDriver.PointerMove -= OnMouseLocationChanged;
            this.Close();
        }

        #endregion
    }

    public enum ScreenBound
    {
        None,
        Left,
        Top,
        Right,
        Bottom
    }

    public class ScreenBoundCollideEventArgs : EventArgs
    {
        public ScreenBound ScreenBound { get; private set; }

        public ScreenBoundCollideEventArgs(ScreenBound b)
        {
            ScreenBound = b;
        }
    }
}
