using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

        protected override void OnSourceInitialized( EventArgs e )
        {
            base.OnSourceInitialized( e );


            HwndSource source = PresentationSource.FromVisual( this ) as HwndSource;
            source.AddHook( WndProc );
        }

        private IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        {
            if( msg == Constants.WM_HOTKEY_MSG_ID )
            {
                if( HookInvoqued != null )
                {
                    HookInvoqued( this, new HookInvokedEventArgs( msg, lParam, wParam ) );
                }
            }
            return IntPtr.Zero;
        }

        public AppView()
        {
            InitializeComponent();
        }
    }
}
