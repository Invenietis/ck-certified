using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CK.Keyboard.Model;
using CK.Windows.App;
using CK.WPF.Controls;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public class SaveAsStepViewModel : HelpAwareWizardPage
    {
        /// <summary>
        /// Gets the list of keyboards of the current context, wrapped in KeyboardViewModels.
        /// </summary>
        public IList<KeyboardViewModel> KeyboardVms { get; private set; }
        KeyboardViewModel _selectedKeyboard;
        IEnumerable<IKeyboard> _keyboards;
        ICommand _selectionCommand;
        IKeyboard _editedKeyboard;
        string _newName;

        /// <summary>
        /// Gets or sets the name of the keyboard on which the current keyboard should be saved.
        /// </summary>
        public string NewName
        {
            get { return _newName; }
            set
            {
                _newName = value;
                NotifyOfPropertyChange( () => NewName );
                NotifyOfPropertyChange( () => CanGoFurther );
                if( _selectedKeyboard != null && _selectedKeyboard.Keyboard.Name != value )
                {
                    _selectedKeyboard.IsSelected = false;
                    _selectedKeyboard = null;
                }

                KeyboardViewModel k = KeyboardVms.Where( ( kvm ) => kvm.Keyboard.Name == value ).SingleOrDefault();
                if( k != null )
                {
                    _selectedKeyboard = k;
                    _selectedKeyboard.IsSelected = true;
                }
            }
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        /// <param name="model">The keyboard to save</param>
        public SaveAsStepViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IKeyboard editedKeyboard )
            : base( root, wizardManager, false )
        {
            _editedKeyboard = editedKeyboard;
            _keyboards = Root.KeyboardContext.Service.Keyboards.Except( Root.KeyboardContext.Service.Keyboards.Where( ( k ) => k.Name == editedKeyboard.Name ) );
            KeyboardVms = new List<KeyboardViewModel>();
            foreach( var keyboard in _keyboards )
            {
                KeyboardVms.Add( new KeyboardViewModel( keyboard ) );
            }

            Title = R.SaveAsStepTitle;
            Description = R.SaveAsStepDesc;

            Next = new EndingStepViewModel( root, wizardManager );
        }

        /// <summary>
        /// Gets the command called when a keyboard is selected
        /// </summary>
        public ICommand SelectionCommand
        {
            get
            {
                if( _selectionCommand == null ) _selectionCommand = new SimpleCommand<KeyboardViewModel>( ( k ) =>
                {
                    KeyboardViewModel keyboardVm = KeyboardVms.Single( ( vm ) => k == vm );
                    if( _selectedKeyboard != null )
                        _selectedKeyboard.IsSelected = false;

                    //The clicked keyboard is now the selected one.
                    keyboardVm.IsSelected = true;
                    _selectedKeyboard = keyboardVm;

                    NewName = keyboardVm.Keyboard.Name;

                    CantGoFurther = false;
                    NotifyOfPropertyChange( () => NewName );
                    NotifyOfPropertyChange( () => IsLastStep );
                    NotifyOfPropertyChange( () => CanGoFurther );
                } );

                return _selectionCommand;
            }
        }

        /// <summary>
        /// Called before going to the next page.
        /// Handles saving/erasing/replacing/renaming keyboards
        /// </summary>
        /// <returns>true if everything went well, false otherwise</returns>
        public override bool OnBeforeNext()
        {
            if( _selectedKeyboard != null )
            {
                if( !HandleKeyboardSelected() ) return false;
            }
            else
            {
                if( !HandleNewName() ) return false;
            }

            //Flush the temporary file
            Root.EnsureBackupIsClean();

            //Save
            Root.Save();

            return true;
        }

        /// <summary>
        /// Handles cases when the user has selected an existing keyboard on which the edited keyboard has to be applied.
        /// </summary>
        /// <returns>false if execution should be stopped, true otherwise</returns>
        private bool HandleKeyboardSelected()
        {
            string keyboardName = _selectedKeyboard.Keyboard.Name;
            ModalViewModel mvm = new ModalViewModel( R.SaveAsStepPopInTitle, String.Format( R.SaveAsStepPopInDesc, keyboardName ) );
            mvm.Buttons.Add( new ModalButton( mvm, R.Yes, ModalResult.Yes ) );
            mvm.Buttons.Add( new ModalButton( mvm, R.No, ModalResult.No ) );
            CustomMsgBox msgBox = new CustomMsgBox( ref mvm );
            msgBox.ShowDialog();

            //If the user clicks on the close button or on "No", cancel going further.
            if( mvm.ModalResult != ModalResult.Yes ) return false;

            //Otherwise

            if( Root.KeyboardBackup.IsNew ) //if we created a new keyboard, we can destroy the selected, and rename the new with the right name.
            {
                _selectedKeyboard.Keyboard.Destroy();
                _editedKeyboard.Rename( keyboardName );
            }
            else //otherwise, we rename the selected keyboard, we give its name to the edited one, then we apply the backup to the seleted keyboard.
            {
                _selectedKeyboard.Keyboard.Rename( "TemporaryName" );
                _editedKeyboard.Rename( keyboardName );

                Root.KeyboardBackup = new KeyboardBackup( _selectedKeyboard.Keyboard, Root.KeyboardBackup.BackUpFilePath );
                Root.CancelModifications();
            }
            return true;
        }

        /// <summary>
        /// Handles cases when no keyboards are selected, that the user has chosen a arbitrary name, inserted in the NewName property.
        /// </summary>
        /// <returns>false if execution should be stopped, true otherwise</returns>
        private bool HandleNewName()
        {
            //Note : the different cases could have been squashed, yet they are not in order to keep the algorithm clean.

            //The user typed the name of the keyboard it is editing. We have nothing to do.
            if( _editedKeyboard.Name == NewName )
            {
                return true;
            }
            else if( Root.KeyboardBackup.Name == NewName ) //The user wants to save this new keyboard under the name from which it has started. So we just rename it, we don't cancel the modifications
            {
                _editedKeyboard.Rename( NewName );
            }
            else if( Root.KeyboardBackup.IsNew ) //If saving a new keyboard under another unused name, just rename the new keyboard.
                _editedKeyboard.Rename( NewName );
            else //if we started from another keyboard, we must rename this modified one, and recreate another one, on which the backup will be applied
            {
                _editedKeyboard.Rename( NewName );
                IKeyboard rootKeyboard = Root.KeyboardContext.Service.Keyboards.Create( Guid.NewGuid().ToString() );

                Root.KeyboardBackup = new KeyboardBackup( rootKeyboard, Root.KeyboardBackup.BackUpFilePath );
                Root.CancelModifications();
            }
            return true;
        }

        /// <summary>
        /// Checks that the user can go on the next page
        /// </summary>
        /// <returns>true if <see cref="Next"/> is not null and <see cref="NewName"/> has a value</returns>
        public override bool CheckCanGoFurther()
        {
            return Next != null && !String.IsNullOrWhiteSpace( NewName );
        }
    }
}
