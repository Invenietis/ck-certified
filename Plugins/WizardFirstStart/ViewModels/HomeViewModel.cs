using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WPF.Wizard;

namespace CK.Plugins.WizardFirstStart.ViewModels
{
    class HomeViewModel : WizardPage
    {
        private readonly WizardManager _wizardManager;

        public HomeViewModel( WizardManager wizardManager )
            : base( wizardManager, isLastStep: false )
        {
            _wizardManager = wizardManager;
            
        }
    }
}
