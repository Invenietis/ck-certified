using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CK.Core;
using CK.Keyboard.Model;
using CK.Storage;
using CK.Windows.App;
using CK.WPF.Controls;

namespace ContextEditor.ViewModels
{
    public class SaveAsStepViewModel : WizardPage
    {
        /// <summary>
        /// Gets the list of keyboards of the current context, wrapped in KeyboardViewModels.
        /// </summary>
        public IList<KeyboardViewModel> KeyboardVms { get; private set; }
        KeyboardViewModel _selectedKeyboard;
        IEnumerable<IKeyboard> _keyboards;
        ICommand _selectionCommand;
        IKeyboard _editedKeyboard;
        ContextEditor _root;
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
        public SaveAsStepViewModel( ContextEditor root, WizardManager wizardManager, IKeyboard editedKeyboard )
            : base( wizardManager, false )
        {
            _root = root;
            _editedKeyboard = editedKeyboard;
            _keyboards = _root.KeyboardContext.Service.Keyboards.Except( _root.KeyboardContext.Service.Keyboards.Where( ( k ) => k.Name == editedKeyboard.Name ) );
            KeyboardVms = new List<KeyboardViewModel>();
            foreach( var keyboard in _keyboards )
            {
                KeyboardVms.Add( new KeyboardViewModel( keyboard ) );
            }

            Next = new EndingStepViewModel( root, wizardManager );
        }

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
            _root.EnsureBackupIsClean();

            return true;
        }

        /// <summary>
        /// Handles cases when the user has selected an existing keyboard on which the edited keyboard has to be applied.
        /// </summary>
        /// <returns>false if execution should be stopped, true otherwise</returns>
        private bool HandleKeyboardSelected()
        {
            string keyboardName = _selectedKeyboard.Keyboard.Name;
            ModalViewModel mvm = new ModalViewModel( "Ecraser le clavier existant", String.Format( "Vous êtes sur le point de sauvegarder vos modifications sur le clavier {0}.{1}Etes-vous sur de vouloir faire cela ?", keyboardName, System.Environment.NewLine ) );
            mvm.Buttons.Add( new ModalButton( mvm, "Oui", ModalResult.Yes ) );
            mvm.Buttons.Add( new ModalButton( mvm, "Non", ModalResult.No ) );
            CustomMsgBox msgBox = new CustomMsgBox( ref mvm );
            msgBox.ShowDialog();

            //If the user clicks on the close button or on "No", cancel going further.
            if( mvm.ModalResult != ModalResult.Yes ) return false;

            //Otherwise

            if( _root.KeyboardBackup.IsNew ) //if we created a new keyboard, we can destroy the selected, and rename the new with the right name.
            {
                _selectedKeyboard.Keyboard.Destroy();
                _editedKeyboard.Rename( keyboardName );
            }
            else //otherwise, we rename the selected keyboard, we give its name to the edited one, then we apply the backup to the seleted keyboard.
            {
                _selectedKeyboard.Keyboard.Rename( "TemporaryName" );
                _editedKeyboard.Rename( keyboardName );

                _root.KeyboardBackup = new KeyboardBackup( _selectedKeyboard.Keyboard, _root.KeyboardBackup.BackUpFilePath );
                _root.CancelModifications();
            }

            return true;
        }



        /// <summary>
        /// Handles cases when no keyboards are selected, that the user has chosen a arbitrary name, inserted in the NewName property.
        /// </summary>
        /// <returns>false if execution should be stopped, true otherwise</returns>
        private bool HandleNewName()
        {
            //IKeyboard k = _root.KeyboardContext.Service.Keyboards[NewName];
            //if( k != null )
            //{
            //    string keyboardName = k.Name;

            //    ModalViewModel mvm = new ModalViewModel( "Un clavier portant ce nom existe déjà", String.Format( "Un clavier portant le nom '{0}' existe déjà.{1} Etes-vous sûr de vouloir l'écraser ?", keyboardName, System.Environment.NewLine ) );
            //    mvm.Buttons.Add( new ModalButton( mvm, "Oui", ModalResult.Yes ) );
            //    mvm.Buttons.Add( new ModalButton( mvm, "Non", ModalResult.No ) );
            //    CustomMsgBox msgBox = new CustomMsgBox( ref mvm );
            //    msgBox.ShowDialog();

            //    if( mvm.ModalResult != ModalResult.Yes ) return false;
            //    if( _root.KeyboardBackup.IsNew )
            //    {
            //        //If we apply a new keyboard on an existing one, we just destroy the existing one, and rename the new one with its name.
            //        k.Destroy();
            //        _editedKeyboard.Rename( keyboardName );
            //    }
            //    else
            //    {
            //        //If we apply an existing keyboard under another existing keyboard, we must apply the backup to the keyboard we want to erase.
            //        // and then we rename the edited keyboard to reflect its new name.

            //        IStructuredSerializable serializableKeyboard = _selectedKeyboard as IStructuredSerializable;
            //        if( serializableKeyboard == null ) throw new CKException( String.Format( "The selected element of the SaveAsStepViewModel should be IStructuredSerializable. Name : {0}", _selectedKeyboard.Keyboard.Name ) );

            //        using( FileStream str = new FileStream( _root.KeyboardBackup.BackUpFilePath, FileMode.Open ) )
            //        {
            //            using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, _root.Context.ServiceContainer ) )
            //            {
            //                //Erasing all properties of the keyboard. We re-apply the backedup ones.
            //                serializableKeyboard.ReadContent( reader );
            //            }
            //        }

            //        _editedKeyboard.Rename( keyboardName );
            //    }
            //    return true;
            //}
            //else
            //{
                if( _root.KeyboardBackup.IsNew ) //If saving a new keyboard under another unused name, just rename the new keyboard.
                    _editedKeyboard.Rename( NewName );
                else //if we started from another keyboard, we must rename this modified one, and recreate another one, on which the backup will be applied
                {
                    _editedKeyboard.Rename( NewName );
                    IKeyboard rootKeyboard = _root.KeyboardContext.Service.Keyboards.Create( Guid.NewGuid().ToString() );

                    _root.KeyboardBackup = new KeyboardBackup( rootKeyboard, _root.KeyboardBackup.BackUpFilePath );
                    _root.CancelModifications();

                    //IStructuredSerializable serializableRootKeyboard = rootKeyboard as IStructuredSerializable;
                    //if( serializableRootKeyboard == null ) throw new CKException( "The created keyboard of the SaveAsStepViewModel should be IStructuredSerializable." );

                    //using( FileStream str = new FileStream( _root.KeyboardBackup.BackUpFilePath, FileMode.Open ) )
                    //{
                    //    using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, _root.Context.ServiceContainer ) )
                    //    {
                    //        //Erasing all properties of the keyboard. We re-apply the backedup ones.
                    //        serializableRootKeyboard.ReadContent( reader );
                    //    }
                    //}
                }
                return true;
            //}
        }



        public override bool CheckCanGoFurther()
        {
            return Next != null && !String.IsNullOrWhiteSpace( NewName );
        }
    }
}
