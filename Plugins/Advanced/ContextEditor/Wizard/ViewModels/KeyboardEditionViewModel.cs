using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Keyboard.Model;
using ContextEditor.Resources;

namespace ContextEditor.ViewModels
{
    public class KeyboardEditionViewModel : WizardPage
    {
        public IKeyboard EditedKeyboard { get; set; }
        IKeyboardEditorRoot _root;

        public KeyboardEditionViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IKeyboard editedKeyboard )
            : base( wizardManager, false )
        {
            _root = root;
            EditedKeyboard = editedKeyboard;
            Next = new SavingStepViewModel(_root, WizardManager, EditedKeyboard );

            Title = String.Format( R.KeyboardEditionStepTitle, editedKeyboard.Name );
            Description = R.KeyboardEditionStepDesc;
        }
    }
}
