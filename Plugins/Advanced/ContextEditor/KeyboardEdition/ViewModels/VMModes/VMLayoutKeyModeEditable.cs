using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CK.Keyboard.Model;
using CK.WPF.ViewModel;
using ContextEditor.ViewModels;
using Microsoft.Win32;

//TODOJL : When having the time, replace the VMKeyboardMode of a VMKeyEditable by this object and its Layout parallel
namespace ContextEditor.ViewModels
{
    public class VMLayoutKeyModeEditable : VMLayoutKeyMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>, IModeViewModel
    {
        ILayoutKeyMode _model;
        public VMLayoutKeyModeEditable( VMContextEditable context, ILayoutKeyMode model )
            : base( context, model )
        {
            _model = model;
        }

        //COMMON
        public bool IsCurrent { get { return _model.IsCurrent; } }

        //COMMON
        public bool IsEmpty { get { return _model.Mode.IsEmpty; } }

        //COMMON
        public string Name { get { return String.IsNullOrWhiteSpace( _model.Mode.ToString() ) ? "Default mode" : _model.Mode.ToString(); } }

        /// <summary>
        /// Gets whether the element is selected.
        /// A VMKeyModeEditable is selected if its parent is selected and that the keyboard's current mode correspond to the Mode associated with this VMLayoutKeyModeEditable
        /// </summary>
        public override bool IsSelected
        {
            get { return Parent.IsSelected 
                && ActualParent.CurrentLayoutKeyModeModeVM.Mode.ContainsAll( _model.Mode )
                && _model.Mode.ContainsAll( ActualParent.CurrentLayoutKeyModeModeVM.Mode )
                && Context.CurrentlyDisplayedModeType == ModeTypes.Layout; }
            set
            {
                
                Context.CurrentlyDisplayedModeType = ModeTypes.Layout;
                Context.KeyboardVM.CurrentMode = _model.Mode;
                if( value ) Parent.IsSelected = value;
                
                OnPropertyChanged( "IsSelected" );
            }
        }

        //COMMON
        public override IKeyboardElement LayoutElement
        {
            get { return Parent.LayoutElement; }
        }

        //COMMON
        private VMKeyEditable ActualParent { get { return Parent as VMKeyEditable; } }

        //COMMON
        VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> _parent;
        /// <summary>
        /// Returns this VMKeyModeEditable's parent
        /// </summary>
        public override VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> Parent
        {
            get
            {
                if( _parent == null ) _parent = Context.Obtain( _model.Key );
                return _parent;
            }
        }

        //COMMON
        /// <summary>
        /// Gets whether this LayoutKeyMode is a fallback or not.
        /// see <see cref="IKeyboardMode"/> for more explanations on the fallback concept
        /// This override checks the mode of the actual parent keyboard, instead of getting the current keyboard's mode
        /// </summary>
        public new bool IsFallback
        {
            get
            {
                IKeyboardMode keyboardMode = Context.KeyboardVM.CurrentMode;
                return !keyboardMode.ContainsAll( _model.Mode ) || !_model.Mode.ContainsAll( keyboardMode );
            }
        }

        //COMMON
        VMCommand _deleteKeyModeCommand;
        /// <summary>
        /// Gets a Command that deletes the <see cref="IKeyMode"/> corresponding to the current <see cref="IKeyboardMode"/>, for the underlying <see cref="IKey"/>
        /// </summary>
        public VMCommand DeleteKeyModeCommand
        {
            get
            {
                if( _deleteKeyModeCommand == null )
                {
                    _deleteKeyModeCommand = new VMCommand( () =>
                    {
                        Context.KeyboardVM.CurrentMode = Context.KeyboardContext.EmptyMode;
                        VMKeyEditable parent = ActualParent; //Keeping a ref to the parent, since the model will be detached from its parent when destroyed
                        _model.Destroy();
                        parent.RefreshKeyboardModelViewModels();

                    } );
                }
                return _deleteKeyModeCommand;
            }
        }

        //COMMON
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
                        if( !Context.KeyboardVM.CurrentMode.ContainsAll( _model.Mode ) || !_model.Mode.ContainsAll( Context.KeyboardVM.CurrentMode ) )
                        {
                            Context.KeyboardVM.CurrentMode = _model.Mode;
                        }
                    } );
                }
                return _applyToCurrentModeCommand;
            }
        }

        public bool ShowLabel
        {
            get { return _model.GetPropertyValue<bool>( Context.Config, "ShowLabel", true ); }
        }

        //COMMON
        public override string ToString()
        {
            return Name;
        }
    }
}
