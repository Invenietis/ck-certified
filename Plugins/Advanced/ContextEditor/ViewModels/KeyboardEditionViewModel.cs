using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Keyboard.Model;

namespace ContextEditor.ViewModels
{
    public class KeyboardEditionViewModel : WizardPage
    {
        public IKeyboard EditedKeyboard { get; set; }
        ContextEditor _root;

        public KeyboardEditionViewModel( ContextEditor root, WizardManager wizardManager, IKeyboard editedKeyboard )
            : base( wizardManager, false )
        {
            _root = root;
            EditedKeyboard = editedKeyboard;
            Next = new SavingStepViewModel(_root, WizardManager, EditedKeyboard );
        }
    }
}
