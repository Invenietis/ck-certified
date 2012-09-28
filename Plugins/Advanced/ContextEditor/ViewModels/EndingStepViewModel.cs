using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using CK.Keyboard.Model;
using CK.Windows;
using CK.Windows.Config;

namespace ContextEditor.ViewModels
{
    public class EndingStepViewModel : WizardPage
    {
        public IList<WizardButtonViewModel> Buttons { get; set; }

        public override bool CheckCanGoFurther()
        {
            return Buttons.Any( ( b ) => b.IsSelected );
        }

        public EndingStepViewModel( WizardManager wizardManager )
            : base( wizardManager, true )
        {
            HideNext = true;
            Buttons = new List<WizardButtonViewModel>();

            Buttons.Add( new WizardButtonViewModel( "Quitter", "Vous pouvez quitter l'assitant si vous avez effectué toutes les modifications nécessaires", "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", CloseWizard ) );
            Buttons.Add( new WizardButtonViewModel( "Recommencer", "Vous pouvez retourner au début de l'assitant pour modifier d'autres claviers", "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", RestartWizard ) );
        }

        SimpleCommand<WizardButtonViewModel> _command;
        public SimpleCommand<WizardButtonViewModel> ButtonCommand
        {
            get
            {
                if( _command == null ) _command = new SimpleCommand<WizardButtonViewModel>( ( k ) =>
                {
                    k.IsSelected = true;
                } );
                return _command;
            }
        }

        public void CloseWizard()
        {
            WizardManager.Close();
        }

        public void RestartWizard()
        {
            WizardManager.Restart();
        }
    }
}
