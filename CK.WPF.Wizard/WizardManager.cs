using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using CK.Windows;

namespace CK.WPF.Wizard
{
    /// <summary>
    /// This specialization of the StackConductor implements the "GoFurther" (Go to the next view) and "Restart" actions.
    /// </summary>
    public class WizardManager : StackConductor<WizardPage>
    {
        ViewAware _parent;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="parent">The ViewAware that is the root of the Wizard</param>
        public WizardManager( ViewAware parent )
        {
            _parent = parent;
        }

        /// <summary>
        /// Gets the Next item of the currently active item
        /// </summary>
        public WizardPage Next
        {
            get { return ActiveItem.Next; }
        }

        public override bool OnBeforeGoBack()
        {
            return ActiveItem.OnBeforeGoBack();
        }

        /// <summary>
        /// Activates the next view
        /// </summary>
        public void GoFurther()
        {
            if( ActiveItem.CheckCanGoFurther() )
            {
                if( ActiveItem.OnBeforeNext() && Next != null )
                {
                    ActivateItem( Next );
                    return;
                }
            }
        }

        /// <summary>
        /// Closes the hosting window
        /// </summary>
        public void Close()
        {
            Window w = _parent.GetView() as Window;
            if( w != null ) w.Close();
        }

        SimpleCommand _goFurtherCommand;
        SimpleCommand _closeCommand;
        /// <summary>
        /// Triggers <see cref="GoFurther()", which activates the next view/>
        /// </summary>
        public ICommand GoFurtherCommand
        {
            get
            {
                if( _goFurtherCommand == null ) _goFurtherCommand = new SimpleCommand( GoFurther );
                return _goFurtherCommand;
            }
        }

        /// <summary>
        /// Triggers <see cref="Close()", which closes the hosting window/>
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                if( _closeCommand == null ) _closeCommand = new SimpleCommand( Close );
                return _closeCommand;
            }
        }

        /// <summary>
        /// Resets the stack to start again at the beginning of the wizard.
        /// WizardPages are not re-created.
        /// </summary>
        public void Restart()
        {
            GoBackToRoot();
        }
    }
}
