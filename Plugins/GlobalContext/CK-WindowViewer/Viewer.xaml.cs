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
using System.Diagnostics;
using System.Runtime.InteropServices;
using CK_WindowManager;
using System.Collections.ObjectModel;

namespace CK_WindowViewer
{
    /// <summary>
    /// Logique d'interaction pour Viewer.xaml
    /// </summary>
    public partial class Viewer : Window
    {
        private Canvas myParentCanvas;
        bool boolette = false;
        private Canvas uniqueCanvas;
        ReadOnlyObservableCollection<IScreenInformations> iScreen;
        public Viewer(ReadOnlyObservableCollection<IScreenInformations> iScreen)
        {
            InitializeComponent();
            myParentCanvas = new Canvas();
            myParentCanvas.AllowDrop = true;
            this.iScreen = iScreen;
            this.AddChild(myParentCanvas);

        }
        public Canvas getCanvas()
        {
            return myParentCanvas;
        }
        public void CreateRect(IWindowsInformations i, int length, int z, int height, int width, int x, int y, double op, SolidColorBrush col)
        {
            CanvasModel myCanvas = new CanvasModel(i);
            myCanvas.Background = col;
            myCanvas.Height = height;
            myCanvas.Width = width;
            myCanvas.Opacity = op;
            myCanvas.MouseMove += new MouseEventHandler(MouseMoveMethode);
            myCanvas.DragOver += new DragEventHandler(DragOverMethode);
            Canvas.SetTop(myCanvas, y);
            Canvas.SetLeft(myCanvas, x);
            TextBlock text = new TextBlock();
            text.Text = (z).ToString();
            text.FontSize = 30;
            myCanvas.Children.Add(text);
            Canvas.SetZIndex(myCanvas, length - z);
            myParentCanvas.Children.Add(myCanvas);
        }
        public void updateCanvasModel(CanvasModel myCanvas, IWindowsInformations i, int length, int z, int height, int width, int x, int y)
        {
            myCanvas.Children.Clear();
            myCanvas.Height = height;
            myCanvas.Width = width;
            Canvas.SetTop(myCanvas, y);
            Canvas.SetLeft(myCanvas, x);
            Canvas.SetZIndex(myCanvas, length - z);
            TextBlock text = new TextBlock();
            text.Text = (z).ToString();
            text.FontSize = 30;
            myCanvas.Children.Add(text);
        }
        private void MouseMoveMethode(object sender, MouseEventArgs e)
        {
            if (!boolette)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {

                    boolette = true;
                    DragDropEffects effects;
                    DataObject obj = new DataObject();
                    uniqueCanvas = (CanvasModel)sender;
                    obj.SetData(typeof(CanvasModel), uniqueCanvas);
                    effects = DragDrop.DoDragDrop(uniqueCanvas, obj, DragDropEffects.Move);
                }
            }
            else
            {
                if (e.LeftButton == MouseButtonState.Released)
                {
                    boolette = false;
                }
            }


        }

        private void DragOverMethode(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(typeof(CanvasModel)))
            {
                Point p = e.GetPosition(myParentCanvas);
                e.Effects = DragDropEffects.Move;
                CanvasModel source = (CanvasModel)uniqueCanvas;
                int x = (int)(p.X * 3 - source.Width * 3 / 2);
                int y = (int)(p.Y * 3 - source.Height * 3 / 2);

                if (iScreen.Count == 2)
                {
                    for (int j = 0; j < iScreen.Count; j++)
                    {

                        if (!iScreen[j].IsPrimaryScreen && iScreen[j].Bounds.Y < 0)
                        {
                            y = (int)((p.Y * 3 - Math.Abs(iScreen[j].Bounds.Y)) - source.Height * 3 / 2);
                        }
                        if (!iScreen[j].IsPrimaryScreen && iScreen[j].Bounds.X < 0)
                        {
                            x = (int)((p.X * 3 - Math.Abs(iScreen[j].Bounds.X)) - source.Width * 3 / 2);
                        }
                    }
                }

                source.move(x, y);
            }
            else
            {

                e.Effects = DragDropEffects.None;

            }

        }
        public void resetCanvas()
        {
            myParentCanvas.Children.RemoveRange(0, myParentCanvas.Children.Count);
        }
    }
}
