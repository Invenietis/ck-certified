using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WPF.ViewModel;
using ContextEditor.ViewModels;

//TODOJL : When having the time, replace the VMKeyboardMode of a VMKeyEditable by this object and its Layout parallel
namespace ContextEditor.KeyboardEdition.ViewModels
{
    //public class VMKeyModeEditable : VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>
    //{
    //    VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> _associatedMode;
    //    VMKeyEditable _holder;

    //    public VMKeyModeEditable( VMContextEditable context, VMKeyEditable holder, VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> associatedMode )
    //        : base( context )
    //    {
    //        _holder = holder;
    //        _associatedMode = associatedMode;
    //    }

    //    /// <summary>
    //    /// Gets whether the element is being edited.
    //    /// A VMKeyModeEditable is being edited if it is selected
    //    /// </summary>
    //    public abstract bool IsBeingEdited { get { return IsSelected; } }

    //    private bool _isSelected;
    //    /// <summary>
    //    /// Gets whether the element is selected.
    //    /// A VMKeyModeEditable is selected if its parent is selected and that the keyboard's current mode correspond to the Mode associated with this VMKeyModeEditable
    //    /// </summary>
    //    public abstract bool IsSelected
    //    {
    //        get
    //        {
    //            return _isSelected && Parent.IsSelected && _holder.CurrentKeyModeModeVM.Mode.ContainsAll( _associatedMode.Mode );
    //        }
    //        set
    //        {
    //            _isSelected = value;
    //            Parent.IsSelected = value;
    //            OnPropertyChanged( "IsSelected" );
    //        }
    //    }
        
    //    private IEnumerable<VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>> GetParents()
    //    {
    //        VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> elem = this;
    //        while( elem != null )
    //        {
    //            elem = elem.Parent;

    //            if( elem != null )
    //                yield return elem;
    //        }
    //    }
    //}
}
