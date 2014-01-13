using System.Windows;
using KeyboardEditor.Tools;

namespace KeyboardEditor.s
{
    /// <summary>
    /// Interaction logic for AppView.xaml
    /// </summary>
    public partial class AppView : Window
    {
        public event HookInvokedEventHandler HookInvoqued;
        public delegate void HookInvokedEventHandler( object sender, HookInvokedEventArgs e );


        //These are global hooks, which are not what we need for this type of interface.
        //protected override void OnSourceInitialized( EventArgs e )
        //{
        //    base.OnSourceInitialized( e );
        //    HwndSource source = PresentationSource.FromVisual( this ) as HwndSource;
        //    source.AddHook( WndProc );
        //}

        //private IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        //{
        //    if( msg == Constants.WM_HOTKEY_MSG_ID )
        //    {
        //        if( HookInvoqued != null )
        //        {
        //            HookInvoqued( this, new HookInvokedEventArgs( msg, lParam, wParam ) );
        //        }
        //    }
        //    return IntPtr.Zero;
        //}

        public AppView()
        {
            InitializeComponent();
        }
    }
}
