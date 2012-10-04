using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using CK.Keyboard.Model;
using CK.Windows;
using CK.Windows.Config;
using ContextEditor.Resources;

namespace ContextEditor.ViewModels
{
    public class HomeViewModel : WizardPage
    {
        SimpleCommand<WizardButtonViewModel> _command;
        WizardButtonViewModel _selected;
        IKeyboardContext _keyboardCtx;
        ContextEditor _root;

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
        public HomeViewModel( ContextEditor root, WizardManager wizardManager, IKeyboardContext keyboardCtx )
            : base( wizardManager, false )
        {
            _root = root;
            _keyboardCtx = keyboardCtx;
            Buttons = new List<WizardButtonViewModel>();

            Buttons.Add( new WizardButtonViewModel( String.Format( R.HomeEditCurrentKeyboard, _keyboardCtx.CurrentKeyboard.Name ), R.HomeEditCurrentKeyboardDesc, "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", EditCurrentKeyboard ) );
            Buttons.Add( new WizardButtonViewModel( R.HomeEditNewKeyboard, R.HomeEditNewKeyboardDesc, "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", CreateNewKeyboard ) );
            Buttons.Add( new WizardButtonViewModel( R.HomeEditOtherKeyboard, R.HomeEditOtherKeyboardDesc, "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", EditOtherKeyboard ) );

            Title = R.HomeStepTitle;
            Description = R.HomeStepDescription;
            HideNext = true;
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
            Next = new KeyboardProfileViewModel( _root, WizardManager );
            WizardManager.GoFurther();
        }

        /// <summary>
        /// Goes to the next page to enable modifying the current keyboard.
        /// </summary>
        public void EditCurrentKeyboard()
        {
            Next = new KeyboardProfileViewModel( _root, WizardManager, _keyboardCtx.CurrentKeyboard );
            WizardManager.GoFurther();
        }

        /// <summary>
        /// Goes to a page listing all the keyboards that can be modified.
        /// </summary>
        public void EditOtherKeyboard()
        {
            Next = new KeyboardListViewModel( _root, WizardManager, _keyboardCtx.Keyboards );
            WizardManager.GoFurther();
        }

        #endregion
    }
}
