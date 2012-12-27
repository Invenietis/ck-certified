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
    public class DeletionKeyboardListViewModel : KeyboardListViewModel
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        /// <param name="model">The keyboard to create or modify</param>
        public DeletionKeyboardListViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IKeyboardCollection model )
            : base( root, wizardManager, model )
        {
            HideNext = false;
            Title = R.KeyboardDeletionListStepTitle;
            Description = R.KeyboardDeletionListStepDesc;
        }

        public override void OnKeyboardSelected( KeyboardViewModel selectedKeyboardViewModel )
        {
            Next = new EndingStepViewModel( _root, WizardManager );

            NotifyOfPropertyChange( () => IsLastStep );
            NotifyOfPropertyChange( () => CanGoFurther );

            
        }

        public override bool OnBeforeNext()
        {
            _selectedKeyboard.Keyboard.Destroy();
            return base.OnBeforeNext();
        }
    }
}
