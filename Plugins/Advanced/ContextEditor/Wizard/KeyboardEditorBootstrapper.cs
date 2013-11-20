using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace KeyboardEditor
{
    internal class KeyboardEditorBootstrapper : BootstrapperBase
    {
        public KeyboardEditorBootstrapper()
            : base( false )
        {
        }

        protected override void OnStartup( object sender, System.Windows.StartupEventArgs e )
        {
           
            base.OnStartup( sender, e );
        }
    }
}
