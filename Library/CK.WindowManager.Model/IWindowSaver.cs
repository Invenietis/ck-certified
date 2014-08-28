using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CK.Plugin;

namespace CK.WindowManager.Model
{
    public interface IWindowSaver : IDynamicService
    {
        void SavePlacement( Window window );

        bool GetPlacement( Window window );

        void AutomaticSaveWindow( Window window );

        void InitWindowPlacement( Window window );
    }
}
