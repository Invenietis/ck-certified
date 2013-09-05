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
            KeyCommandTypeProviderViewModel provider = new KeyCommandTypeProviderViewModel();

            Assert.NotNull( provider );
            Assert.That( provider.AvailableTypes.Count() == 2 );

            KeyCommandViewModel kcvm = new KeyCommandViewModel( provider, "sendString:Bonjour" );

            Assert.NotNull( kcvm );
            Assert.That( kcvm.Type.Description == "Permet d'écrire n'importe quelle chaine de caractère" );
            Assert.That( kcvm.Type.InnerName == "sendString" );
            Assert.That( kcvm.Type.Name == "Ecrire une lettre ou une phrase" );
            Assert.IsTrue( kcvm.Type.IsValid );

            Assert.That( kcvm.Parameter.GetParameterString() == "Bonjour" );
            Assert.That( kcvm.ToString() == "sendString:Bonjour" );
        }

        [Test]
        public void SendKeySimpleTest()
        {
            KeyCommandTypeProviderViewModel provider = new KeyCommandTypeProviderViewModel();

            Assert.NotNull( provider );
            Assert.That( provider.AvailableTypes.Count() == 2 );

            KeyCommandViewModel kcvm = new KeyCommandViewModel( provider, "sendKey:Back" );

            Assert.NotNull( kcvm );
            Assert.That( kcvm.Type.Description == "Permet de simuler la pression sur une touche sépciale comme Entrée, les touches F1..12, Effacer, Suppr etc..." );
            Assert.That( kcvm.Type.InnerName == "sendKey" );
            Assert.That( kcvm.Type.Name == "Touche spéciale (F11, Entrée, Suppr ...)" );
            Assert.IsTrue( kcvm.Type.IsValid );

            Assert.That( kcvm.Parameter.GetParameterString() == "Back" );
            Assert.That( kcvm.ToString() == "sendKey:Back" );
        }
    }
}
