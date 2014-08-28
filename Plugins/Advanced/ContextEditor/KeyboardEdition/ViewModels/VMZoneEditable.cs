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

namespace KeyboardEditor.ViewModels
{
    public class VMZoneEditable : VMContextElementEditable, IHasOrder
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

            foreach( IKey key in _zone.Keys )
            {
                VMKeyEditable k = Context.Obtain( key );
                Keys.Add( k );
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
                        key.KeyModes.First().UpLabel = "New key";
                        key.CurrentLayout.Current.X = X;
                        key.CurrentLayout.Current.Y = Y;
                        key.CurrentLayout.Current.Width = 40;
                        key.CurrentLayout.Current.Height = 40;
                        key.CurrentLayout.Current.Visible = true;
                        OnPropertyChanged( "Keys" );
                    } );
                }
                return _createKeyCommand;
            }
        }


        ICommand _deleteZoneCommand;
        public ICommand DeleteZoneCommand
        {
            get
            {
                if( _deleteZoneCommand == null )
                {
                    DeleteZone();
                }
                return _deleteZoneCommand;
            }
        }

        private void DeleteZone()
        {
            _deleteZoneCommand = new CK.Windows.App.VMCommand( () =>
            {
                ModalViewModel mvm = new ModalViewModel( R.DeleteZone, R.DeleteZoneConfirmation );
                //mvm.Buttons.Add( new ModalButton( mvm, R.SaveKeys, ModalResult.Yes ) );
                mvm.Buttons.Add( new ModalButton( mvm, R.DeleteKeys, ModalResult.No ) );
                mvm.Buttons.Add( new ModalButton( mvm, R.Cancel, ModalResult.Cancel ) );
                CustomMsgBox msgBox = new CustomMsgBox( ref mvm );
                msgBox.ShowDialog();

                if( mvm.ModalResult == ModalResult.Cancel ) return;

                for( int i = Keys.Count - 1; i >= 0; i-- )
                {
                    Keys[i].Model.Destroy();
                }

                Context.SelectedElement = Parent;
                Model.Destroy();

                #region key saving commented

                ////
                ////Putting the keys of the zone into the default zone, with visible = false
                ////
                //if( mvm.ModalResult == ModalResult.Yes ) //Saving the keys
                //{
                //    for( int i = Model.Keys.Count - 1; i >= 0; i-- )
                //    {
                //        Console.Out.WriteLine( "Touches dans previous Zone avant transfert : " + Model.Keys.Count );
                //        Console.Out.WriteLine( "Touches dans previous zone VM avant transfert : " + Keys.Count );

                //        Console.Out.WriteLine( "Touches dans target zone avant transfert : " + Context.KeyboardVM.Model.Zones[""].Keys.Count );
                //        Console.Out.WriteLine( "Touches dans target zone VM avant transfert : " + Context.Obtain(Context.KeyboardVM.Model.Zones[""]).Keys.Count );

                //        IKey transferred = Model.Keys[i];
                //        Console.Out.WriteLine( "Zone de la touche : " + transferred.Current.UpLabel + ", avant transfert : " + transferred.Zone.Name );

                //        Model.Keys[i].SwapZones( Context.KeyboardVM.Model.Zones[""] );
                //        Console.Out.WriteLine( "Touches dans previous Zone avant transfert : " + Model.Keys.Count );
                //        Console.Out.WriteLine( "Touches dans previous zone VM avant transfert : " + Keys.Count );

                //        Console.Out.WriteLine( "Touches dans target zone avant transfert : " + Context.KeyboardVM.Model.Zones[""].Keys.Count );
                //        Console.Out.WriteLine( "Touches dans target zone VM avant transfert : " + Context.Obtain( Context.KeyboardVM.Model.Zones[""] ).Keys.Count );

                //        Console.Out.WriteLine( "Zone de la touche : " + transferred.Current.UpLabel + ", après transfert : " + transferred.Zone.Name );
                //        Console.Out.WriteLine( "Transfert " + i + " done" );
                //    }

                //    Console.Out.WriteLine( "Fin" );

                //    Console.Out.WriteLine( "Touches dans target zone après transfert : " + Context.KeyboardVM.Model.Zones[""].Keys.Count );
                //    Console.Out.WriteLine( "Touches dans target zone VM après transfert : " + Context.Obtain( Context.KeyboardVM.Model.Zones[""] ).Keys.Count );
                //    Console.Out.WriteLine( "Touches dans previous Zone après transfert : " + Model.Keys.Count );
                //    Console.Out.WriteLine( "Touches dans previous zone VM après transfert : " + Keys.Count );
                //}

                #endregion

            } );
        }

        private IKey InsertKeyMode( IKeyMode keyMode, IKey newKey )
        {

            //IKeyMode newKeyMode = newKey.KeyModes.Create( keyMode.Mode );
            //newKeyMode.UpLabel = keyMode.UpLabel;
            //newKeyMode.DownLabel = keyMode.DownLabel;
            //newKeyMode.Description = keyMode.Description;
            //newKeyMode.Enabled = keyMode.Enabled;

            //foreach( string keyProgram in keyMode.OnKeyDownCommands.Commands )
            //{
            //    newKeyMode.OnKeyDownCommands.Commands.Add( keyProgram );
            //}

            //foreach( string keyProgram in keyMode.OnKeyPressedCommands.Commands )
            //{
            //    newKeyMode.OnKeyPressedCommands.Commands.Add( keyProgram );
            //}

            //foreach( string keyProgram in keyMode.OnKeyUpCommands.Commands )
            //{
            //    newKeyMode.OnKeyUpCommands.Commands.Add( keyProgram );
            //}

            //TODOJL : copy all the plugindatas of the keymode onto the new keymode

            return newKey;
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
            if( e.MultiPluginId.Any( ( c ) => String.Compare( c.UniqueId.ToString(), "36C4764A-111C-45E4-83D6-E38FC1DF5979", StringComparison.InvariantCultureIgnoreCase ) == 0 ) )
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
            //OnPropertyChanged( "Index" );
        }

        public ICommand UpIndexCommand
        {
            get
            {
                return new CK.Windows.App.VMCommand( (Action)( () => Context.KeyboardVM.IncreaseZoneIndex( this )) );
            }
        }

        public ICommand DownIndexCommand
        {
            get
            {
                return new CK.Windows.App.VMCommand( (Action)( () => Context.KeyboardVM.DecreaseZoneIndex( this )) );
            }
        }


        /// <summary>
        /// Gets or sets the Name of the underlying <see cref="IZone"/>
        /// </summary>
        public string Name
        {
            get { return IsDefaultZone ? R.DefaultZone : Model.Name; }
            set { Model.Rename( value ); }
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

        #region move through arrows

        public override void OnKeyDownAction( int keyCode, int delta )
        {
            switch( keyCode )
            {
                case VMContextEditable.suppr:
                    DeleteZone();
                    break;
                case VMContextEditable.left:
                    MoveLeft( delta );
                    break;
                case VMContextEditable.up:
                    MoveUp( delta );
                    break;
                case VMContextEditable.right:
                    MoveRight( delta );
                    break;
                case VMContextEditable.down:
                    MoveDown( delta );
                    break;
            }
        }

        public void MoveUp( int pixels )
        {
            foreach( VMKeyEditable key in Keys )
            {
                key.MoveUp( pixels );
            }
        }

        public void MoveLeft( int pixels )
        {
            foreach( VMKeyEditable key in Keys )
            {
                key.MoveLeft( pixels );
            }
        }

        public void MoveDown( int pixels )
        {
            foreach( VMKeyEditable key in Keys )
            {
                key.MoveDown( pixels );
            }
        }

        public void MoveRight( int pixels )
        {
            foreach( VMKeyEditable key in Keys )
            {
                key.MoveRight( pixels );
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
