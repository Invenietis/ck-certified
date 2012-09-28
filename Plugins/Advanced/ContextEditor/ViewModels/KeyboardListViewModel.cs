using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CK.Keyboard.Model;
using CK.Windows;

namespace ContextEditor.ViewModels
{
    public class KeyboardListViewModel : WizardPage
    {
        IKeyboardCollection _keyboards;
        public IList<KeyboardViewModel> KeyboardVms { get; set; }
        ContextEditor _root;

        KeyboardViewModel _selectedKeyboard;
        ICommand _selectionCommand;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        /// <param name="model">The keyboard to create or modify</param>
        public KeyboardListViewModel( ContextEditor root, WizardManager wizardManager, IKeyboardCollection model )
            : base( wizardManager, false )
        {
            _root = root;
            _keyboards = model;
            KeyboardVms = new List<KeyboardViewModel>();
            foreach( var keyboard in _keyboards )
            {
                KeyboardVms.Add( new KeyboardViewModel( this, keyboard ) );
            }
            HideNext = true;
        }

        public override bool CheckCanGoFurther()
        {
            return Next != null && _selectedKeyboard != null;
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

                    //We update the Next property to give it the proper model.
                    Next = new KeyboardProfileViewModel(_root, WizardManager, _selectedKeyboard.Keyboard );

                    CantGoFurther = false;
                    NotifyOfPropertyChange( () => IsLastStep );
                    NotifyOfPropertyChange( () => CanGoFurther );

                    WizardManager.GoFurther();
                } );

                return _selectionCommand;
            }
        }
    }
}
