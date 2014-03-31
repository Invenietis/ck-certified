using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using CK.Plugin;
using CK.WindowManager.Model;
using CK.Windows;

namespace CK.WindowManager
{
    [Plugin( "{BEA2BC3A-B7A1-4AF5-A86E-A039B7197BA8}", Categories = new string[] { "Accessibility" }, PublicName = "CK.WindowManager.UnbindButtonManager", Version = "1.0.0" )]
    public class UnbindButtonManager : IPlugin, IUnbindButtonManager
    {
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITopMostService> TopMostService { get; set; }

        public WindowElement CreateButton( ISpatialBinding spatialBinding, ISpatialBinding slaveSpatialBinding, BindingPosition position )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            return InitializeButton( spatialBinding, slaveSpatialBinding, position );
        }

        WindowElement InitializeButton( ISpatialBinding spatialBinding, ISpatialBinding slaveSpatialBinding, BindingPosition position )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            WindowElement button = null;

            button = new WindowElement( new UnbindButtonView()
            {
                DataContext = CreateVM( spatialBinding, slaveSpatialBinding, position )
            }, "unbindButton" );

            InitialButtonPlacing( button, spatialBinding, slaveSpatialBinding, position );

            TopMostService.Service.RegisterTopMostElement( "30", button.Window );

            return button;
        }

        public void RemoveButton( IWindowElement button )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );

            button.Window.Close();
            TopMostService.Service.UnregisterTopMostElement( button.Window );
        }

        VMUnbindButton CreateVM( ISpatialBinding spatialBinding, ISpatialBinding slaveSpatialBinding, BindingPosition position )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( position == BindingPosition.None )
                return null;

            Action<Rect,Rect> move = null;

            if( position == BindingPosition.Top )
            {
                move = ( windowRectangle, slaveWindowRectangle ) =>
                {
                    WindowManager.Service.Move( spatialBinding.Window, windowRectangle.Top + 10, windowRectangle.Left ).Broadcast();
                    WindowManager.Service.Move( slaveSpatialBinding.Window, slaveWindowRectangle.Top - 10, slaveWindowRectangle.Left ).Broadcast();
                };
            }
            else if( position == BindingPosition.Bottom )
            {
                move = ( windowRectangle, slaveWindowRectangle ) =>
                    {
                        WindowManager.Service.Move( spatialBinding.Window, windowRectangle.Top - 10, windowRectangle.Left ).Broadcast();
                        WindowManager.Service.Move( slaveSpatialBinding.Window, slaveWindowRectangle.Top + 10, slaveWindowRectangle.Left ).Broadcast();
                    };
            }
            else if( position == BindingPosition.Right )
            {
                move = ( windowRectangle, slaveWindowRectangle ) =>
                    {
                        WindowManager.Service.Move( spatialBinding.Window, windowRectangle.Top, windowRectangle.Left - 10 ).Broadcast();
                        WindowManager.Service.Move( slaveSpatialBinding.Window, slaveWindowRectangle.Top, slaveWindowRectangle.Left + 10 ).Broadcast();
                    };
            }
            else if( position == BindingPosition.Left )
            {
                move = ( windowRectangle, slaveWindowRectangle ) =>
                    {
                        WindowManager.Service.Move( spatialBinding.Window, windowRectangle.Top, windowRectangle.Left + 10 ).Broadcast();
                        WindowManager.Service.Move( slaveSpatialBinding.Window, slaveWindowRectangle.Top, slaveWindowRectangle.Left - 10 ).Broadcast();
                    };
            }

            return new VMUnbindButton( () =>
            {
                Rect windowRectangle = WindowManager.Service.GetClientArea( spatialBinding.Window );
                Rect slaveWindowRectangle = WindowManager.Service.GetClientArea( slaveSpatialBinding.Window );
                WindowBinder.Service.Unbind( spatialBinding.Window, slaveSpatialBinding.Window, false );
                move( windowRectangle, slaveWindowRectangle );
            } );
        }

        void DoPlaceButtons( WindowElement button, ISpatialBinding spatialBinding, ISpatialBinding slaveSpatialBinding, BindingPosition position )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            double top = 0;
            double height = 0;
            double width = 0;
            double left = 0;

            var pos = WindowManager.Service.GetClientArea( spatialBinding.Window );

            top = pos.Top;
            height = pos.Height;
            width = pos.Width;
            left = pos.Left;

            Action moveButtons = () =>
            {
                if( position == BindingPosition.Top )
                {
                    button.Window.Top = top - button.Window.Height / 2;
                    button.Window.Left = left + width / 2 - button.Window.Width / 2;
                }
                else if( position == BindingPosition.Bottom )
                {
                    button.Window.Top = top + height - button.Window.Height / 2;
                    button.Window.Left = left + width / 2 - button.Window.Width / 2;
                }
                else if( position == BindingPosition.Right )
                {
                    button.Window.Top = top + height / 2 - button.Window.Height / 2;
                    button.Window.Left = left + width - button.Window.Width / 2;
                }
                else if( position == BindingPosition.Left )
                {
                    button.Window.Top = top + height / 2 - button.Window.Width / 2;
                    button.Window.Left = left - button.Window.Height / 2;
                }
            };

            moveButtons();
        }

        void InitialButtonPlacing( WindowElement button, ISpatialBinding spatialBinding, ISpatialBinding slaveSpatialBinding, BindingPosition position )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            DoPlaceButtons( button, spatialBinding, slaveSpatialBinding, position );
        }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
        }

        #endregion

    }
}
