using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CK.Keyboard.Model;
using CK.Windows;
using ContextEditor.Resources;

namespace ContextEditor.ViewModels
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
            Next = new KeyboardProfileViewModel( _root, WizardManager, _selectedKeyboard.Keyboard );

            NotifyOfPropertyChange( () => IsLastStep );
            NotifyOfPropertyChange( () => CanGoFurther );

            WizardManager.GoFurther();
        }
    }
}
