using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using CK.Keyboard.Model;
using CK.Windows;
using CK.Windows.Config;

namespace ContextEditor.ViewModels
{
    public class HomeViewModel : WizardPage
    {
        public IList<WizardButtonViewModel> Buttons { get; set; }
        IKeyboardContext _keyboardCtx;
        WizardButtonViewModel _selected;
        ContextEditor _root;

        public override bool CheckCanGoFurther()
        {
            return Buttons.Any( ( b ) => b.IsSelected );
        }

        public HomeViewModel( ContextEditor root, WizardManager wizardManager, IKeyboardContext keyboardCtx )
            : base( wizardManager, false )
        {
            HideNext = true;
            _root = root;
            _keyboardCtx = keyboardCtx;
            Next = new KeyboardProfileViewModel( _root, WizardManager, _keyboardCtx.CurrentKeyboard );
            Buttons = new List<WizardButtonViewModel>();

            Buttons.Add( new WizardButtonViewModel( "Editer le clavier courant", "Editer simplement et rapidement cotre clavier actif", "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", EditCurrentKeyboard ) );
            Buttons.Add( new WizardButtonViewModel( "Nouveau clavier", "Commencer à créer un nouveau clavier CiviKey", "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", CreateNewKeyboard ) );
            Buttons.Add( new WizardButtonViewModel( "Editer un autre clavier", "Ouvrez un autre contexte et éditez-le simplement", "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", EditOtherKeyboard ) );
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

        public void CreateNewKeyboard()
        {
            Next = new KeyboardProfileViewModel( _root, WizardManager );
            WizardManager.GoFurther();
        }

        public void EditCurrentKeyboard()
        {
            Next = new KeyboardProfileViewModel( _root, WizardManager, _keyboardCtx.CurrentKeyboard );
            WizardManager.GoFurther();
        }

        public void EditOtherKeyboard()
        {
            Next = new KeyboardListViewModel( _root, WizardManager, _keyboardCtx.Keyboards );
            WizardManager.GoFurther();
        }
    }
}
