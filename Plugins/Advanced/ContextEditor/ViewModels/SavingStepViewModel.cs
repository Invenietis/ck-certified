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
    public class SavingStepViewModel : WizardPage
    {
        public IList<WizardButtonViewModel> Buttons { get; set; }
        IKeyboard _keyboardToSave;
        WizardButtonViewModel _selected;
        ContextEditor _root;

        public override bool CheckCanGoFurther()
        {
            return Buttons.Any( ( b ) => b.IsSelected );
        }

        public SavingStepViewModel(ContextEditor root, WizardManager wizardManager, IKeyboard keyboard )
            : base( wizardManager, false )
        {
            _root = root;
            _keyboardToSave = keyboard;
            
            Buttons = new List<WizardButtonViewModel>();
            HideNext = true;
            Buttons.Add( new WizardButtonViewModel( "Sauvegarder", "Sauvez vos modification dans le contexte courant", "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", SaveInCurrentContext ) );
            Buttons.Add( new WizardButtonViewModel( "Annuler et quitter", "Annuler toutes les modifications faites sur ce clavier et quitter l'assistant", "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", CancelAndQuit ) );
            Buttons.Add( new WizardButtonViewModel( "Annuler et recommencer", "Annuler toutes les modifications faites sur ce clavier et recommecner au début de l'assistant", "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", CancelAndRestart ) );
            //Buttons.Add( new WizardButtonViewModel( "Enregistrer dans un autre contexte", "Exportez ce clavier vers une autre bibliothèque de claviers", "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", SaveInExistingContext ) );
            //Buttons.Add( new WizardButtonViewModel( "Enregistrer dans un nouveau contexte", "Exportez ce clavier vers une nouvelle bibliothèque de claviers", "pack://application:,,,/ContextEditor;component/Resources/keyboard.png", SaveAsNewContext ) );
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

        public void SaveInCurrentContext()
        {
            Next = new EndingStepViewModel( WizardManager );
            WizardManager.GoFurther();
        }

        public void CancelAndQuit()
        {
            CancelModifications();
            WizardManager.Close();
        }

        public void CancelAndRestart()
        {
            CancelModifications();
            WizardManager.Restart();
        }

        /// <summary>
        /// Retrieves the keyboard's state before any modification and re-apply it.
        /// </summary>
        private void CancelModifications()
        {
            //TODO
        }

        //public void SaveInExistingContext()
        //{
        //    //TODO : Get the XML of the keyboard, set it in the selected context and destroy it from the current context.
        //    Next = new ContextListViewModel( WizardManager, _root.Context.ConfigManager.UserConfiguration.ContextProfiles, _keyboardToSave );
        //    WizardManager.GoFurther();
        //}

        //public void SaveAsNewContext()
        //{
        //    //TODO
        //}
    }
}
