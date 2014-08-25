#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\KeyboardEditorTests\KeyCommandTests.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardEditorTests
{
    //[TestFixture( Category = "KeyCommand" )]
    //public class KeyCommandTests
    //{
    //    [Test]
    //    public void SendStringSimpleTest()
    //    {
    //        KeyCommandProviderViewModel provider = new KeyCommandProviderViewModel();

    //        Assert.NotNull( provider );
    //        Assert.That( provider.AvailableTypes.Count() == 2 );

    //        provider.CreateKeyCommand("sendString:Bonjour" );

    //        Assert.NotNull( provider.KeyCommand );
    //        Assert.That( provider.KeyCommand.Type.Description == "Permet d'écrire n'importe quelle chaine de caractère" );
    //        Assert.That( provider.KeyCommand.Type.Protocol == "sendString" );
    //        Assert.That( provider.KeyCommand.Type.Name == "Ecrire une lettre ou une phrase" );
    //        Assert.IsTrue( provider.KeyCommand.Type.IsValid );

    //        Assert.That( provider.KeyCommand.Parameter.GetParameterString() == "Bonjour" );
    //        Assert.That( provider.KeyCommand.ToString() == "sendString:Bonjour" );
    //    }

    //    [Test]
    //    public void SendKeySimpleTest()
    //    {
    //        KeyCommandProviderViewModel provider = new KeyCommandProviderViewModel();

    //        Assert.NotNull( provider );
    //        Assert.That( provider.AvailableTypes.Count() == 2 );

    //        provider.CreateKeyCommand( "sendKey:Back" );

    //        Assert.NotNull( provider.KeyCommand );
    //        Assert.That( provider.KeyCommand.Type.Description == "Permet de simuler la pression sur une touche sépciale comme Entrée, les touches F1..12, Effacer, Suppr etc..." );
    //        Assert.That( provider.KeyCommand.Type.Protocol == "sendKey" );
    //        Assert.That( provider.KeyCommand.Type.Name == "Touche spéciale (F11, Entrée, Suppr ...)" );
    //        Assert.IsTrue( provider.KeyCommand.Type.IsValid );

    //        Assert.That( provider.KeyCommand.Parameter.GetParameterString() == "Back" );
    //        Assert.That( provider.KeyCommand.ToString() == "sendKey:Back" );
    //    }
    //}
}
