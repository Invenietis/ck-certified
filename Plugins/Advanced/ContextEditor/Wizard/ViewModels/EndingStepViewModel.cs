using System.Collections.Generic;
using System.Linq;
using CK.Windows;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public class EndingStepViewModel : HelpAwareWizardPage
    {
        public IList<WizardButtonViewModel> Buttons { get; set; }

        public override bool CheckCanGoFurther()
        {
            return Buttons.Any( ( b ) => b.IsSelected );
        }

        public EndingStepViewModel( IKeyboardEditorRoot root, WizardManager wizardManager )
            : base( root, wizardManager, true )
        {
            Buttons = new List<WizardButtonViewModel>();
            HideNext = true;
            HideBack = true;

            Buttons.Add( new WizardButtonViewModel( R.Quit, R.EndingStepQuitDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/exit.png", CloseWizard ) );
            Buttons.Add( new WizardButtonViewModel( R.StartOver, R.EndingStepStartOverDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/restart.png", RestartWizard ) );

            Title = R.EndingStepTitle;
            Description = R.EndingStepDesc;
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
            Root.EnsureBackupIsClean();
            WizardManager.Close();
        }

        public void RestartWizard()
        {
            Root.EnsureBackupIsClean();
            WizardManager.Restart();
        }
    }
}
