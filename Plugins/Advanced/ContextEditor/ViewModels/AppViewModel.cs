using Caliburn.Micro;
using ContextEditor.Resources;

namespace ContextEditor.ViewModels
{
    public class AppViewModel : Conductor<IScreen>
    {
        public WizardManager WizardManager { get; private set; }

        public AppViewModel( IKeyboardEditorRoot ctx )
        {
            DisplayName = R.WindowTitle;

            WizardManager = new WizardManager( this );
            WizardManager.ActivateItem( new HomeViewModel( ctx, WizardManager, ctx.KeyboardContext.Service ) );
            ActivateItem( WizardManager );
        }
    }
}
