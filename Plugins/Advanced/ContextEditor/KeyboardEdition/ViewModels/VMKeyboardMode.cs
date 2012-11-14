using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.WPF.ViewModel;
using ContextEditor.ViewModels;

namespace ContextEditor.ViewModels
{
    /// <summary>
    /// Wrapper on a <see cref="IKeyboardMode"/> that enables setting the <see cref="IsChecked"/> property, 
    /// automatically triggering a command on the holder that activates or deactivates the current mode. 
    /// Also has a command setting the holder's current mode to the one embedded in this class. (deactivates all mode that is not contained in the embedded one)
    /// </summary>
    public class VMKeyboardMode : INotifyPropertyChanged, IDisposable
    {
        VMContextEditable _holder;

        /// <summary>
        /// Gets the underlying model.
        /// </summary>
        public IKeyboardMode Mode { get; private set; }

        /// <summary>
        /// Gets or sets whether the embedded <see cref="IKeyboardMode"/> is activated or deactivated on the holder
        /// </summary>
        public bool IsChecked
        {
            get { return _holder.KeyboardVM.CurrentMode.ContainsAll( Mode ); }
            set
            {
                if( value ) _holder.KeyboardVM.AddKeyboardModeCommand.Execute( Mode );
                else _holder.KeyboardVM.RemoveKeyboardModeCommand.Execute( Mode );
                OnPropertyChanged( "IsChecked" );
            }
        }

        public VMKeyboardMode( VMContextEditable holder, IKeyboardMode keyboardMode )
        {
            Mode = keyboardMode;
            _holder = holder;
        }

        /// <summary>
        /// Gets whether the embedded <see cref="IKeyboardMode"/> matches the holder's current <see cref="IKeyboardMode"/>
        /// </summary>
        public bool IsHolderCurrent { get { return _holder.KeyboardVM.CurrentMode.ContainsAll( Mode ) && Mode.ContainsAll( _holder.KeyboardVM.CurrentMode ); } }

        VMCommand _applyToCurrentModeCommand;
        /// <summary>
        /// Gets a command that sets the embedded <see cref="IKeyboardMode"/> as the holder's current one.
        /// </summary>
        public VMCommand ApplyToCurrentModeCommand
        {
            get
            {
                if( _applyToCurrentModeCommand == null )
                {
                    _applyToCurrentModeCommand = new VMCommand( () =>
                    {
                        if( !IsHolderCurrent )
                        {
                            _holder.KeyboardVM.CurrentMode = Mode;
                        }
                    } );
                }
                return _applyToCurrentModeCommand;
            }
        }

        #region INotifyPropertyChanged Implementation

        public void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
