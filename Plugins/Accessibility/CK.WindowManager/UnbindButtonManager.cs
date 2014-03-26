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
            if( NoFocusManager.Default.NoFocusDispatcher.CheckAccess() )
            {
                return InitializeButton( spatialBinding, slaveSpatialBinding, position );
            }
            else
            {
                return (WindowElement)NoFocusManager.Default.NoFocusDispatcher.Invoke( (Func<WindowElement>)(() =>
                    {
                        return InitializeButton( spatialBinding, slaveSpatialBinding, position );
                    }) );
            }
        }

        WindowElement InitializeButton( ISpatialBinding spatialBinding, ISpatialBinding slaveSpatialBinding, BindingPosition position )
        {
            WindowElement button = null;

            button = new WindowElement( new UnbindButtonView( NoFocusManager.Default )
            {
                DataContext = CreateVM( spatialBinding, slaveSpatialBinding, position )
            }, "unbindButton" );

            InitialButtonPlacing( button, spatialBinding, slaveSpatialBinding, position );

            TopMostService.Service.RegisterTopMostElement( "30", button.Window );

            return button;
        }

        public void DeleteButton( IWindowElement button )
        {
            button.Window.Dispatcher.Invoke( (Action)(() =>
                {
                    button.Window.Close();
                    TopMostService.Service.UnregisterTopMostElement( button.Window );
                }) );
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
                        WindowManager.Service.Move( spatialBinding.Window, spatialBinding.Window.Top + 10, spatialBinding.Window.Left ).Broadcast();
                        WindowManager.Service.Move( slaveSpatialBinding.Window, slaveSpatialBinding.Window.Top - 10, slaveSpatialBinding.Window.Left ).Broadcast();
                    };
            }
            else if( position == BindingPosition.Bottom )
            {
                move = () =>
                    {
                        WindowManager.Service.Move( spatialBinding.Window, spatialBinding.Window.Top - 10, spatialBinding.Window.Left ).Broadcast();
                        WindowManager.Service.Move( slaveSpatialBinding.Window, slaveSpatialBinding.Window.Top + 10, slaveSpatialBinding.Window.Left ).Broadcast();
                    };
            }
            else if( position == BindingPosition.Right )
            {
                move = () =>
                    {
                        WindowManager.Service.Move( spatialBinding.Window, spatialBinding.Window.Top, spatialBinding.Window.Left - 10 ).Broadcast();
                        WindowManager.Service.Move( slaveSpatialBinding.Window, slaveSpatialBinding.Window.Top, slaveSpatialBinding.Window.Left + 10 ).Broadcast();
                    };
            }
            else if( position == BindingPosition.Left )
            {
                move = () =>
                    {
                        WindowManager.Service.Move( spatialBinding.Window, spatialBinding.Window.Top, spatialBinding.Window.Left + 10 ).Broadcast();
                        WindowManager.Service.Move( slaveSpatialBinding.Window, slaveSpatialBinding.Window.Top, slaveSpatialBinding.Window.Left - 10 ).Broadcast();
                    };
            }

            return new VMUnbindButton( () =>
            {
                WindowBinder.Service.Unbind( spatialBinding.Window, slaveSpatialBinding.Window, false );
                move();
            } );
        }

        void DoPlaceButtons( WindowElement button, ISpatialBinding spatialBinding, ISpatialBinding slaveSpatialBinding, BindingPosition position )
        {
            double top = 0;
            double height = 0;
            double width = 0;
            double left = 0;

            //We necessarily are on the spatialBinding Window's thread
            top = spatialBinding.Window.Window.Top;
            height = spatialBinding.Window.Window.Height;
            width = spatialBinding.Window.Window.Width;
            left = spatialBinding.Window.Window.Left;

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

            if( !button.Window.Dispatcher.CheckAccess() )
            {
                //Button placing is not crucial, so we can safely use BeginInvoke.
                button.Window.Dispatcher.BeginInvoke( (Action)(() =>
                {
                    moveButtons();
                }) );
            }
            else
            {
                moveButtons();
            }
        }

        void InitialButtonPlacing( WindowElement button, ISpatialBinding spatialBinding, ISpatialBinding slaveSpatialBinding, BindingPosition position )
        {
            if( !spatialBinding.Window.Window.Dispatcher.CheckAccess() )
            {
                //Button placing is not crucial, so we can safely use BeginInvoke
                spatialBinding.Window.Window.Dispatcher.BeginInvoke( (Action)(() => DoPlaceButtons( button, spatialBinding, slaveSpatialBinding, position )) );
            }
            else
            {
                DoPlaceButtons( button, spatialBinding, slaveSpatialBinding, position );
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
