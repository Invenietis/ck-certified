using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Moq;
using CK.WindowManager.Model;
using System.Windows;

namespace CK.Certified.WindowManager.Test
{
    [TestFixture]
    public class WindowManagerTest
    {
        [Test]
        public void WindowManagerApiTest()
        {
            SkinPlugin skin = new SkinPlugin();
            skin.WindowManager = Mock.Of<IWindowManager>();
        }

        class SkinPlugin : IWindowElement
        {
            Window _w;

            public IWindowManager WindowManager { get; set; }

            public event EventHandler LocationChanged;

            public event EventHandler SizeChanged;

            public event EventHandler Hidden;

            public event EventHandler Restored;

            public SkinPlugin()
            {
                WindowManager.Register( this );
                _w.LocationChanged += _w_LocationChanged;
                _w.SizeChanged += _w_SizeChanged;
            }

            void _w_SizeChanged( object sender, SizeChangedEventArgs e )
            {
                if( SizeChanged != null )
                    SizeChanged( sender, EventArgs.Empty );
            }

            void _w_LocationChanged( object sender, EventArgs e )
            {
                if( LocationChanged != null )
                    LocationChanged( sender, e );
            }

            public string Name { get; set; }

            double IWindowElement.Left
            {
                get { return _w.Left; }
            }

            double IWindowElement.Top
            {
                get { return _w.Top; }
            }

            double IWindowElement.Width
            {
                get { return _w.Width; }
            }

            double IWindowElement.Height
            {
                get { return _w.Height; }
            }
        }

    }
}
