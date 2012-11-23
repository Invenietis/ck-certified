#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\EditableSkin\ViewModels\VMKeyEditable.cs) is part of CiviKey. 
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
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using System.Windows.Controls;
using System.Windows;
using CK.Plugin.Config;
using CK.Core;
using Microsoft.Win32;
using System.Windows.Input;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using CommonServices;

namespace ContextEditor.ViewModels
{
    public partial class VMKeyEditable : VMKey<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>
    {
        VMContextEditable _ctx;
        bool _isSelected;

        public VMKeyEditable( VMContextEditable ctx, IKey k )
            : base( ctx, k, false )
        {
            _ctx = ctx;
            KeyDownCommand = new VMCommand( () => _ctx.SelectedElement = this );
            _currentKeyModeModeVM = new VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>( _ctx, k.Current.Mode );
            _currentLayoutKeyModeModeVM = new VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>( _ctx, k.CurrentLayout.Current.Mode );
        }

        #region Properties

        public override VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> Parent
        {
            get { return Context.Obtain( Model.Zone ); }
        }

        /// <summary>
        /// Gets whether this element is being edited. 
        /// An element is being edited if it IsSelected or one of its parents is being edited.
        /// </summary>
        public override bool IsBeingEdited
        {
            get { 
                return IsSelected || Parent.IsBeingEdited; }
        }

        /// <summary>
        /// Gets whether this element is selected.
        /// </summary>
        public override bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                Context.SelectedElement = this;
                OnPropertyChanged( "IsBeingEdited" );
                OnPropertyChanged( "IsSelected" );
                OnPropertyChanged( "Opacity" );
            }
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            Context.Config.ConfigChanged += new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );

            SetActionOnPropertyChanged( "CurrentLayout", () =>
            {
                OnPropertyChanged( "HighlightBackground" );
                OnPropertyChanged( "PressedBackground" );
                OnPropertyChanged( "HoverBackground" );
                OnPropertyChanged( "TextDecorations" );
                OnPropertyChanged( "LetterColor" );
                OnPropertyChanged( "FontWeight" );
                OnPropertyChanged( "Background" );
                OnPropertyChanged( "FontStyle" );
                OnPropertyChanged( "ShowLabel" );
                OnPropertyChanged( "FontSize" );
                OnPropertyChanged( "Opacity" );
                OnPropertyChanged( "Image" );
            } );

            this.PropertyChanged += new PropertyChangedEventHandler( OnPropertyChangedTriggered );
        }

        internal void TriggerOnPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );
        }


        public void TriggerMouseEvent( KeyboardEditorMouseEvent eventType, PointerDeviceEventArgs args )
        {
            switch( eventType )
            {
                case KeyboardEditorMouseEvent.MouseMove:
                    OnMouseMove( args );
                    break;
                case KeyboardEditorMouseEvent.PointerButtonUp:
                    OnPointerButtonUp( args );
                    break;
                default: //ButtonDown is handler by a Command, we don't use the pointer device driver for that. (yet ?)
                    break;
            }
        }

        #endregion

        #region OnXXX

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( LayoutKeyMode.GetPropertyLookupPath().Contains( e.Obj ) )
            {
                OnPropertyChanged( "HighlightBackground" );
                OnPropertyChanged( "PressedBackground" );
                OnPropertyChanged( "HoverBackground" );
                OnPropertyChanged( "TextDecorations" );
                OnPropertyChanged( "LetterColor" );
                OnPropertyChanged( "Background" );
                OnPropertyChanged( "FontWeight" );
                OnPropertyChanged( "FontStyle" );
                OnPropertyChanged( "ShowLabel" );
                OnPropertyChanged( "FontSize" );
                OnPropertyChanged( "Opacity" );
                OnPropertyChanged( "Image" );
            }
        }

        private void OnPropertyChangedTriggered( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsBeingEdited" )
            {
                OnPropertyChanged( "Opacity" );
            }
        }

        protected override void OnDispose()
        {
            Context.Config.ConfigChanged -= new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );
            base.OnDispose();
        }

        #endregion
    }
}
