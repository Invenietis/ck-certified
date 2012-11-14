using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CK.Plugins.WizardFirstStart.Resources;
using CK.WPF.Wizard;

namespace CK.Plugins.WizardFirstStart
{
    public class WizardViewModel : Conductor<IScreen>
    {
        public WizardManager WizardManager { get; private set; }

        public WizardViewModel()
        {
            DisplayName = R.WindowTitle;

            WizardManager = new WizardManager( this );
            WizardManager.ActivateItem( new HomeViewModel( WizardManager ) );
            ActivateItem( WizardManager );
        }
    }
}
