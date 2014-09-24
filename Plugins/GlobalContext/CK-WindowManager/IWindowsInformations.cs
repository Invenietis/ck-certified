using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace CK_WindowManager
{
    public interface IWindowsInformations : INotifyPropertyChanged
    {
        int X { get; set; }
        int Y {get;set;}
        int Z {get;set;}
        int Width {get;set;}
        int Height {get;set;}
        String Title {get;set;}
        IScreenInformations ScreenInfo {get;set;}
        Rectangle Rect { get;}
        void Move(int x,int y);
        void Close();
        
    }
}
