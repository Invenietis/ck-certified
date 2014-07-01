﻿using System.Collections.Generic;
using CK.Keyboard.Model;
using CK.WPF.Wizard;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public class DeletionKeyboardListViewModel : KeyboardListViewModel
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        /// <param name="model">The keyboard to create or modify</param>
        public DeletionKeyboardListViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IEnumerable<IKeyboard> model )
            : base( root, wizardManager, model )
        {
            HideNext = false;
            Title = R.KeyboardDeletionListStepTitle;
            Description = R.KeyboardDeletionListStepDesc;
        }

        public override void OnKeyboardSelected( KeyboardViewModel selectedKeyboardViewModel )
        {
            Next = new EndingStepViewModel( Root, WizardManager );

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
