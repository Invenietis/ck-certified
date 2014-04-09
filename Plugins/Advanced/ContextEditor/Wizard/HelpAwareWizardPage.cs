using System.Windows.Input;
using Caliburn.Micro;
using CK.WPF.Wizard;

namespace KeyboardEditor
{
    public abstract class HelpAwareWizardPage : WizardPage
    {
        internal IKeyboardEditorRoot Root { get; private set; }

        public HelpAwareWizardPage( IKeyboardEditorRoot root, WizardManager wizardManager, WizardPage next, bool isLastStep, string title = "" )
            : base( wizardManager, next, title )
        {
            Root = root;
        }

        public HelpAwareWizardPage( IKeyboardEditorRoot root, WizardManager wizardManager, WizardPage next, string title = "" )
            : this( root, wizardManager, next, false, title )
        {
        }

        public HelpAwareWizardPage( IKeyboardEditorRoot root, WizardManager wizardManager, bool isLastStep, string title = "" )
            : this( root, wizardManager, null, isLastStep, title )
        {
        }

        ICommand _showHelpCommand;
        public ICommand ShowHelpCommand
        {
            get
            {
                if( _showHelpCommand == null )
                {
                    _showHelpCommand = new CK.Windows.App.VMCommand( () => Root.ShowHelp() );
                }
                return _showHelpCommand;
            }
        }
    }
}
