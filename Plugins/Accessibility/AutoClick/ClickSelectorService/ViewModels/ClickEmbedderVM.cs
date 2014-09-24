#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\ClickSelectorService\ViewModels\ClickEmbedderVM.cs) is part of CiviKey. 
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
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;
using HighlightModel;

namespace CK.Plugins.AutoClick.ViewModel
{
    public class ClickEmbedderVM : ObservableCollection<ClickVM>, INotifyPropertyChanged, IVisualizableHighlightableElement
    {
        #region Variables & Properties

        private ICommand _changeSelectionCmd;
        public ICommand ChangeSelectionCommand
        {
            get { return _changeSelectionCmd; }
        }

        private string _imagePath;
        public string ImagePath { get { return _imagePath; } }

        private int _index;
        public int Index 
        { 
            get { return _index; } 
            set 
            {
                if( _index != value )
                {
                    _index = value;
                    OnPropertyChanged( "Index" );
                    OnPropertyChanged( "NextClick" );
                }
            } 
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private ClicksVM _holder;
        public ClicksVM Holder
        {
            get { return _holder; }
            set { _holder = value; }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set 
            {
                if( _isSelected != value )
                {
                    Index = 0;
                    _isSelected = value; 
                    OnPropertyChanged( "IsSelected" );
                }
            }
        }

        public ClickVM NextClick
        {
            get { return this[Index]; }
        }

        internal ClickVM GetNextClick( bool doIncrement )
        {
            ClickVM nextClick = NextClick;
            if( doIncrement )
            {
                if( Index == this.Count - 1 )
                {
                    Index = 0;
                }
                else Index++;
            }
            return nextClick;
        }

        bool _isHighlighted = false;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;
                OnPropertyChanged( "IsHighlighted" );
            }
        }

        #endregion

        #region Constructor

        public ClickEmbedderVM( ClicksVM holder, string name, string vectorImagePath )
        {
            _holder = holder;
            VectorImagePath = vectorImagePath;
            _imagePath = vectorImagePath;
            _isSelected = false;
            _name = name;

            _changeSelectionCmd = new CK.Windows.App.VMCommand( DoSelect );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Will be called by the UI (via a Command) or an external device (the scroller for example) whenever the clickEmbedder is selected
        /// </summary>
        internal void DoSelect()
        {
            _holder.ChangeSelection( this );
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

        public Core.ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return Core.CKReadOnlyListEmpty<IHighlightableElement>.Empty; }
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
            get { return SkippingBehavior.None; }
        }


        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            IsHighlighted = true;
            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            IsHighlighted = false;
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            _holder.Click(this);

            scrollingDirective.NextActionType = ActionType.GoToAbsoluteRoot;
            return scrollingDirective;
        }
        public bool IsHighlightableTreeRoot
        {
            get { return false; }
        }

        #region IVizualizableHighlightableElement Members

        public string ElementName
        {
            get { return Name; }
        }

        public string VectorImagePath
        {
            get;
            private set;
        }

        #endregion
    }

    #region EventHandlers / EventArgs

    public delegate void SelectedClickEmbedderEventHandler( object sender, SelectedClickEmbedderVMEventArgs e );

    /// <summary>
    /// EventArgs containing the clickEmbedder that needs to be set as the selected one.
    /// </summary>
    public class SelectedClickEmbedderVMEventArgs : EventArgs
    {
        private ClickEmbedderVM _clickembedderVM;
        public ClickEmbedderVM ClickEmbedderVM { get { return _clickembedderVM; } set { _clickembedderVM = value; } }
        public SelectedClickEmbedderVMEventArgs( ClickEmbedderVM ceVM )
        {
            _clickembedderVM = ceVM;
        }
    }

    #endregion
}
