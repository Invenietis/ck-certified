using KeyboardEditor.KeyboardEdition;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardEditorTests
{
    [TestFixture( Category = "KeyCommand" )]
    public class KeyCommandTests
    {
        [Test]
        public void SendStringSimpleTest()
        {
            KeyCommandProviderViewModel provider = new KeyCommandProviderViewModel();

            Assert.NotNull( provider );
            Assert.That( provider.AvailableTypes.Count() == 2 );

            provider.CreateKeyCommand("sendString:Bonjour" );

            Assert.NotNull( provider.KeyCommand );
            Assert.That( provider.KeyCommand.Type.Description == "Permet d'écrire n'importe quelle chaine de caractère" );
            Assert.That( provider.KeyCommand.Type.Protocol == "sendString" );
            Assert.That( provider.KeyCommand.Type.Name == "Ecrire une lettre ou une phrase" );
            Assert.IsTrue( provider.KeyCommand.Type.IsValid );

            Assert.That( provider.KeyCommand.Parameter.GetParameterString() == "Bonjour" );
            Assert.That( provider.KeyCommand.ToString() == "sendString:Bonjour" );
        }

        [Test]
        public void SendKeySimpleTest()
        {
            KeyCommandProviderViewModel provider = new KeyCommandProviderViewModel();

            Assert.NotNull( provider );
            Assert.That( provider.AvailableTypes.Count() == 2 );

            provider.CreateKeyCommand( "sendKey:Back" );

            Assert.NotNull( provider.KeyCommand );
            Assert.That( provider.KeyCommand.Type.Description == "Permet de simuler la pression sur une touche sépciale comme Entrée, les touches F1..12, Effacer, Suppr etc..." );
            Assert.That( provider.KeyCommand.Type.Protocol == "sendKey" );
            Assert.That( provider.KeyCommand.Type.Name == "Touche spéciale (F11, Entrée, Suppr ...)" );
            Assert.IsTrue( provider.KeyCommand.Type.IsValid );

            Assert.That( provider.KeyCommand.Parameter.GetParameterString() == "Back" );
            Assert.That( provider.KeyCommand.ToString() == "sendKey:Back" );
        }
    }
}
