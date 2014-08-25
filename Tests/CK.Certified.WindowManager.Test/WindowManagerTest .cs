#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Certified.WindowManager.Test\WindowManagerTest .cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
