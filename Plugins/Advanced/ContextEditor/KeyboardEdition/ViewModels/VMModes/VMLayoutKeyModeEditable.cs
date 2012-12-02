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

        public bool IsCurrent { get { return _model.IsCurrent; } }

        public bool IsEmpty { get { return _model.Mode.IsEmpty; } }

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

        public override IKeyboardElement LayoutElement
        {
            get { return Parent.LayoutElement; }
        }

        private VMKeyEditable ActualParent { get { return Parent as VMKeyEditable; } }

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

        VMCommand<string> _deleteKeyModeCommand;
        /// <summary>
        /// Gets a Command that deletes the <see cref="IKeyMode"/> corresponding to the current <see cref="IKeyboardMode"/>, for the underlying <see cref="IKey"/>
        /// </summary>
        public VMCommand<string> DeleteKeyModeCommand
        {
            get
            {
                if( _deleteKeyModeCommand == null )
                {
                    _deleteKeyModeCommand = new VMCommand<string>( ( type ) =>
                    {
                        Context.KeyboardVM.CurrentMode = Context.KeyboardContext.EmptyMode;
                        _model.Destroy();
                        ActualParent.RefreshKeyboardModelViewModels();

                    } );
                }
                return _deleteKeyModeCommand;
            }
        }

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

        public override string ToString()
        {
            return Name;
        }
    }
}
