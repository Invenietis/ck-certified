using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Keyboard.Model;
using KeyboardEditor.Resources;
using CK.WPF.ViewModel;

namespace KeyboardEditor.ViewModels
{
    public class KeyboardEditionViewModel : HelpAwareWizardPage
    {
        public VMContextEditable EditedContext { get { return Root.EditedContext; } }

        public KeyboardEditionViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IKeyboard editedKeyboard )
            : base( root, wizardManager, false )
        {
            Root.EditedContext = new VMContextEditable( root, editedKeyboard, Root.Config, root.SkinConfiguration );
            Next = new SavingStepViewModel( Root, WizardManager, EditedContext.KeyboardVM.Model );

            Title = String.Format( R.KeyboardEditionStepTitle, editedKeyboard.Name );
            Description = R.KeyboardEditionStepDesc;
        }

        public override bool OnBeforeNext()
        {
            return base.OnBeforeNext();
        }

        public override bool OnBeforeGoBack()
        {
            return base.OnBeforeGoBack();
        }

        object _selectedHolder;
        // Used by the binding
        public object SelectedHolder
        {
            get { return _selectedHolder; }
            set
            {
                _selectedHolder = value;
                Refresh();
            }
        }

        // Used to find the config
        internal IKeyboardElement ConfigHolder
        {
            get
            {
                string holderType = _selectedHolder.GetType().ToString();
                switch( holderType )
                {
                    case "VMKeyboardEditable":
                        return ( _selectedHolder as VMKeyboardEditable ).Model;
                    case "VMZoneEditable":
                        return ( _selectedHolder as VMZoneEditable ).Model;
                    case "VMKeyEditable":
                        return ( _selectedHolder as VMKeyEditable ).Model;
                    default:
                        break;
                }
                return null;
            }
        }
    }
}
