#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\StdClickTypeSelector.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
using CK.Windows.App;

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
                    _incrementTimeBeforeCountDownStartsCommand = new CK.Windows.App.VMCommand( () => _holder.ModifyCountDownConfiguration( "TimeBeforeCountDownStarts", 100 ) );
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
                    _decrementTimeBeforeCountDownStartsCommand = new CK.Windows.App.VMCommand( () => _holder.ModifyCountDownConfiguration( "TimeBeforeCountDownStarts", -100 ) );
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
                    _incrementCountDownDurationCommand = new CK.Windows.App.VMCommand( () => _holder.ModifyCountDownConfiguration( "CountDownDuration", 100 ) );
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
                    _decrementCountDownDurationCommand = new CK.Windows.App.VMCommand( () => _holder.ModifyCountDownConfiguration( "CountDownDuration", -100 ) );
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
