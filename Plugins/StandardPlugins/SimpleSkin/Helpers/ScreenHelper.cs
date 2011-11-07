using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Interop;
using System;

namespace SimpleSkin.Helper
{
    public class ScreenHelper
    {
        public static bool IsInScreen( Rectangle rect )
        {
            return Screen.AllScreens.Any( ( s ) => s.WorkingArea.Contains( rect ) );
        }

        public static bool IsInScreen( Point point )
        {
            return Screen.AllScreens.Any( ( s ) => s.WorkingArea.Contains( point ) );
        }

        public static Point GetCenterOfParentScreen( Rectangle rect )
        {
            Screen parent = Screen.FromRectangle( rect );
            return new Point( parent.WorkingArea.Width / 2, parent.WorkingArea.Height / 2 );
        }

        public static Rectangle GetPrimaryScreenSize()
        {
            return Screen.PrimaryScreen.Bounds;
        }

    }
}
