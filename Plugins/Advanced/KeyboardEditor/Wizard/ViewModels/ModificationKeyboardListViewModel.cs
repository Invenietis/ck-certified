using CK.Keyboard.Model;
using CK.WPF.Wizard;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public class ModificationKeyboardListViewModel : KeyboardListViewModel
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        /// <param name="model">The keyboard to create or modify</param>
        public ModificationKeyboardListViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IKeyboardCollection model )
            : base( root, wizardManager, model )
        {
            HideNext = true;
            Title = R.KeyboardListStepTitle;
            Description = R.KeyboardListStepDesc;
        }

        public override void OnKeyboardSelected( KeyboardViewModel selectedKeyboardViewModel )
        {
            //We update the Next property to give it the proper model.
            Next = new KeyboardProfileViewModel( Root, WizardManager, _selectedKeyboard.Keyboard );

            NotifyOfPropertyChange( () => IsLastStep );
            NotifyOfPropertyChange( () => CanGoFurther );

            WizardManager.GoFurther();
        }
    }
}
