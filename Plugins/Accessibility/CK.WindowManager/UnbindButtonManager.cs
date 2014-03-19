using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            WindowElement button = null;

            button = new WindowElement( new UnbindButtonView( NoFocusManager.Default )
            {
                DataContext = CreateVM( spatialBinding, slaveSpatialBinding, position )
            }, "unbindButton" );

            TopMostService.Service.RegisterTopMostElement( "30", button.Window );

            PlacingButton( button, spatialBinding, slaveSpatialBinding, position );

            return button;
        }

        public void DeleteButton( IWindowElement button )
        {
            if( TopMostService.Status == InternalRunningStatus.Started )
            {
                TopMostService.Service.UnregisterTopMostElement( button.Window );
            }
            button.Window.Dispatcher.Invoke( (Action)(() => button.Window.Hide()) );
        }

        VMUnbindButton CreateVM( ISpatialBinding spatialBinding, ISpatialBinding slaveSpatialBinding, BindingPosition position )
        {
            if( position == BindingPosition.None ) 
                return null;
            
            Action move = null;

            if( position == BindingPosition.Top )
            {
                move = () =>
                    {
                        WindowManager.Service.Move( spatialBinding.Window, spatialBinding.Window.Top + 10, spatialBinding.Window.Left ).Silent();
                        WindowManager.Service.Move( slaveSpatialBinding.Window, slaveSpatialBinding.Window.Top - 10, slaveSpatialBinding.Window.Left ).Silent();
                    };
            }
            else if( position == BindingPosition.Bottom )
            {
                move = () =>
                    {
                        WindowManager.Service.Move( spatialBinding.Window, spatialBinding.Window.Top - 10, spatialBinding.Window.Left ).Silent();
                        WindowManager.Service.Move( slaveSpatialBinding.Window, slaveSpatialBinding.Window.Top + 10, slaveSpatialBinding.Window.Left ).Silent();
                    };
            }
            else if( position == BindingPosition.Right )
            {
                move = () =>
                    {
                        WindowManager.Service.Move( spatialBinding.Window, spatialBinding.Window.Top, spatialBinding.Window.Left - 10 ).Silent();
                        WindowManager.Service.Move( slaveSpatialBinding.Window, slaveSpatialBinding.Window.Top, slaveSpatialBinding.Window.Left + 10 ).Silent();
                    };
            }
            else if( position == BindingPosition.Left )
            {
                move = () =>
                    {
                        WindowManager.Service.Move( spatialBinding.Window, spatialBinding.Window.Top, spatialBinding.Window.Left + 10 ).Silent();
                        WindowManager.Service.Move( slaveSpatialBinding.Window, slaveSpatialBinding.Window.Top, slaveSpatialBinding.Window.Left - 10 ).Silent();
                    };
            }

            return new VMUnbindButton( () =>
            {
                WindowBinder.Service.Unbind( spatialBinding.Window, slaveSpatialBinding.Window, false );
                move();
            } );
        }

        void PlacingButton( WindowElement button, ISpatialBinding spatialBinding, ISpatialBinding slaveSpatialBinding, BindingPosition position )
        {
            if( position == BindingPosition.Top )
            {
                button.Window.Top = spatialBinding.Window.Top - button.Window.Height / 2;
                button.Window.Left = spatialBinding.Window.Left + spatialBinding.Window.Width / 2 - button.Window.Width / 2;
            }
            else if( position == BindingPosition.Bottom )
            {
                button.Window.Top = spatialBinding.Window.Top + spatialBinding.Window.Height - button.Window.Height / 2;
                button.Window.Left = spatialBinding.Window.Left + spatialBinding.Window.Width / 2 - button.Window.Width / 2;
            }
            else if( position == BindingPosition.Right )
            {
                button.Window.Top = spatialBinding.Window.Top + spatialBinding.Window.Height / 2;
                button.Window.Left = spatialBinding.Window.Left + spatialBinding.Window.Width - button.Window.Width / 2;
            }
            else if( position == BindingPosition.Left )
            {
                button.Window.Top = spatialBinding.Window.Top + spatialBinding.Window.Height / 2 - button.Window.Width / 2;
                button.Window.Left = spatialBinding.Window.Left - button.Window.Height / 2;
            }
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
