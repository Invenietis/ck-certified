#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\CK.WindowManager\WindowAutoBinder.cs) is part of CiviKey. 
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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CK.Plugin;
using CK.WindowManager.Model;
using CommonServices;
using System.Timers;
using System;
using System.Diagnostics;
using System.Windows.Threading;
using CK.Core;

namespace CK.WindowManager
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion )]
    public class WindowAutoBinder : IPlugin
    {
        #region Plugin description

        const string PluginGuidString = "{B63BB144-1C13-4A3B-93BD-AC5233F4F18E}";
        const string PluginVersion = "0.1.0";
        const string PluginPublicName = "CK.WindowManager.AutoBinder";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowManager WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowBinder WindowBinder { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IPointerDeviceDriver PointerDeviceDriver { get; set; }

        public double AttractionRadius = 65;

        HitTester _tester;
        IBindResult _bindResult;

        bool _resizeMoveLock; //avoids bind during a resize
        bool _pointerDownLock; //avoids bind during window.Move()

        public WindowAutoBinder()
        {
            _tester = new HitTester();
        }

        #region PointerDeviceDriver Members

        private DispatcherTimer _activationTimer;

        //TODO test if the pointer is in the window
        void OnPointerButtonUp( object sender, PointerDeviceEventArgs e )
        {
            //DIRTYFIX
            //Allows the bypass the fact that Windows puts a window to the initial position
            //if the windows was moved during the PointerKeyUp treatment event
            if( _bindResult != null && _activationTimer == null )
            {
                _activationTimer = new DispatcherTimer();
                _activationTimer.Interval = new TimeSpan( 0, 0, 0, 0, 50 );
                _activationTimer.Tick += _activationTimer_Tick;
                _activationTimer.Start();
            }

            _resizeMoveLock = false;
            _pointerDownLock = true;
        }

        void _activationTimer_Tick( object sender, EventArgs e )
        {
            try
            {
                if( _bindResult != null )
                {
                    //Console.WriteLine( "Elapsed OnPointerButtonUp Seal !" );
                    _bindResult.Seal();
                }
            }
            finally
            {
                _bindResult = null;
                _activationTimer.Stop();
                _activationTimer = null;
            }
        }

        #endregion PointerDeviceDriver Members

        #region WindowBinder Members

        void OnBeforeBinding( object sender, WindowBindingEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );

            _tester.Block();
        }

        bool _afterUnbind;
        void OnAfterBinding( object sender, WindowBindedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );

            _tester.Release();
            _afterUnbind = e.BindingType == BindingEventType.Detach;
        }

        #endregion WindowBinder Members

        #region WindowManager Members

        //avoids bind during a resize
        void OnWindowResized( object sender, WindowElementResizeEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );

            _resizeMoveLock = true;
            _firstMove = true;
        }

        bool _firstMove = true;
        void OnWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );

            //avoids bind during a resize
            if( !_resizeMoveLock && !_pointerDownLock )
            {
                if( !_firstMove )
                {
                    if( _tester.CanTest )
                    {
                        ISpatialBinding binding = WindowBinder.GetBinding( e.Window );
                        IDictionary<IWindowElement, Rect> rect = WindowManager.WindowElements.ToDictionary( x => x, y => WindowManager.GetClientArea( y ) );

                        IBinding result = _tester.Test( binding, rect, AttractionRadius );
                        if( result != null )
                        {
                            if( !_afterUnbind ) _bindResult = WindowBinder.PreviewBind( result.Target, result.Origin, result.Position );
                        }
                        else
                        {
                            if( _tester.LastResult != null )
                            {
                                WindowBinder.PreviewUnbind( _tester.LastResult.Target, _tester.LastResult.Origin );
                                _bindResult = null;
                            }
                            _afterUnbind = false;
                        }
                    }
                }
                _firstMove = false;
            }
        }

        #endregion WindowManager Members

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            _pointerDownLock = true;
            return true;
        }

        public void Start()
        {
            PointerDeviceDriver.PointerButtonUp += OnPointerButtonUp;
            PointerDeviceDriver.PointerButtonDown += OnPointerButtonDown;

            WindowBinder.BeforeBinding += OnBeforeBinding;
            WindowBinder.AfterBinding += OnAfterBinding;

            WindowManager.WindowMoved += OnWindowMoved;
            WindowManager.WindowResized += OnWindowResized;
        }

        //avoids bind during window.Move()
        void OnPointerButtonDown( object sender, PointerDeviceEventArgs e )
        {
            _pointerDownLock = false;
        }

        public void Stop()
        {
            PointerDeviceDriver.PointerButtonUp -= OnPointerButtonUp;
            PointerDeviceDriver.PointerButtonDown -= OnPointerButtonDown;

            WindowBinder.AfterBinding -= OnAfterBinding;
            WindowBinder.BeforeBinding -= OnBeforeBinding;

            WindowManager.WindowMoved -= OnWindowMoved;
            WindowManager.WindowResized -= OnWindowResized;
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
