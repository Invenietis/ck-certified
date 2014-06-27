#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SnakeMode\SnakeMode.cs) is part of CiviKey. 
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
    [Plugin( SnakeMode.PluginIdString, PublicName = PluginPublicName, Version = SnakeMode.PluginIdVersion )]
    public class SnakeMode : IPlugin
    {
        const string PluginPublicName = "Snake Mode";
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
            PointerDriver.Service.PointerMove -= Service_PointerMove;
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

                if( nextX != newPoint.X || nextY != newPoint.Y ) MouseProcessor.MoveMouseToAbsolutePosition( nextX, nextY );
                _isActive = false;
            }
        }
    }
}
