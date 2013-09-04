using KeyboardEditor.KeyboardEdition;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardEditorTests
{
    [TestFixture]
    public class KeyCommandTests
    {
        [Test]
        public void SimpleTest()
        {
            KeyCommandTypeProviderViewModel provider = new KeyCommandTypeProviderViewModel();
            KeyCommandViewModel kcvm = new KeyCommandViewModel( provider, "sendString:Bonjour" );

            Assert.NotNull( provider );
            Assert.That( provider.AvailableTypes.Count() == 2 );

            Assert.NotNull( kcvm );
            Assert.That( kcvm.Type.Description == "Permet d'écrire n'importe quelle chaine de caractère" );
            Assert.That( kcvm.Type.InnerName == "sendString" );
            Assert.That( kcvm.Type.Name == "Ecrire une lettre ou une phrase" );
            Assert.IsTrue( kcvm.Type.IsValid );

            Assert.That( kcvm.Parameter.GetParameterString() == "Bonjour" );
            Assert.That( kcvm.ToString() == "sendString:Bonjour" );

        }
    }
}
