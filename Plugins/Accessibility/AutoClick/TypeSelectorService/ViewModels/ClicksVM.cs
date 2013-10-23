#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\ViewModels\ClicksVM.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CK.Plugins.AutoClick.Model;

namespace CK.Plugins.AutoClick.ViewModel
{
    public class ClicksVM : ObservableCollection<ClickEmbedderVM>, INotifyPropertyChanged
    {

        #region Properties
        
        private ClickEmbedderVM _selectedClickEmbedderVM;

        public ClickVM NextClick
        {
            get 
            { 
                return _selectedClickEmbedderVM.NextClick; }
        }

        internal ClickVM GetNextClick(bool doIncrement)
        {
            ClickVM nextClick = _selectedClickEmbedderVM.GetNextClick( doIncrement );
            if( doIncrement )
                OnPropertyChanged( "NextClick" );
            return nextClick;
        }

        #endregion

        #region Constructor

        public ClicksVM()
        {
            InitializeDefaultClicks();
        }

        public ClicksVM( IList<ClickEmbedderVM> clickEmbeddersVM )
        {
            foreach( ClickEmbedderVM clickEmbedderVM in clickEmbeddersVM )
            {
                Add( clickEmbedderVM );
            }
            this.First().ChangeSelectionCommand.Execute( this );
            OnPropertyChanged( "NextClick" );
        }

        #endregion

        #region Methods

        private void InitializeDefaultClicks()
        {
            ClickEmbedderVM clickEmbedderVM;
            IList<ClickVM> _clickList = new List<ClickVM>();

            clickEmbedderVM = new ClickEmbedderVM( this, "Clic Gauche", "/AutoClick;component/Res/Images/LeftClick.png", _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Clic Gauche", new List<ClickInstruction>()
                { 
                    ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp
                } ) );            
            Add( clickEmbedderVM );
            _clickList.Clear();

            clickEmbedderVM = new ClickEmbedderVM( this, "Clic Droit", "/AutoClick;component/Res/Images/RightClick.png", _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Clic Droit", new List<ClickInstruction>() 
                { 
                   ClickInstruction.RightButtonDown, ClickInstruction.RightButtonUp
                } ) );
            Add( clickEmbedderVM );
            _clickList.Clear();

            clickEmbedderVM = new ClickEmbedderVM( this, "Double Clic", "/AutoClick;component/Res/Images/DoubleLeftClick.png", _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Double Clic", new List<ClickInstruction>() 
                { 
                    ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp, ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp
                } ) );
            Add( clickEmbedderVM );
            _clickList.Clear();

            clickEmbedderVM = new ClickEmbedderVM( this, "Glisser-Déposer", "/AutoClick;component/Res/Images/DragDrop.png", _clickList );
            clickEmbedderVM.Add(new ClickVM( clickEmbedderVM, "Presser clic Gauche", new List<ClickInstruction>() 
                { 
                     ClickInstruction.LeftButtonDown
                } ));
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Relâcher clic Droit", new List<ClickInstruction>() 
                { 
                    ClickInstruction.LeftButtonUp
                } ) );            
            Add( clickEmbedderVM );

            this.First().ChangeSelectionCommand.Execute( this );
            OnPropertyChanged( "NextClick" );
        }     

        public void ChangeSelection( ClickEmbedderVM selectedClickEmbedder )
        {
            //If the previous selectedClickEmbedder is different from this one
            if( !selectedClickEmbedder.IsSelected )
            {
                _selectedClickEmbedderVM = selectedClickEmbedder;
                foreach( ClickEmbedderVM clickEmbedderVM in this )
                {
                    if( clickEmbedderVM == selectedClickEmbedder )
                        clickEmbedderVM.IsSelected = true;
                    else
                        clickEmbedderVM.IsSelected = false;
                }
            }
            else
            {
                selectedClickEmbedder.Index = 0;
            }
            OnPropertyChanged( "NextClick" );
        }

        #endregion

        #region VMBase Methods
        static bool _throwException;

        /// <summary>
        /// Sets whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the <see cref="CheckPropertyName"/> method.
        /// The default value is false, but it might be set to true in unit test contexts.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void SetThrowOnInvalidPropertyName( bool throwException )
        {
            _throwException = throwException;
        }

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional( "DEBUG" )]
        [DebuggerStepThrough]
        public void CheckPropertyName( string propertyName )
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if( TypeDescriptor.GetProperties( this )[propertyName] == null )
            {
                string msg = "Invalid property name: " + propertyName;
                if( _throwException ) throw new Exception( msg );
                Debug.Fail( msg );
            }
        }

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public new event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        //TODO : new ?

        /// <summary>
        /// Raises this object's <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged( string propertyName )
        {
            this.CheckPropertyName( propertyName );
            System.ComponentModel.PropertyChangedEventHandler handler = this.PropertyChanged;
            if( handler != null )
            {
                var e = new System.ComponentModel.PropertyChangedEventArgs( propertyName );
                handler( this, e );
            }
        }

        #endregion

    }
}
