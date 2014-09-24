using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using System.Collections.ObjectModel;

namespace CK_WindowManager
{
    public interface IWindowManager : IDynamicService
    {
        ReadOnlyObservableCollection<IWindowsInformations> WindowList {get;}
        ReadOnlyObservableCollection<IScreenInformations> ScreenList {get;}
        int My_WorkingAreaWidth {get;}
        int My_WorkingAreaHeight {get;}
    }
}
