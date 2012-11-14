using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CK.Plugins.WizardFirstStart.Resources;
using CK.WPF.Wizard;

namespace CK.Plugins.WizardFirstStart.ViewModels
{
    class WizardViewModel : Conductor<IScreen>
    {
        public WizardViewModel()
        {
            DisplayName = R.WindowTitle;

            WizardManager wizardManager = new WizardManager( this );
            wizardManager.ActivateItem( new HomeViewModel( wizardManager) );
            ActivateItem( wizardManager );
        }
    }
}
