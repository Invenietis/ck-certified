using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using CK.Keyboard.Model;
using CK.Storage;
using CK.Windows;
using CK.Windows.Config;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public class SavingStepViewModel : HelpAwareWizardPage
    {
        /// <summary>
        /// Gets the list of <see cref="WizardButtonViewModel"/> of this <see cref="WizardPage"/>
        /// </summary>
        public IList<WizardButtonViewModel> Buttons { get; private set; }
        SimpleCommand<WizardButtonViewModel> _command;
        WizardButtonViewModel _selected;
        IKeyboard _keyboardToSave;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="root">The ContextEditor that is the root of this wizard</param>
        /// <param name="wizardManager">The wizardManager</param>
        /// <param name="keyboard">The modified keyboard to save</param>
        public SavingStepViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IKeyboard keyboard )
            : base( root, wizardManager, false )
        {
            _keyboardToSave = keyboard;

            Buttons = new List<WizardButtonViewModel>();
            HideNext = true;
            Buttons.Add( new WizardButtonViewModel( R.SavingStepSaveTitle, R.SavingStepSaveDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/save-keyboard.png", SaveOnEditedKeyboard ) );
            Buttons.Add( new WizardButtonViewModel( R.SavingStepSaveAsTitle, R.SavingStepSaveAsDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/save-as.png", SaveUnderOtherKeyboard ) );
            Buttons.Add( new WizardButtonViewModel( R.SavingStepCancelAndQuitTitle, R.SavingStepCancelAndQuitDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/cancel-quit.png", CancelAndQuit ) );
            Buttons.Add( new WizardButtonViewModel( R.SavingStepCancelAndRestartTitle, R.SavingStepCancelAndRestartDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/cancel-restart.png", CancelAndRestart ) );

            Title = R.SavingStepTitle;
            Description = String.Format( R.SavingStepDesc, keyboard.Name );
        }

        /// <summary>
        /// Gets the command called when the user clicks on a WizardButtonViewModel
        /// </summary>
        public SimpleCommand<WizardButtonViewModel> ButtonCommand
        {
            get
            {
                if( _command == null ) _command = new SimpleCommand<WizardButtonViewModel>( ( k ) =>
                {
                    if( _selected != null )
                        _selected.IsSelected = false;

                    _selected = k;
                    k.IsSelected = true;
                    NotifyOfPropertyChange( () => CanGoFurther );
                } );
                return _command;
            }
        }

        /// <summary>
        /// Goes to the next page.
        /// Not doing anything means keeping the modifications done on the keyboard on which they have been done.
        /// </summary>
        private void SaveOnEditedKeyboard()
        {
            Next = new EndingStepViewModel( Root, WizardManager );
            WizardManager.GoFurther();
        }

        /// <summary>
        /// Saves the modifications done on another keyboard and restores the current one.
        /// Note that in reality, the current keyboard holds the modifications, so we will rename it, 
        /// and restore the backup of the previous state of the keyboard on a new keyboard that we'll create with the name it previously had.
        /// </summary>
        private void SaveUnderOtherKeyboard()
        {
            Next = new SaveAsStepViewModel( Root, WizardManager, _keyboardToSave );
            WizardManager.GoFurther();
        }

        private void CancelAndQuit()
        {
            Root.CancelModifications();
            WizardManager.Close();
        }

        private void CancelAndRestart()
        {
            Root.CancelModifications();
            WizardManager.Restart();
        }

        /// <summary>
        /// Checks that a keyboard has been selected before enabling going to the next step.
        /// </summary>
        /// <returns>true if a keyboard has been selected, false otherwise</returns>
        public override bool CheckCanGoFurther()
        {
            return Buttons.Any( ( b ) => b.IsSelected );
        }
    }
}
