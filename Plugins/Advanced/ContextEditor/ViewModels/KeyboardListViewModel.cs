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
    public class KeyboardListViewModel : WizardPage
    {
        public IList<KeyboardViewModel> KeyboardVms { get; set; }
        internal KeyboardViewModel _selectedKeyboard;
        internal IKeyboardEditorRoot _root;
        IKeyboardCollection _keyboards;
        ICommand _selectionCommand;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        /// <param name="model">The keyboard to create or modify</param>
        public KeyboardListViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IKeyboardCollection model )
            : base( wizardManager, false )
        {
            _root = root;
            _keyboards = model;
            KeyboardVms = new List<KeyboardViewModel>();
            foreach( var keyboard in _keyboards )
            {
                if(root.KeyboardContext.Service.CurrentKeyboard != keyboard)
                    KeyboardVms.Add( new KeyboardViewModel( keyboard ) );
            }

            HideNext = true;
            Title = R.KeyboardListStepTitle;
            Description = R.KeyboardListStepDesc;
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
                    if( _selectedKeyboard != null )
                        _selectedKeyboard.IsSelected = false;

                    //The clicked keyboard is now the selected one.
                    k.IsSelected = true;
                    _selectedKeyboard = k;

                    OnKeyboardSelected( k );
                   
                } );

                return _selectionCommand;
            }
        }

        public virtual void OnKeyboardSelected( KeyboardViewModel selectedKeyboardViewModel )
        {
        }
    }
}
