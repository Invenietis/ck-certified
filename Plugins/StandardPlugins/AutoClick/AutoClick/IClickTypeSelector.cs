using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.StandardPlugins.AutoClick.Model;
using CK.StandardPlugins.AutoClick.ViewModel;

namespace CK.StandardPlugins.AutoClick
{
    interface IClickTypeSelector
    {
        #region Methods
        
        void AskClickType();

        #endregion

        #region Events

        /// <summary>
        /// Fired when a ClickType is chosen
        /// </summary>
        event ClickTypeChosenEventHandler AutoClickClickTypeChosen;

        /// <summary>
        /// Fired when the "NoClick" clickType is selected
        /// </summary>
        event AutoClickStopEventHandler AutoClickStopEvent;

        /// <summary>
        /// Fired when the "NoClick" clickType is no longer selected
        /// </summary>
        event AutoClickResumeEventHandler AutoClickResumeEvent;

        #endregion
    }

    #region Delegates

    public delegate void ClickTypeChosenEventHandler(object sender, ClickTypeEventArgs e);
    public delegate void AutoClickStopEventHandler(object sender);
    public delegate void AutoClickResumeEventHandler(object sender);

    #endregion

    #region EventArgs

    /// <summary>
    /// EventArgs that contains the click chosen by the ClickTypeSelector.
    /// </summary>
    public class ClickTypeEventArgs : EventArgs
    {
        public ClickVM ClickVM { get; set; }

        public ClickTypeEventArgs( ClickVM clickVM )
        {
            ClickVM = clickVM;            
        }
    }

    #endregion
}
