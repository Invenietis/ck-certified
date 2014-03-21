using System;
using System.Windows;
using CK.Windows;

namespace CK.WindowManager.Model
{
    public class PreviewBindingInfo
    {
        public IBinding Binding;
        public CKWindow Window;

        public BindingPosition Position
        {
            get { return Binding != null ? Binding.Position : BindingPosition.None; }
        }

        public bool HasPreview
        {
            get { return Position != BindingPosition.None; }
        }

        public void Shutdown()
        {
            if( HasPreview )
            {
                Binding = null;
                if( Window != null ) Window.Dispatcher.BeginInvoke( new Action( () => Window.Hide() ) );
            }
        }

        public void Display( IBinding binding )
        {
            if( binding == null ) return;

            if( Window == null )
            {
                Window = new CKWindow();
            }
            Binding = binding;

            Rect r = Binding.GetWindowArea();
            if( r != Rect.Empty )
            {
                Window.Dispatcher.BeginInvoke( new Action( () =>
                {
                    Window.Opacity = .8;
                    Window.Background = new System.Windows.Media.SolidColorBrush( System.Windows.Media.Color.FromRgb( 152, 120, 152 ) );
                    Window.ResizeMode = ResizeMode.NoResize;
                    Window.WindowStyle = WindowStyle.None;
                    Window.ShowInTaskbar = false;
                    Window.Show();
                    Window.Left = r.Left;
                    Window.Top = r.Top;
                    Window.Width = r.Width;
                    Window.Height = r.Height;
                } ) );
            }
        }

        public bool IsPreviewOf( IBinding newBinding )
        {
            if( newBinding == null ) throw new ArgumentNullException( "binding" );

            if( Binding == null ) return false;
            return newBinding.Position == Binding.Position && Binding.Target == newBinding.Target;
        }
    }

}
