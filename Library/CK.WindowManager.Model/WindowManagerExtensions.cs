using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CK.WindowManager.Model
{
    public static class WindowManagerExtensions
    {

        public static void RegisterWindow( this IWindowManager m, string name, Window window )
        {
            m.Register( new WindowElement( window, "Skin" ) );
        }

        public static void UnregisterWindow( this IWindowManager m, string name )
        {
            IWindowElement el = m.GetByName( name );
            if( el != null ) m.Unregister( el );
        }
    }
}
