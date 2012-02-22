using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugins.AutoClick.ViewModel;
using CK.Plugins.AutoClick.Model;
using CK.Plugins.AutoClick.Views;
using System.Windows.Input;
using CK.WPF.ViewModel;
using System.Diagnostics;
using System.ComponentModel;

namespace CK.Plugins.AutoClick
{
    public class StdClickTypeSelector : CK.WPF.ViewModel.VMBase, IClickTypeSelector
    {

        #region Variables & Properties

        private ICommand _incrementTimeBeforeCountDownStartsCommand;
        public ICommand IncrementTimeBeforeCountDownStartsCommand
        {
            get
            {
                if( _incrementTimeBeforeCountDownStartsCommand == null )
                {
                    _incrementTimeBeforeCountDownStartsCommand = new VMCommand( () => _holder.ModifyCountDownConfiguration( "TimeBeforeCountDownStarts", 100 ) );
                }
                return _incrementTimeBeforeCountDownStartsCommand;
            }
        }

        private ICommand _decrementTimeBeforeCountDownStartsCommand;
        public ICommand DecrementTimeBeforeCountDownStartsCommand
        {
            get
            {
                if( _decrementTimeBeforeCountDownStartsCommand == null )
                {
                    _decrementTimeBeforeCountDownStartsCommand = new VMCommand( () => _holder.ModifyCountDownConfiguration( "TimeBeforeCountDownStarts", -100 ) );
                }
                return _decrementTimeBeforeCountDownStartsCommand;
            }
        }

        private ICommand _incrementCountDownDurationCommand;
        public ICommand IncrementCountDownDurationCommand
        {
            get
            {
                if( _incrementCountDownDurationCommand == null )
                {
                    _incrementCountDownDurationCommand = new VMCommand( () => _holder.ModifyCountDownConfiguration( "CountDownDuration", 100 ) );
                }
                return _incrementCountDownDurationCommand;
            }
        }

        private ICommand _decrementCountDownDurationCommand;
        public ICommand DecrementCountDownDurationCommand
        {
            get
            {
                if( _decrementCountDownDurationCommand == null )
                {
                    _decrementCountDownDurationCommand = new VMCommand( () => _holder.ModifyCountDownConfiguration( "CountDownDuration", -100 ) );
                }
                return _decrementCountDownDurationCommand;
            }
        }

        public int TimeBeforeCountDownStarts { get { return _holder.MouseWatcher.Service.TimeBeforeCountDownStarts; } }
        public int CountDownDuration { get { return _holder.MouseWatcher.Service.CountDownDuration; } }

        private AutoClick _holder;

        //ViewModel
        private ClicksVM _clicksVM;
        public ClicksVM ClicksVM
        {
            get { return _clicksVM; }
            set { ClicksVM = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// StandardClickTypeSelector Constructor
        /// </summary>
        public StdClickTypeSelector( AutoClick holder )
            : this( )
        {
            _holder = holder;
        }

        /// <summary>
        /// StandardClickTypeSelector Constructor
        /// </summary>
        /// <param name="clicks"></param>
        public StdClickTypeSelector()
        {
            _clicksVM = new ClicksVM();
        }

        #endregion

        #region IClickTypeSelector Members

        #region Methods

        /// <summary>
        /// Part of the IClickTypeSelector Interface, called by the AutoClickPlugin when it needs to launch a click.
        /// This method doesn't return the Click as it may need to be asyncronous (when the PointerClickTypeSelector is enabled)
        /// </summary>
        public void AskClickType()
        {
            ClickVM clickToLaunch = null;

            clickToLaunch = _clicksVM.GetNextClick( true );

            if( AutoClickClickTypeChosen != null && clickToLaunch != null )
            {
                AutoClickClickTypeChosen( this, new ClickTypeEventArgs( clickToLaunch ) );                
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event fired when a click has been chosen
        /// </summary>
        public event ClickTypeChosenEventHandler AutoClickClickTypeChosen;

        /// <summary>
        /// Event fired when the AutoClickPlugin should be stopped
        /// </summary>
        public event AutoClickStopEventHandler AutoClickStopEvent;

        /// <summary>
        /// Event fired when the AutoclickPlugin may go back to work
        /// </summary>
        public event AutoClickResumeEventHandler AutoClickResumeEvent;

        #endregion

        #endregion
    }
}
