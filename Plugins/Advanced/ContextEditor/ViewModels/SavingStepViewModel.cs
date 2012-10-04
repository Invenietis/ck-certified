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
using ContextEditor.Resources;

namespace ContextEditor.ViewModels
{
    public class SavingStepViewModel : WizardPage
    {
        public IList<WizardButtonViewModel> Buttons { get; set; }
        IKeyboard _keyboardToSave;
        WizardButtonViewModel _selected;
        ContextEditor _root;

        public SavingStepViewModel( ContextEditor root, WizardManager wizardManager, IKeyboard keyboard )
            : base( wizardManager, false )
        {
            _root = root;
            _keyboardToSave = keyboard;

            Buttons = new List<WizardButtonViewModel>();
            HideNext = true;
            Buttons.Add( new WizardButtonViewModel( R.SavingStepSaveTitle, R.SavingStepSaveDesc, "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", SaveOnEditedKeyboard ) );
            Buttons.Add( new WizardButtonViewModel( R.SavingStepSaveAsTitle, R.SavingStepSaveAsDesc, "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", SaveUnderOtherKeyboard ) );
            Buttons.Add( new WizardButtonViewModel( R.SavingStepCancelAndQuitTitle, R.SavingStepCancelAndQuitDesc, "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", CancelAndQuit ) );
            Buttons.Add( new WizardButtonViewModel( R.SavingStepCancelAndRestartTitle, R.SavingStepCancelAndRestartDesc, "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", CancelAndRestart ) );
        }

        SimpleCommand<WizardButtonViewModel> _command;
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
            Next = new EndingStepViewModel( _root, WizardManager );
            WizardManager.GoFurther();
        }

        /// <summary>
        /// Saves the modifications done on another keyboard and restores the current one.
        /// Note that in reality, the current keyboard holds the modifications, so we will rename it, 
        /// and restore the backup of the previous state of the keyboard on a new keyboard that we'll create with the name it previously had.
        /// </summary>
        private void SaveUnderOtherKeyboard()
        {
            Next = new SaveAsStepViewModel( _root, WizardManager, _keyboardToSave );
            WizardManager.GoFurther();
        }

        private void CancelAndQuit()
        {
            _root.CancelModifications();
            WizardManager.Close();
        }

        private void CancelAndRestart()
        {
            _root.CancelModifications();
            WizardManager.Restart();
        }

        public override bool CheckCanGoFurther()
        {
            return Buttons.Any( ( b ) => b.IsSelected );
        }
    }
}
