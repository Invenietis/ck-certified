using System;
using System.Windows;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using System.ComponentModel;
using CK.Core;
using CK.Interop;
using CK.Windows;

namespace SimpleSkin
{
    /// <summary>
    /// Logique d'interaction pour SkinWindow.xaml
    /// </summary>
    public partial class SkinWindow : NoFocusWindow
    {
        public SkinWindow( object dc )
        {
            this.DataContext = dc;
            InitializeComponent();
        } 
        
    }
}
