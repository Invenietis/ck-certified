using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Moq;
using CK.WindowManager.Model;
using System.Windows;
using System.Windows.Threading;
using CK.Core;
using CK.WindowManager;

namespace CK.Certified.WindowManager.Test
{
    [TestFixture( Category = "WindowManager" )]
    public class WindowManagerTest
    {
        [Test, RequiresSTA]
        public void WindowManagerApiTest()
        {
            IWindowManager windowManager = new CK.WindowManager.WindowManager();
            WindowElement A = new WindowElement( windowManager, new Window() { Top = 10, Left = 10, Width = 10, Height = 10 },"A" );
            WindowElement B = new WindowElement( windowManager, new Window() { Top = 10, Left = 20, Width = 10, Height = 10 },"B" );
            WindowElement C = new WindowElement( windowManager, new Window() { Top = 10, Left = 30, Width = 10, Height = 10 },"C" );
            WindowElement D = new WindowElement( windowManager, new Window() { Top = 20, Left = 20, Width = 10, Height = 10 },"D" );
            WindowElement E = new WindowElement( windowManager, new Window() { Top = 20, Left = 40, Width = 10, Height = 10 },"E" );
            WindowElement F = new WindowElement( windowManager, new Window(), "F");

            // B is linked to A C D
            //  A has 1 binding: AB. 
            //  B has 3 bindings: BA BC BD. 
            //  C has 1 binding: CB.
            //  D has 1 binding: DB.
            //  E has no binding.
            //  
            //   A-B-C  
            //     |    E
            //     D

            WindowElementBinder binder = new WindowElementBinder();
            binder.BeforeBinding += ( sender, e ) =>
            {
            };

            WindowBindedEventArgs bindedEvent = null;

            binder.AfterBinding += ( sender, e ) =>
            {
                bindedEvent = e;
            };


            // The listener plugin listen the window location change event directly from the WindowManager.
            // When a Window from A B C or D change the location, all the window must move together.
            WindowManagerExecutor executor = new WindowManagerExecutor();
            executor.WindowManager = windowManager;
            executor.WindowBinder = binder;
            executor.Start();

            binder.Attach( A, B );
            Assert.That( bindedEvent, Is.Not.Null );
            Assert.That( bindedEvent.BindingType == BindingEventType.Attach );
            Assert.That( bindedEvent.Binding, Is.Not.Null );
            Assert.That( bindedEvent.Binding.First == A );
            Assert.That( bindedEvent.Binding.Second == B );

            binder.Attach( B, C );
            Assert.That( bindedEvent.BindingType == BindingEventType.Attach );
            Assert.That( bindedEvent.Binding, Is.Not.Null );
            Assert.That( bindedEvent.Binding.First == B );
            Assert.That( bindedEvent.Binding.Second == C );

            binder.Attach( B, D );
            Assert.That( bindedEvent.BindingType == BindingEventType.Attach );
            Assert.That( bindedEvent.Binding, Is.Not.Null );
            Assert.That( bindedEvent.Binding.First == B );
            Assert.That( bindedEvent.Binding.Second == D );

            A.Window.Left = 15;
            A.OnWindowLocationChanged( A, EventArgs.Empty );

            executor.Stop();
        }

        
    }
}
