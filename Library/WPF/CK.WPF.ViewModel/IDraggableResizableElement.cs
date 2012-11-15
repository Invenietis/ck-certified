using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.WPF.ViewModel
{
    /// <summary>
    /// Interface of elements that have the properties necessary to move ans resize them in a canvas : X, Y, Width, Height
    /// </summary>
    public interface IDraggableResizableElement
    {
        int X { get; set; }
        int Y { get; set; }
        int Width { get; set; }
        int Height { get; set; }
    }
}
