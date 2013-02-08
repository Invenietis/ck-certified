#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\EditableSkin\ViewModels\VMContextEditable.cs) is part of CiviKey. 
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

using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using CK.Core;
using CK.Context;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System;
using System.Globalization;
using CK.Plugin;
using CommonServices;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;

namespace ContextEditor.ViewModels
{
    public enum KeyboardEditorMouseEvent
    {
        MouseMove = 1,
        PointerButtonUp = 2,
        PointerButtonDown = 3
    }

    public enum ModeTypes
    {
        Mode = 1,
        Layout = 2
    }

    public class VMContextEditable : VMContext<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>
    {
        Dictionary<object, VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>> _dic;
        ModeTypes _currentlyDisplayedModeType = ModeTypes.Mode;
        IKeyboardEditorRoot _root;

        public VMContextEditable( IKeyboardEditorRoot root, IKeyboard keyboardToEdit, IPluginConfigAccessor config, IPluginConfigAccessor skinConfiguration )
            : base( root.Context, root.KeyboardContext.Service.Keyboards.Context, config, skinConfiguration )
        {
            _root = root;
            Model = root.KeyboardContext.Service;
            KeyboardVM = Obtain( keyboardToEdit );
            _dic = new Dictionary<object, VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>>();
            Initialize();
        }

        private void Initialize()
        {
            PointerDeviceDriver.Service.PointerMove += OnMouseMove;
            PointerDeviceDriver.Service.PointerButtonUp += OnPointerButtonUp;
            KeyboardVM.Initialize();
        }

        //For now, the VMContext is the one handling mouse triggers directly to the ones that need it.
        private void OnMouseMove( object sender, PointerDeviceEventArgs args )
        {
            OnMouseEventTriggered( KeyboardEditorMouseEvent.MouseMove, args );
        }

        //For now, the VMContext is the one handling mouse triggers directly to the ones that need it.
        private void OnPointerButtonUp( object sender, PointerDeviceEventArgs args )
        {
            OnMouseEventTriggered( KeyboardEditorMouseEvent.PointerButtonUp, args );
        }

        private void OnMouseEventTriggered( KeyboardEditorMouseEvent eventType, PointerDeviceEventArgs args )
        {
            if( SelectedElement as VMKeyEditable != null ) ( SelectedElement as VMKeyEditable ).TriggerMouseEvent( eventType, args );
            else if( SelectedElement as VMZoneEditable != null )
            {
                foreach( var key in ( SelectedElement as VMZoneEditable ).Keys )
                {
                    key.TriggerMouseEvent( eventType, args );
                }
            }
            else if( SelectedElement as VMKeyboardEditable != null )
            {
                foreach( var zone in ( SelectedElement as VMKeyboardEditable ).Zones )
                {
                    foreach( var key in zone.Keys )
                    {
                        key.TriggerMouseEvent( eventType, args );
                    }
                }
            }
        }
        
        /// <summary>
        /// The fact that we are displaying the LayoutKeyMode or the KeyMode, on a IKey edition panel must be handled at a higher level.
        /// This property gets whether we should display the KeyMode or the LayoutKeyMode panel.
        /// </summary>
        public ModeTypes CurrentlyDisplayedModeType 
        { 
            get { return _currentlyDisplayedModeType; } 
            set 
            {
                if( _currentlyDisplayedModeType != value )
                {
                    _currentlyDisplayedModeType = value;

                    Debug.Assert( SelectedElement is VMKeyEditable, "When modifying the CurrentlyDisplayedModeType, the selected element should always be a VMKeyEditable" );
                    ( (VMKeyEditable)SelectedElement ).LayoutKeyModeVM.TriggerPropertyChanged( "IsSelected" );
                    ( (VMKeyEditable)SelectedElement ).KeyModeVM.TriggerPropertyChanged( "IsSelected" );

                    OnPropertyChanged( "CurrentlyDisplayedModeType" );
                    Console.Out.WriteLine( "Passage a la valeur : " + value.ToString() );
                }
            } 
        }

        /// <summary>
        /// Gets the model linked to this ViewModel
        /// </summary>
        public IKeyboardContext Model { get; private set; }

        /// <summary>
        /// Gets the pointer device driver, can be used to hook events
        /// </summary>
        public IService<IPointerDeviceDriver> PointerDeviceDriver { get { return _root.PointerDeviceDriver; } }

        /// <summary>
        /// Gets the Skin plugin's configuration accessor
        /// </summary>
        //public new IPluginConfigAccessor SkinConfiguration { get { return _root.SkinConfiguration; } }

        VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> _selectedElement;
        public VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> SelectedElement
        {
            get
            {
                if( _selectedElement == null )
                {
                    _selectedElement = KeyboardVM;
                    _selectedElement.IsSelected = true;
                }
                return _selectedElement;
            }
            set
            {
                if( _selectedElement != value && value != null)
                {
                    if(_selectedElement != null)
                        _selectedElement.IsSelected = false;
                    _selectedElement = value;
                    _selectedElement.IsSelected = true;
                    OnPropertyChanged( "SelectedElement" );
                }
            }
        }

        public VMKeyModeEditable FindViewModel( IKeyMode km )
        {
            VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> vm;
            _dic.TryGetValue( km, out vm );
            return (VMKeyModeEditable)vm;
        }

        public VMKeyModeEditable FindViewModel( ILayoutKeyMode lkm )
        {
            VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> vm;
            _dic.TryGetValue( lkm, out vm );
            return (VMKeyModeEditable)vm;
        }

        protected override VMLayoutKeyModeEditable CreateLayoutKeyMode( ILayoutKeyMode layoutKeyMode )
        {
            VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> vm = new VMLayoutKeyModeEditable( this, layoutKeyMode );
            return (VMLayoutKeyModeEditable)vm;
        }

        protected override VMKeyModeEditable CreateKeyMode( IKeyMode keyMode )
        {
            VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> vm = new VMKeyModeEditable( this, keyMode );
            return (VMKeyModeEditable)vm;
        }

        protected override VMKeyEditable CreateKey( IKey k )
        {
            VMKeyEditable vmKey = new VMKeyEditable( this, k );
            vmKey.Initialize();
            return vmKey;
        }

        protected override VMZoneEditable CreateZone( IZone z )
        {
            VMZoneEditable vmZone = new VMZoneEditable( this, z );
            vmZone.Initialize();
            return vmZone;
        }

        protected override VMKeyboardEditable CreateKeyboard( IKeyboard kb )
        {
            VMKeyboardEditable vmKeyboard = new VMKeyboardEditable( this, kb );
            vmKeyboard.Initialize();
            return vmKeyboard;
        }

        protected override void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            //Do nothing, we are not bound to the current keyboard of the keyboard context
        }

        protected override void OnDispose()
        {
            if( PointerDeviceDriver != null && PointerDeviceDriver.Status == InternalRunningStatus.Started )
            {
                PointerDeviceDriver.Service.PointerMove -= OnMouseMove;
                PointerDeviceDriver.Service.PointerButtonUp -= OnPointerButtonUp;
            }
        }

        VMCommand<VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>> _selectCommand;
        public VMCommand<VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>> SelectCommand
        {
            get
            {
                if( _selectCommand == null )
                {
                    _selectCommand = new CK.WPF.ViewModel.VMCommand<VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>>( ( elem ) =>
                    {
                        SelectedElement = elem;
                    } );
                }
                return _selectCommand;
            }
        }

    }
}
