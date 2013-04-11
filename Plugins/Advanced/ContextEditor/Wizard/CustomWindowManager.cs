using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Caliburn.Micro;

namespace KeyboardEditor
{
    public class CustomWindowManager : WindowManager
    {
        protected override Window EnsureWindow( object model, object view, bool isDialog )
        {
            Console.Out.WriteLine( "Ensure window called" ); 
            Window window = base.EnsureWindow( model, view, isDialog );
            window.SizeToContent = SizeToContent.WidthAndHeight;
            return window;
        }
    }
}
