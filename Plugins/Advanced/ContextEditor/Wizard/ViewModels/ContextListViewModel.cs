using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using CK.Windows;

namespace KeyboardEditor.ViewModels
{
    public class ContextListViewModel : HelpAwareWizardPage
    {
        IKeyboard _keyboardtoSave;
        IUriHistoryCollection _contexts;
        public IList<ContextViewModel> ContextVms { get; set; }

        ContextViewModel _selectedContext;
        ICommand _selectionCommand;
        

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        /// <param name="model">The context to which we should save a keyboard</param>
        public ContextListViewModel( KeyboardEditor root, WizardManager wizardManager, IUriHistoryCollection contexts, IKeyboard keyboardToSave )
            : base( root, wizardManager, false )
        {
            _contexts = contexts;
            _keyboardtoSave = keyboardToSave;

            Next = new EndingStepViewModel( Root, WizardManager );

            ContextVms = new List<ContextViewModel>();
            foreach( var context in _contexts )
            {
                ContextVms.Add( new ContextViewModel( this, context ) );
            }
        }

        public override bool CheckCanGoFurther()
        {
            return _selectedContext != null && Next != null;
        }

        public ICommand SelectionCommand
        {
            get
            {
                if( _selectionCommand == null ) _selectionCommand = new SimpleCommand<ContextViewModel>( ( k ) =>
                {
                    ContextViewModel contextVm = ContextVms.Single( ( vm ) => k == vm );
                    if( _selectedContext != null )
                        _selectedContext.IsSelected = false;

                    //The clicked context is now the selected one.
                    contextVm.IsSelected = true;
                    _selectedContext = contextVm;

                    NotifyOfPropertyChange( () => CanGoFurther );
                } );

                return _selectionCommand;
            }
        }

        bool _canSave;
        public bool CanSave
        {
            get { return _canSave; }
            set
            {
                _canSave = value; NotifyOfPropertyChange( () => CanSave );
            }
        }

        ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if( _saveCommand == null )
                {
                    _saveCommand = new SimpleCommand( () =>
                    {
                        Debug.Assert( _canSave, "The save command can't be executed if CanSave is false" );
                        if( _selectedContext == null ) return;

                        //TODO : Add the keyboard to the selected context.
                        WizardManager.GoFurther();
                    } );
                }

                return _saveCommand;
            }
        }

        
    }
}
