using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using CK.Keyboard.Model;
using CK.Windows.Config;

namespace ContextEditor.ViewModels
{
    public class AppViewModel : Conductor<IScreen>
    {
        public WizardManager WizardManager { get; private set; }

        public AppViewModel( ContextEditor ctx )
        {
            DisplayName = "Keyboard edition wizard";

            WizardManager = new WizardManager( this );
            WizardManager.ActivateItem( new HomeViewModel( ctx, WizardManager, ctx.KeyboardContext.Service ) );
            ActivateItem( WizardManager );
        }
    }
}
