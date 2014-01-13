using CK.Windows;

namespace CK.WindowManager.Model
{
    public static class WindowManagerExtensions
    {

        public static void RegisterWindow( this IWindowManager m, string name, CKWindow window )
        {
            m.Register( new WindowElement( window, name ) );
        }

        public static void UnregisterWindow( this IWindowManager m, string name )
        {
            IWindowElement el = m.GetByName( name );
            if( el != null ) m.Unregister( el );
        }
    }
}
