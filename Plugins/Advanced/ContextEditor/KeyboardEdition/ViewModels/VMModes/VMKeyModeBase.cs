#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\ViewModels\VMModes\VMKeyModeBase.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using CK.Keyboard.Model;
using CK.Windows.App;
using System.Collections.Generic;

namespace KeyboardEditor.ViewModels
{
    public class VMKeyModeBase : VMContextElementEditable
    {
        IKeyboardMode _modelMode;
        IKeyPropertyHolder _model;

        public VMKeyModeBase( VMContextEditable context, IKeyMode model )
            : base( context )
        {
            _model = model;
            _modelMode = model.Mode;
        }

        public VMKeyModeBase( VMContextEditable context, ILayoutKeyMode model )
            : base( context )
        {
            _model = model;
            _modelMode = model.Mode;
        }

        public override bool IsSelected { get; set; }

        /// <summary>
        /// Gets whether this LayoutKeyMode is a fallback or not.
        /// see <see cref="IKeyboardMode"/> for more explanations on the fallback concept
        /// This override checks the mode of the actual parent keyboard, instead of getting the current keyboard's mode
        /// </summary>
        public bool IsFallback
        {
            get
            {
                IKeyboardMode keyboardMode = Context.KeyboardVM.CurrentMode;
                return !keyboardMode.ContainsAll( _modelMode ) || !_modelMode.ContainsAll( keyboardMode );
            }
        }

        public bool IsCurrent { get { return _model.IsCurrent; } }

        public bool IsEmpty { get { return _modelMode.IsEmpty; } }

        public string Name { get { return _modelMode.ToString(); } }

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
                        if( !Context.KeyboardVM.CurrentMode.ContainsAll( _modelMode ) || !_modelMode.ContainsAll( Context.KeyboardVM.CurrentMode ) )
                        {
                            Context.KeyboardVM.CurrentMode = _modelMode;
                        }
                    } );
                }
                return _applyToCurrentModeCommand;
            }
        }

        /// <summary>
        /// Returns this VMKeyModeEditable's parent's layout element
        /// </summary>
        public override CK.Keyboard.Model.IKeyboardElement LayoutElement
        {
            get { return Parent.LayoutElement; }
        }

        VMContextElementEditable _parent;
        /// <summary>
        /// Returns this VMKeyModeEditable's parent
        /// </summary>
        public override VMContextElementEditable Parent
        {
            get
            {
                if( _parent == null ) _parent = Context.Obtain( _model.Key );
                return _parent;
            }
        }
        public void TriggerPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );
        }

        public void TriggerModeChanged()
        {
            OnPropertyChanged( "IsFallback" );
            OnModeChangedTriggered();
        }

        protected virtual void OnModeChangedTriggered()
        {
        }

        protected VMKeyEditable ActualParent { get { return Parent as VMKeyEditable; } }

        private IEnumerable<VMContextElementEditable> GetParents()
        {
            VMContextElementEditable elem = this;
            while( elem != null )
            {
                elem = elem.Parent;

                if( elem != null )
                    yield return elem;
            }
        }
    }
}
