using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BasicCommandHandlers;
using CK.InputDriver;
using CK.Plugin;
using CommonServices;

namespace SnakeMode
{
    [Plugin( SnakeMode.PluginIdString, PublicName = PluginPublicName, Version = SnakeMode.PluginIdVersion,
       Categories = new string[] { "Visual", "Accessibility" } )]
    public class SnakeMode : IPlugin
    {
        const string PluginPublicName = "SnakeMode";
        const string PluginIdString = "{D237F483-A351-424D-B1B7-E84A17ED5C81}";
        const string PluginIdVersion = "0.1.0";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> PointerDriver { get; set; }

        RegionHelper _regionHelper;
        Point _lastPoint;
        bool _isActive = false;

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            _regionHelper = new RegionHelper();
            foreach( var s in Screen.AllScreens ) _regionHelper.Add( s.Bounds );

            _lastPoint = Point.Empty;
            _timer = new Timer();
            _timer.Interval = 10;
            _timer.Tick += _timer_Tick;
            _timer.Start();

            return true;
        }

        void _timer_Tick( object sender, EventArgs e )
        {
            ActiveSnakeMode();
        }

        public void Start()
        {
            PointerDriver.Service.PointerMove += Service_PointerMove;
        }

        Timer _timer;

        void Service_PointerMove( object sender, PointerDeviceEventArgs e )
        {
            if( !_isActive ) _isActive = true;
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Teardown()
        {
        }

        #endregion

        void ActiveSnakeMode()
        {
            if( _isActive )
            {
                Point newPoint = new Point( PointerDriver.Service.CurrentPointerXLocation, PointerDriver.Service.CurrentPointerYLocation );
                int nextX = newPoint.X;
                int nextY = newPoint.Y;

                Rectangle screen = Screen.GetBounds( newPoint );

                if( screen.Contains( newPoint ) )
                {
                    if( screen.Right == newPoint.X ) nextX = _regionHelper.GetMinXPosition( newPoint.Y ) + 1;
                    else if( screen.Left == newPoint.X ) nextX = _regionHelper.GetMaxXPosition( newPoint.Y ) - 1;
                    if( screen.Top == newPoint.Y ) 
                        nextY = _regionHelper.GetMaxYPosition( newPoint.Y ) - 1;
                    else if( screen.Bottom == newPoint.Y ) nextY = _regionHelper.GetMinYPosition( newPoint.Y ) + 1;
                }
                else
                {
                    if( screen.Right < newPoint.X ) nextX = _regionHelper.GetMinXPosition( newPoint.Y ) + 1;
                    else if( screen.Left > newPoint.X ) nextX = _regionHelper.GetMaxXPosition( newPoint.Y ) - 1;
                    if( screen.Top > newPoint.Y ) nextY = _regionHelper.GetMaxYPosition( newPoint.Y ) - 1;
                    else if( screen.Bottom < newPoint.Y ) nextY = _regionHelper.GetMinYPosition( newPoint.Y ) + 1;
                }

                if( nextX !=  newPoint.X || nextY != newPoint.Y) MouseProcessor.MoveMouseToAbsolutePosition( nextX, nextY );
                _isActive = false;
            }
        }
    }
}
