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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CK.Plugins.AutoClick.Model;
using HighlightModel;
using CK.Core;
using AutoClick.Res;
using System.Windows.Media;
using CommonServices;

namespace CK.Plugins.AutoClick.ViewModel
{
    public class ClicksVM : ObservableCollection<ClickEmbedderVM>, INotifyPropertyChanged, IHighlightableElement
    {
        #region Properties

        private ClickEmbedderVM _selectedClickEmbedderVM;
        public ClickVM NextClick { get { return _selectedClickEmbedderVM.NextClick; } }

        ISharedData _sharedData;

        private readonly CKReadOnlyCollectionOnICollection<ClickEmbedderVM> _clicksVmReadOnlyAdapter;
        public ICKReadOnlyList<ClickEmbedderVM> ReadOnlyClicksVM { get { return _clicksVmReadOnlyAdapter.ToReadOnlyList(); } }

        internal ClickVM GetNextClick( bool doIncrement, ClickEmbedderVM click = null )
        {
            var clickEmbedder = click ?? _selectedClickEmbedderVM;
            ClickVM nextClick = clickEmbedder.GetNextClick( doIncrement );
            if( doIncrement )
                OnPropertyChanged( "NextClick" );
            return nextClick;
        }

        #endregion

        #region Constructor

        //TODO : remove when the clickselector is transformed into a clickselectorprovider
        public IClickSelector Holder { get; set; }

        public ClicksVM( IClickSelector holder, ISharedData sharedData )
        {
            Holder = holder;
            _sharedData = sharedData;
            InitializeDefaultClicks();
            InitializeSharedData();
            _sharedData.SharedPropertyChanged += OnSharedPropertyChanged;
        }

        public ClicksVM( IClickSelector holder, ISharedData sharedData, IEnumerable<ClickEmbedderVM> clickEmbeddersVM )
        {
            Holder = holder;
            _sharedData = sharedData;

            foreach( ClickEmbedderVM clickEmbedderVM in clickEmbeddersVM )
            {
                Add( clickEmbedderVM );
            }
            _clicksVmReadOnlyAdapter = new CKReadOnlyCollectionOnICollection<ClickEmbedderVM>( this );

            InitializeSharedData();
            _sharedData.SharedPropertyChanged += OnSharedPropertyChanged;

            this.First().ChangeSelectionCommand.Execute( this );
            OnPropertyChanged( "NextClick" );
        }

        #endregion

        #region ISharedData

        double _clickSelectorOpacity;
        public double ClickSelectorOpacity
        {
            get { return _clickSelectorOpacity; }
            set
            {
                if( value != _clickSelectorOpacity )
                {
                    _clickSelectorOpacity = value;
                    OnPropertyChanged( "ClickSelectorOpacity" );
                }
            }
        }

        Color _clickSelectorBackgroundColor;
        public Color ClickSelectorBackgroundColor
        {
            get { return _clickSelectorBackgroundColor; }
            set
            {
                if( value != _clickSelectorBackgroundColor )
                {
                    _clickSelectorBackgroundColor = value;
                    OnPropertyChanged( "ClickSelectorBackgroundColor" );
                }
            }
        }

        int _clickSelectorBorderThickness;
        public int ClickSelectorBorderThickness
        {
            get { return _clickSelectorBorderThickness; }
            set
            {
                if( value != _clickSelectorBorderThickness )
                {
                    _clickSelectorBorderThickness = value;
                    OnPropertyChanged( "ClickSelectorBorderThickness" );
                }
            }
        }

        private Brush _clickSelectorBorderBrush;
        public Brush ClickSelectorBorderBrush
        {
            get { return _clickSelectorBorderBrush; }
            set
            {
                if( value != _clickSelectorBorderBrush )
                {
                    _clickSelectorBorderBrush = value;
                    OnPropertyChanged( "ClickSelectorBorderBrush" );
                }
            }
        }

        void OnSharedPropertyChanged( object sender, SharedPropertyChangedEventArgs e )
        {
            switch( e.PropertyName )
            {
                case "WindowOpacity":
                    ClickSelectorOpacity = _sharedData.WindowOpacity;
                    break;
                case "WindowBorderThickness":
                    ClickSelectorBorderThickness = _sharedData.WindowBorderThickness;
                    break;
                case "WindowBorderBrush":
                    ClickSelectorBorderBrush = new SolidColorBrush( _sharedData.WindowBorderBrush );
                    break;
                case "WindowBackgroundColor":
                    ClickSelectorBackgroundColor = _sharedData.WindowBackgroundColor;
                    break;
            }
        }

        void InitializeSharedData()
        {
            ClickSelectorOpacity = _sharedData.WindowOpacity;
            ClickSelectorBackgroundColor = _sharedData.WindowBackgroundColor;
            ClickSelectorBorderBrush = new SolidColorBrush( _sharedData.WindowBorderBrush );
            ClickSelectorBorderThickness = _sharedData.WindowBorderThickness;
        }

        #endregion

        #region Methods

        private void InitializeDefaultClicks()
        {
            ClickEmbedderVM clickEmbedderVM;

            clickEmbedderVM = new ClickEmbedderVM( this, R.LeftClick, "/Res/Images/LeftClick.png" );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Clic Gauche", new List<ClickInstruction>()
                { 
                    ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp
                } ) );
            Add( clickEmbedderVM );

            clickEmbedderVM = new ClickEmbedderVM( this, R.RightClick, "/Res/Images/RightClick.png" );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Clic Droit", new List<ClickInstruction>() 
                { 
                   ClickInstruction.RightButtonDown, ClickInstruction.RightButtonUp
                } ) );
            Add( clickEmbedderVM );

            clickEmbedderVM = new ClickEmbedderVM( this, R.DoubleClick, "/Res/Images/DoubleLeftClick.png" );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Double Clic", new List<ClickInstruction>() 
                { 
                    ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp, ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp
                } ) );
            Add( clickEmbedderVM );

            clickEmbedderVM = new ClickEmbedderVM( this, R.DragDrop, "/Res/Images/DragDrop.png" );
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
            
                _selectedClickEmbedderVM = selectedClickEmbedder;
                foreach( ClickEmbedderVM clickEmbedderVM in this )
                {
                    clickEmbedderVM.IsSelected = clickEmbedderVM == selectedClickEmbedder;
                }
            OnPropertyChanged( "NextClick" );
        }

        public void Click(ClickEmbedderVM click = null)
        {
            Holder.AskClickType(click);
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
