#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\ViewModels\VMZoneEditable.cs) is part of CiviKey. 
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

using CK.Keyboard.Model;
using CK.Core;
using System.Linq;
using System;
using System.Windows.Input;
using CK.Windows.App;
using KeyboardEditor.Resources;
using CK.WPF.ViewModel;
using CK.Plugin.Config;
using KeyboardEditor.Model;
using System.Collections;

namespace KeyboardEditor.ViewModels
{
    public class VMZoneEditable : VMContextElementEditable, IHasOrder, IHandleDragDrop
    {
        IZone _zone;
        CKObservableSortedArrayKeyList<VMKeyEditable, int> _keys;

        public IZone Model { get { return _zone; } }

        public CKObservableSortedArrayKeyList<VMKeyEditable, int> Keys { get { return _keys; } }

        public VMZoneEditable( VMContextEditable ctx, IZone zone )
            : base( ctx )
        {
            _ctx = ctx;
            _zone = zone;
            _keys = new CKObservableSortedArrayKeyList<VMKeyEditable, int>( k => k.Index );

            VMKeyEditable[] keys = new VMKeyEditable[_zone.Keys.Count];
            foreach( IKey key in _zone.Keys )
            {
                VMKeyEditable k = Context.Obtain( key );
                keys[k.Index] = k;
            }

            for( int i = 0; i < keys.Length; i++ )
            {
                Keys.Add( keys[i] );
            }


            if( _ctx.SkinConfiguration != null )
                _ctx.SkinConfiguration.ConfigChanged += OnLayoutConfigChanged;
        }

        internal override void Dispose()
        {
            foreach( VMKeyEditable key in Keys )
            {
                key.Dispose();
            }

            base.Dispose();
        }

        public override VMContextElementEditable Parent
        {
            get { return Context.Obtain( Model.Keyboard ); }
        }

        public override IKeyboardElement LayoutElement
        {
            get { return Model.CurrentLayout; }
        }

        bool _isSelected;
        /// <summary>
        /// Gets whether this element is being edited.
        /// </summary>
        public override bool IsBeingEdited
        {
            get { return IsSelected || Parent.IsBeingEdited; }
        }

        public override bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                Context.SelectedElement = this;
                OnPropertyChanged( "IsBeingEdited" );
                OnPropertyChanged( "IsSelected" );
                foreach( var item in Keys )
                {
                    item.TriggerOnPropertyChanged( "IsBeingEdited" );
                }
            }
        }

        internal void TriggerOnPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );
        }

        internal void TriggerOnPropertyChanged( string propertyName, bool propagate )
        {
            OnPropertyChanged( propertyName );
            if( !propagate ) return;

            foreach( var item in Keys )
            {
                item.TriggerOnPropertyChanged( propertyName );
            }
        }

        VMContextEditable _ctx;

        ICommand _createKeyCommand;
        public ICommand CreateKeyCommand
        {
            get
            {
                if( _createKeyCommand == null )
                {
                    _createKeyCommand = new CK.Windows.App.VMCommand( () =>
                    {
                        IKey key = Model.Keys.Create();
                        key.KeyModes.First().UpLabel = String.Empty;
                        key.CurrentLayout.Current.X = X;
                        key.CurrentLayout.Current.Y = Y;
                        key.CurrentLayout.Current.Width = 40;
                        key.CurrentLayout.Current.Height = 40;
                        key.CurrentLayout.Current.Visible = true;
                        OnPropertyChanged( "Keys" );

                        var vm = Keys.Single( k => k.Model == key );
                        Context.SelectedElement = vm.LayoutKeyModeVM;
                    } );
                }
                return _createKeyCommand;
            }
        }

        ICommand _clearZoneCommand;
        public ICommand ClearZoneCommand
        {
            get
            {
                if( _clearZoneCommand == null )
                {
                    _clearZoneCommand = new CK.Windows.App.VMCommand( ClearZone );
                }
                return _clearZoneCommand;
            }
        }

        private void ClearZone()
        {
            ModalViewModel mvm = new ModalViewModel( R.ClearZone, R.ClearZoneConfirmation );
            mvm.Buttons.Add( new ModalButton( mvm, R.SaveKeys, ModalResult.Yes ) );
            mvm.Buttons.Add( new ModalButton( mvm, R.Cancel, ModalResult.No ) );
            CustomMsgBox msgBox = new CustomMsgBox( ref mvm );
            msgBox.ShowDialog();

            if( mvm.ModalResult == ModalResult.Yes )
            {
                for( int i = Keys.Count - 1; i >= 0; i-- )
                {
                    Keys[i].Model.Destroy();
                }
            }
        }

        ICommand _deleteZoneCommand;
        public ICommand DeleteZoneCommand
        {
            get
            {
                if( _deleteZoneCommand == null )
                {
                    _deleteZoneCommand = new CK.Windows.App.VMCommand( DeleteZone );
                }
                return _deleteZoneCommand;
            }
        }

        private void DeleteZone()
        {
            if( Model.IsDefault ) return;

            ModalViewModel mvm = new ModalViewModel( R.DeleteZone, R.DeleteZoneConfirmation );
            mvm.Buttons.Add( new ModalButton( mvm, R.SaveKeys, ModalResult.Yes ) );
            mvm.Buttons.Add( new ModalButton( mvm, R.DeleteKeys, ModalResult.No ) );
            mvm.Buttons.Add( new ModalButton( mvm, R.Cancel, ModalResult.Cancel ) );
            CustomMsgBox msgBox = new CustomMsgBox( ref mvm );
            msgBox.ShowDialog();

            if( mvm.ModalResult == ModalResult.Cancel ) return;

            for( int i = Keys.Count - 1; i >= 0; i-- )
            {
                if( mvm.ModalResult == ModalResult.Yes ) //Saving the keys
                {
                    IKey key = Model.Keyboard.Zones[0].Keys.CreateCopy( Keys[i].Model );
                    foreach( var item in key.CurrentLayout.LayoutKeyModes )
                    {
                        item.Visible = false;
                    }
                }
                Keys[i].Model.Destroy();
            }

            Context.SelectedElement = Parent;
            Model.Destroy();
        }

        VMCommand<VMKeyEditable> _insertKeyCommand;
        public VMCommand<VMKeyEditable> InsertKeyCommand
        {
            get
            {
                if( _insertKeyCommand == null )
                {
                    _insertKeyCommand = new VMCommand<VMKeyEditable>( vmKey =>
                    {
                        TransfertKey( vmKey, this );
                    } );
                }

                return _insertKeyCommand;
            }
        }

        void TransfertKey( VMKeyEditable key, VMZoneEditable targetZone )
        {
            if( key.Parent != targetZone )
            {
                var target = targetZone.Model.Keys.CreateCopy( key.Model );
                key.Model.Destroy();
            }
        }

        ICommand _selectZoneCommand;
        public ICommand SelectZoneCommand
        {
            get
            {
                if( _selectZoneCommand == null )
                {
                    _selectZoneCommand = new CK.Windows.App.VMCommand( () =>
                    {
                        _ctx.SelectedElement = this;
                    } );
                }
                return _selectZoneCommand;
            }
        }

        public int Index
        {
            get { return Model.Index; }
            set { Model.Index = value; }
        }

        public int LoopCount
        {
            get { return _zone.GetPropertyValue( _ctx.SkinConfiguration, "LoopCount", 1 ); }
            set
            {
                _ctx.SkinConfiguration[_zone]["LoopCount"] = value;
            }
        }

        VMCommand<string> _clearCmd;
        public VMCommand<string> ZoneClearPropertyCmd { get { return _clearCmd ?? (_clearCmd = new VMCommand<string>( ClearProperty, CanClearProperty )); } }

        void ClearProperty( string propertyName )
        {
            string[] names = propertyName.Split( ',' );
            foreach( var pname in names ) Context.SkinConfiguration[_zone].Remove( pname );
        }

        bool CanClearProperty( string propertyName )
        {
            string[] names = propertyName.Split( ',' );

            // We can clear property if the property owns directly a value.
            foreach( var pname in names ) if( Context.SkinConfiguration[_zone][pname] != null ) return true;
            return false;
        }

        void OnLayoutConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( ( c ) => String.Compare( c.UniqueId.ToString(), "36C4764A-111C-45E4-83D6-E38FC1DF5979", StringComparison.InvariantCultureIgnoreCase ) == 0 ) ) //MainKeyboardManager
            {
                switch( e.Key )
                {
                    case "LoopCount":
                        OnPropertyChanged( "LoopCount" );
                        break;
                    default:
                        break;
                }
            }
        }

        internal void IndexChanged()
        {
            OnPropertyChanged( "Index" );
        }

        internal void KeyIndexChanged( int previousIndex )
        {
            _keys.CheckPosition( previousIndex );
        }

        public ICommand UpIndexCommand
        {
            get
            {
                return new CK.Windows.App.VMCommand( (Action)(() => Context.KeyboardVM.IncreaseZoneIndex( this )) );
            }
        }

        public ICommand DownIndexCommand
        {
            get
            {
                return new CK.Windows.App.VMCommand( (Action)(() => Context.KeyboardVM.DecreaseZoneIndex( this )) );
            }
        }

        /// <summary>
        /// Gets or sets the Name of the underlying <see cref="IZone"/>
        /// </summary>
        public string Name
        {
            get { return IsDefaultZone ? R.DefaultZone : Model.Name; }
            set
            {
                if( !String.IsNullOrWhiteSpace( value ) )
                    Model.Rename( value );
                IsBeingRenamed = false;
            }
        }

        public bool IsDefaultZone
        {
            get { return String.IsNullOrWhiteSpace( Model.Name ); }
        }

        /// <summary>
        /// Gets the X position of the key contained in this zone that is the nearer to the Top Left corner;
        /// </summary>
        public int X
        {
            get { return Keys.Min( k => k.X ); }
        }

        /// <summary>
        /// Gets the Y position of the key contained in this zone that is the nearer to the Top Left corner;
        /// </summary>
        public int Y
        {
            get { return Keys.Min( k => k.Y ); }
        }

        /// <summary>
        /// Gets the Width between the two keys contained in this zone that are the farther from each other (width-wise)
        /// </summary>
        public int Width
        {
            get { return Keys.Max( k => k.X + k.Width ) - X; }
        }

        /// <summary>
        /// Gets the Height between the two keys contained in this zone that are the farther from each other (height-wise)
        /// </summary>
        public int Height
        {
            get { return Keys.Max( k => k.Y + k.Height ) - Y; }
        }

        #region KeyBinding

        internal int GetMaximumMovementAmplitude( MoveDirection direction, int pixels )
        {
            return Keys.Count > 0 ? Keys.Min( k => k.GetMaximumMovementAmplitude( direction, pixels ) ) : 0;
        }

        internal override void OnMove( MoveDirection direction, int pixels, bool arrangeMovementAmplitude = true )
        {
            if( arrangeMovementAmplitude ) pixels = GetMaximumMovementAmplitude( direction, pixels );

            foreach( VMKeyEditable key in Keys )
            {
                key.OnMove( direction, pixels, false );
            }
        }

        internal override void OnSuppr()
        {
            DeleteZone();
        }

        #endregion

        #region IHandleDragDrop Members

        bool _isEnabled = true;
        public bool IsDragDropEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
            }
        }

        public bool CanBeDropTarget( IHandleDragDrop draggedItem )
        {
            return draggedItem is VMKeyEditable;
        }

        public bool CanBeDropSource( IHandleDragDrop target )
        {
            return false;
        }

        public void ExecuteDropAction( IHandleDragDrop droppedItem )
        {
            VMKeyEditable actualDroppedItem = droppedItem as VMKeyEditable;
            if( actualDroppedItem != null )
            {
                //If the target Zone is already the key's zone, then we put the key at the index 0.
                if( actualDroppedItem.Model.Zone == Model )
                {
                    actualDroppedItem.Index = 0;
                }
                else
                {
                    //otherwise, we copy the key into this zone's key list and destroy the dropped key.
                    InsertKeyCommand.Execute( droppedItem );
                }
            }
        }

        #endregion
    }

    public interface IHasOrder
    {
        ICommand UpIndexCommand { get; }
        ICommand DownIndexCommand { get; }
    }
}
