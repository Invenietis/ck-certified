using System;
using CK.Plugin;
using CK.WindowManager.Model;
using CK.Windows;

namespace SimpleSkin
{
    public partial class SimpleSkin
    {
        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        WindowManagerSubscriber _subscriber;

        partial void OnSuccessfulStart()
        {
            _subscriber = new WindowManagerSubscriber( WindowManager, WindowBinder );
            _skinDispatcher.BeginInvoke( new Action( () =>
            {
                _subscriber.WindowRegistered = ( e ) =>
                {
                    e.Window.Hidden += OnWindowHidden;
                };
                _subscriber.WindowUnregistered = ( e ) =>
                {
                    e.Window.Hidden -= OnWindowHidden;
                };
                _subscriber.Subscribe( "Skin", _skinWindow );
                _skinWindow.HidingAsked += OnWindowHidden;

            } ) );


        }

        void OnWindowHidden( object sender, EventArgs e )
        {
            HideSkin();
        }

        partial void OnSuccessfulStop()
        {
            _skinDispatcher.BeginInvoke( new Action( () =>
            {
                _skinWindow.HidingAsked -= OnWindowHidden;
            } ) );
            _subscriber.Unsubscribe();
        }

        /// <summary>
        /// Hides the skin and shows the keyboard's MiniView
        /// </summary>
        public void HideSkin()
        {
            if( !_viewHidden )
            {
                _viewHidden = true;

                _skinDispatcher.BeginInvoke( (Action)( () =>
                {
                    ShowMiniView();
                    if( Highlighter.Status == InternalRunningStatus.Started )
                    {
                        Highlighter.Service.RegisterTree( "MinimizeKeyboard", _miniViewVm );
                        Highlighter.Service.UnregisterTree( "Keyboard", _ctxVm.KeyboardVM );
                    }
                    if( _timer != null ) _timer.Stop();
                } ), null );
            }
        }

    }
}
