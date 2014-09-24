using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace CK_WindowManager
{
    public interface IScreenInformations : INotifyPropertyChanged
    {
        string DeviceName {get; set;}
        Rectangle Bounds {get; set;}
        Rectangle WorkingArea {get; set;}
        bool IsPrimaryScreen {get;set;}
    }
}
