#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\ClickSelectorService\ViewModels\ClicksVM.cs) is part of CiviKey. 
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CK.Plugins.AutoClick.Model;
using HighlightModel;
using CK.Core;
using AutoClick.Res;

namespace CK.Plugins.AutoClick.ViewModel
{
    public class ClicksVM : ObservableCollection<ClickEmbedderVM>, INotifyPropertyChanged, IHighlightableElement
    {
        #region Properties

        private ClickEmbedderVM _selectedClickEmbedderVM;
        public ClickVM NextClick { get { return _selectedClickEmbedderVM.NextClick; } }

        private readonly CKReadOnlyCollectionOnICollection<ClickEmbedderVM> _clicksVmReadOnlyAdapter;
        public ICKReadOnlyList<ClickEmbedderVM> ReadOnlyClicksVM { get { return _clicksVmReadOnlyAdapter.ToReadOnlyList(); } }

        internal ClickVM GetNextClick( bool doIncrement )
        {
            ClickVM nextClick = _selectedClickEmbedderVM.GetNextClick( doIncrement );
            if( doIncrement )
                OnPropertyChanged( "NextClick" );
            return nextClick;
        }

        string _leftClickVectorPath = "M20.180747,21.585C20.228347,21.644298,20.280347,21.697097,20.326047,21.756895L26.422185,29.758859C29.804953,34.199928,28.94756,40.541042,24.506903,43.923441L23.98421,44.32263C19.541353,47.70653,13.201616,46.847954,9.817549,42.408587L3.7226885,34.40622C3.6758092,34.346324,3.6399197,34.281224,3.5950002,34.220126z M5.408216,20.435999L10.625,27.283507 2.9199337,33.153C0.58399725,28.923633,1.5669129,23.543948,5.408216,20.435999z M12.217023,17.785192C14.812087,17.769212,17.386172,18.751739,19.330999,20.650035L11.625712,26.519999 6.4099998,19.673508C8.1675651,18.414083,10.19864,17.79762,12.217023,17.785192z M7.7603743,0L9.9109099,0C11.445591,1.4389477 12.433944,3.1620827 11.865614,5.1171455 11.457792,6.5197239 10.610847,7.7689734 8.4435108,8.6654558L8.3360851,8.6986551C8.2857921,8.710865 3.4495938,9.8268156 1.8369575,12.91929 1.153331,14.226479 1.1462507,15.741566 1.8142462,17.423152 2.0276377,17.723249 3.2987057,19.358735 5.2883121,18.597541L5.757087,19.825031C3.2381525,20.788423,1.3342407,19.109937,0.67869186,18.088446L0.6232686,17.982246C-0.22443271,15.900264 -0.20734191,13.989581 0.67405343,12.306095 2.4920226,8.8339043 7.363863,7.579525 7.9896266,7.4310856 9.6283839,6.7435923 10.455338,5.9315891 10.556144,5.0248566 10.783956,2.9643345 9.4713957,1.3014984 7.7603743,0z";
        string _rightClickVectorPath = "M20.180747,21.585C20.228347,21.644298,20.280347,21.697097,20.326047,21.756895L26.422185,29.758859C29.804953,34.199928,28.94756,40.541042,24.506903,43.923441L23.98421,44.32263C19.541353,47.70653,13.201616,46.847954,9.817549,42.408587L3.7226885,34.40622C3.6758092,34.346324,3.6399197,34.281224,3.5950002,34.220126z M5.408216,20.435999L10.625,27.283507 2.9199337,33.153C0.58399725,28.923633,1.5669129,23.543948,5.408216,20.435999z M12.217023,17.785192C14.812087,17.769212,17.386172,18.751739,19.330999,20.650035L11.625712,26.519999 6.4099998,19.673508C8.1675651,18.414083,10.19864,17.79762,12.217023,17.785192z M7.7603743,0L9.9109099,0C11.445591,1.4389477 12.433944,3.1620827 11.865614,5.1171455 11.457792,6.5197239 10.610847,7.7689734 8.4435108,8.6654558L8.3360851,8.6986551C8.2857921,8.710865 3.4495938,9.8268156 1.8369575,12.91929 1.153331,14.226479 1.1462507,15.741566 1.8142462,17.423152 2.0276377,17.723249 3.2987057,19.358735 5.2883121,18.597541L5.757087,19.825031C3.2381525,20.788423,1.3342407,19.109937,0.67869186,18.088446L0.6232686,17.982246C-0.22443271,15.900264 -0.20734191,13.989581 0.67405343,12.306095 2.4920226,8.8339043 7.363863,7.579525 7.9896266,7.4310856 9.6283839,6.7435923 10.455338,5.9315891 10.556144,5.0248566 10.783956,2.9643345 9.4713957,1.3014984 7.7603743,0z";
        string _dbClickVectorPath = "M20.180747,21.585C20.228347,21.644298,20.280347,21.697097,20.326047,21.756895L26.422185,29.758859C29.804953,34.199928,28.94756,40.541042,24.506903,43.923441L23.98421,44.32263C19.541353,47.70653,13.201616,46.847954,9.817549,42.408587L3.7226885,34.40622C3.6758092,34.346324,3.6399197,34.281224,3.5950002,34.220126z M5.408216,20.435999L10.625,27.283507 2.9199337,33.153C0.58399725,28.923633,1.5669129,23.543948,5.408216,20.435999z M12.217023,17.785192C14.812087,17.769212,17.386172,18.751739,19.330999,20.650035L11.625712,26.519999 6.4099998,19.673508C8.1675651,18.414083,10.19864,17.79762,12.217023,17.785192z M7.7603743,0L9.9109099,0C11.445591,1.4389477 12.433944,3.1620827 11.865614,5.1171455 11.457792,6.5197239 10.610847,7.7689734 8.4435108,8.6654558L8.3360851,8.6986551C8.2857921,8.710865 3.4495938,9.8268156 1.8369575,12.91929 1.153331,14.226479 1.1462507,15.741566 1.8142462,17.423152 2.0276377,17.723249 3.2987057,19.358735 5.2883121,18.597541L5.757087,19.825031C3.2381525,20.788423,1.3342407,19.109937,0.67869186,18.088446L0.6232686,17.982246C-0.22443271,15.900264 -0.20734191,13.989581 0.67405343,12.306095 2.4920226,8.8339043 7.363863,7.579525 7.9896266,7.4310856 9.6283839,6.7435923 10.455338,5.9315891 10.556144,5.0248566 10.783956,2.9643345 9.4713957,1.3014984 7.7603743,0z";
        string _dragDropVectorPath = "M19.712,20.551L28.039999,28.446855 24.602834,28.776956 27.048252,33.898092 25.032055,34.955999 22.654241,29.735964 19.712,31.983379z M6.4025176,3.84L21.208582,3.84C22.624092,3.8399998,23.772999,4.9858878,23.772999,6.4025874L23.772999,22.496357 18.07376,17.09072 18.07376,28.610998 6.4025176,28.610998C4.9877176,28.610998,3.8399997,27.463789,3.84,26.048481L3.84,6.4025874C3.8399997,4.9858878,4.9877176,3.8399998,6.4025176,3.84z M6.4024458,0L17.494389,0C17.494389,0,18.75351,0.78771353,17.494389,1.9198742L6.4024458,1.9198742C3.9309749,1.9198742,1.9199517,3.9309078,1.9199518,6.4023874L1.9199518,21.308426C1.9199517,21.308426,0.99879551,22.331217,0,21.308426L0,6.4023874C0,2.8717064,2.8718774,0,6.4024458,0z";

        #endregion

        #region Constructor

        //TODO : remove when the clickselector is transformed into a clickselectorprovider
        public ClickSelector Holder { get; set; }

        public ClicksVM()
        {
            InitializeDefaultClicks();
        }

        public ClicksVM( IEnumerable<ClickEmbedderVM> clickEmbeddersVM )
        {
            foreach( ClickEmbedderVM clickEmbedderVM in clickEmbeddersVM )
            {
                Add( clickEmbedderVM );
            }
            _clicksVmReadOnlyAdapter = new CKReadOnlyCollectionOnICollection<ClickEmbedderVM>( this );

            this.First().ChangeSelectionCommand.Execute( this );
            OnPropertyChanged( "NextClick" );
        }

        #endregion

        #region Methods

        private void InitializeDefaultClicks()
        {
            ClickEmbedderVM clickEmbedderVM;
            IList<ClickVM> _clickList = new List<ClickVM>();

            clickEmbedderVM = new ClickEmbedderVM( this, R.LeftClick, "/Res/Images/LeftClick.png", _leftClickVectorPath, _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Clic Gauche", new List<ClickInstruction>()
                { 
                    ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp
                } ) );
            Add( clickEmbedderVM );
            _clickList.Clear();

            clickEmbedderVM = new ClickEmbedderVM( this, R.RightClick, "/Res/Images/RightClick.png", _rightClickVectorPath, _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Clic Droit", new List<ClickInstruction>() 
                { 
                   ClickInstruction.RightButtonDown, ClickInstruction.RightButtonUp
                } ) );
            Add( clickEmbedderVM );
            _clickList.Clear();

            clickEmbedderVM = new ClickEmbedderVM( this, R.DoubleClick, "/Res/Images/DoubleLeftClick.png",_dbClickVectorPath, _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Double Clic", new List<ClickInstruction>() 
                { 
                    ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp, ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp
                } ) );
            Add( clickEmbedderVM );
            _clickList.Clear();

            clickEmbedderVM = new ClickEmbedderVM( this, R.DragDrop, "/Res/Images/DragDrop.png",_dragDropVectorPath, _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, R.LeftDown, new List<ClickInstruction>() 
                { 
                     ClickInstruction.LeftButtonDown
                } ) );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, R.LeftUp, new List<ClickInstruction>() 
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

        public void Click()
        {
            Holder.AskClickType();
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

        #region IHighlitableElement

        public Core.ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return ReadOnlyClicksVM; }
        }

        public int X
        {
            get { return 0; }
        }

        public int Y
        {
            get { return 0; }
        }

        public int Width
        {
            get { return 0; }
        }

        public int Height
        {
            get { return 0; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.EnterChildren; }
        }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return false; }
        }

        #endregion
    }
}
