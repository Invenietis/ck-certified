using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using CK.Windows;

namespace ContextEditor
{
    /// <summary>
    /// This class is used to fill the <see cref="WizardManager"/>'s stack.
    /// It has all the properties needed by the <see cref="WizardManager"/> to display a wizard with next and back actions.
    /// </summary>
    public abstract class WizardPage : PropertyChangedBase
    {
        bool _cantGoFurther;

        /// <summary>
        /// Gets the title of the <see cref="WizardPage"/>
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Gets the description of the <see cref="WizardPage"/>
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the WizardManager that holds this step (this WizardPage).
        /// </summary>
        public WizardManager WizardManager;

        /// <summary>
        /// The next wizard page, which will be activate when the user clicks on next.
        /// </summary>
        public WizardPage Next { get; set; }

        /// <summary>
        /// Set to true if you want to hide the "Next" button of this wizard page.
        /// </summary>
        public bool HideNext { get; set; }

        /// <summary>
        /// Set to true if you want to hide the "Back" button of this wizard page.
        /// </summary>
        public bool HideBack { get; set; }

        /// <summary>
        /// Set to true if this is th elast step of the wizard. 
        /// When set to true, the "Next" button now shows "Close"
        /// </summary>
        public bool IsLastStep { get; set; }

        /// <summary>
        /// Boolean used to specify that the model is not ready to go to the next step.
        /// Can be used by the interface to warn the user that he must do a certain action before going further.
        /// </summary>
        public bool CantGoFurther
        {
            get { return _cantGoFurther; }
            set { _cantGoFurther = value; NotifyOfPropertyChange( () => CantGoFurther ); }
        }

        private WizardPage( WizardManager wizardManager, WizardPage next, bool isLastStep, string title = "" )
        {
            WizardManager = wizardManager;
            IsLastStep = isLastStep;
            Next = next;
            Title = title;
        }

        /// <summary>
        /// This constructor specifies a next wizard.
        /// Its "IsLastStep" Property is autmatically set to false.
        /// </summary>
        /// <param name="wizardManager">The WizardManager</param>
        /// <param name="next">The next WizardPage</param>
        /// <param name="title">(optional) title of the page</param>
        public WizardPage( WizardManager wizardManager, WizardPage next, string title = "" )
            : this( wizardManager, next, false )
        {
        }

        /// <summary>
        /// This constructor doesn't specify a next wizard page (you can do it directly in the WizardPage).
        /// You should therefor specify if it is the last step of the wizard or not.
        /// </summary>
        /// <param name="wizardManager">The WizardManager</param>
        /// <param name="isLastStep">Whether this step is the last of the wizard</param>
        /// <param name="title">(optional) title of the page</param>
        public WizardPage( WizardManager wizardManager, bool isLastStep, string title = "" )
            : this( wizardManager, null, isLastStep, title )
        {
        }

        /// <summary>
        /// Gets whether the user can go on the next Wizard page
        /// </summary>
        public bool CanGoFurther { get { return CheckCanGoFurther(); } }

        /// <summary>
        /// Gets whether the user can go on the previous Wizard page
        /// </summary>
        public bool CanGoBack { get { return CheckCanGoBack(); } }

        /// <summary>
        /// Is called before going to the next step of the wizard.
        /// Can be overridden to implement the test corresponding to the situation.
        /// </summary>
        /// <returns>True if the user can be shown the next step</returns>
        public virtual bool CheckCanGoFurther()
        {
            return true;
        }

        /// <summary>
        /// Is called before going to the previous step of the wizard.
        /// Can be overridden to implement the test corresponding to the situation.
        /// </summary>
        /// <returns>True if the user can go back to the previous step</returns>
        public virtual bool CheckCanGoBack()
        {
            return true;
        }

        /// <summary>
        /// Is called right before going ot the next step, but after checking CanGoFurther.
        /// Can be overridden to do something before changing the ActiveItem.
        /// Return false to cancel going ot the next WizardPage
        /// </summary>
        /// <returns>true if the manager should go on changing the view, false otherwise</returns>
        public virtual bool OnBeforeNext()
        {
            return true;
        }

        /// <summary>
        /// Is called right before going ot the previous step, but after checking CanGoBack.
        /// Can be overridden to do something before changing the ActiveItem.
        /// Return false to cancel going ot the previous WizardPage 
        /// </summary>
        /// <returns>true if the manager should go on changing the view, false otherwise</returns>
        public virtual bool OnBeforeGoBack()
        {
            return true;
        }
    }
}
