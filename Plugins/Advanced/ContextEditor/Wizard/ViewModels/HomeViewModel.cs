using System.Collections.Generic;
using System.Linq;
using CK.Keyboard.Model;
using CK.Windows;
using CK.WPF.Wizard;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public class HomeViewModel : HelpAwareWizardPage
    {
        SimpleCommand<WizardButtonViewModel> _command;
        WizardButtonViewModel _selected;
        IKeyboardContext _keyboardCtx;

        /// <summary>
        /// Gets the list of <see cref="WizardButtonViewModel"/> on this <see cref="WizardPage"/>
        /// </summary>
        public IList<WizardButtonViewModel> Buttons { get; private set; }

        /// <summary>
        /// Method that checks the at least one <see cref="WizardButtonViewModel"/> is selected before going ot the next page.
        /// </summary>
        /// <returns></returns>
        public override bool CheckCanGoFurther()
        {
            return Buttons.Any( ( b ) => b.IsSelected ) && Next != null;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="root"></param>
        /// <param name="wizardManager"></param>
        /// <param name="keyboardCtx"></param>
        public HomeViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IKeyboardContext keyboardCtx )
            : base(root, wizardManager, false )
        {
            _keyboardCtx = keyboardCtx;
            Buttons = new List<WizardButtonViewModel>();

            Buttons.Add( new WizardButtonViewModel( R.HomeEditCurrentKeyboard, R.HomeEditCurrentKeyboardDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/edit-keyboard.png", EditCurrentKeyboard ) );
            Buttons.Add( new WizardButtonViewModel( R.HomeEditNewKeyboard, R.HomeEditNewKeyboardDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/create-keyboard.png", CreateNewKeyboard ) );
            Buttons.Add( new WizardButtonViewModel( R.HomeEditOtherKeyboard, R.HomeEditOtherKeyboardDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/edit-other-keyboard.png", EditOtherKeyboard ) );
            Buttons.Add( new WizardButtonViewModel( R.HomeDestroyKeyboardTitle, R.HomeDestroyKeyboardDesc, "pack://application:,,,/KeyboardEditor;component/Resources/Images/destroy-keyboard.png", DestroyKeyboard ) );
            Title = R.HomeStepTitle;
            Description = R.HomeStepDescription;
            HideNext = true;
            HideBack = true;
        }

        /// <summary>
        /// Command that takes a <see cref="WizardButtonViewModel"/> as parameter.
        /// Sets the <see cref="WizardButtonViewModel.IsSelected"/> property of the <see cref="WizardButtonViewModel"/> to true.
        /// The previously selected <see cref="WizardButtonViewModel"/> is no longer selected.
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

        #region Button methods

        /// <summary>
        /// Creates a new keyboard and goes to the next page.
        /// </summary>
        public void CreateNewKeyboard()
        {
            Next = new KeyboardProfileViewModel( Root, WizardManager );
            WizardManager.GoFurther();
        }

        /// <summary>
        /// Goes to the next page to enable modifying the current keyboard.
        /// </summary>
        public void EditCurrentKeyboard()
        {
            var keyboard = _keyboardCtx.CurrentKeyboard;
            
            Next = new KeyboardProfileViewModel( Root, WizardManager, keyboard );
            WizardManager.GoFurther();
        }

        /// <summary>
        /// Goes to a page listing all the keyboards that can be modified.
        /// </summary>
        public void EditOtherKeyboard()
        {
            Next = new ModificationKeyboardListViewModel( Root, WizardManager, _keyboardCtx.Keyboards );
            WizardManager.GoFurther();
        }

        /// <summary>
        /// Goes to a page listing all the keyboards that can be deleted.
        /// </summary>
        public void DestroyKeyboard()
        {
            Next = new DeletionKeyboardListViewModel( Root, WizardManager, _keyboardCtx.Keyboards.Where( k => !k.IsActive ) );
            WizardManager.GoFurther();
        }

        #endregion
    }
}
