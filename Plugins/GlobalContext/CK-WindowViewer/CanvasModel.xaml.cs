using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CK_WindowManager;

namespace CK_WindowViewer
{
    /// <summary>
    /// Logique d'interaction pour CanvasModel.xaml
    /// </summary>
    public partial class CanvasModel : Canvas
    {
        private IWindowsInformations i;
        public CanvasModel(IWindowsInformations i)
        {
            InitializeComponent();
            this.i = i;
        }

        public void move(int x,int y)
        {
            i.Move(x,y);
        }
        public IWindowsInformations getWindowsInfo()
        {
            return i;
        }
    }
}
