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
