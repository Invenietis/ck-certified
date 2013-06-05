#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.ViewModel\VMCommand.cs) is part of CiviKey. 
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
using System.Windows.Input;
using System.Diagnostics;

namespace CK.WPF.ViewModel
{

    /// <summary>
    /// A command whose sole purpose is to 
    /// relay its functionality to other
    /// objects by invoking delegates. The
    /// default return value for the CanExecute
    /// method is 'true'.
    /// From http://mvvmfoundation.codeplex.com/ open source project.
    /// </summary>
    public class VMCommand<T> : ICommand
    {
        readonly Action<T> _execute = null;
        readonly Predicate<T> _canExecute = null;

        public VMCommand( Action<T> execute )
            : this( execute, null )
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public VMCommand( Action<T> execute, Predicate<T> canExecute )
        {
            if( execute == null )
                throw new ArgumentNullException( "execute" );

            _execute = execute;
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute( object parameter )
        {
            return _canExecute == null ? true : _canExecute( (T)parameter );
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if( _canExecute != null )
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if( _canExecute != null )
                    CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute( object parameter )
        {
            _execute( (T)parameter );
        }

    }

    /// <summary>
    /// A command whose sole purpose is to 
    /// relay its functionality to other
    /// objects by invoking delegates. The
    /// default return value for the CanExecute
    /// method is 'true'.
    /// From http://mvvmfoundation.codeplex.com/ open source project.
    /// </summary>
    public class VMCommand : ICommand
    {
        #region Constructors

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public VMCommand( Action execute )
            : this( execute, null )
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public VMCommand( Action execute, Func<bool> canExecute )
        {
            if( execute == null )
                throw new ArgumentNullException( "execute" );

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute( object parameter )
        {
            return _canExecute == null ? true : _canExecute();
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if( _canExecute != null )
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if( _canExecute != null )
                    CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute( object parameter )
        {
            _execute();
        }

        #endregion // ICommand Members

        #region Fields

        readonly Action _execute;
        readonly Func<bool> _canExecute;

        #endregion // Fields
    }
}
