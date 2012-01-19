﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugins.AutoClick.Model;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;
using CK.WPF.ViewModel;

namespace CK.Plugins.AutoClick.ViewModel
{
    public class ClickEmbedderVM : ObservableCollection<ClickVM>, INotifyPropertyChanged
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
        public int Index { get { return _index; } set { _index = value; OnPropertyChanged( "Index" ); OnPropertyChanged( "NextClick" ); } }

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
            set { _isSelected = value; OnPropertyChanged( "IsSelected" ); }
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

        #endregion

        #region Constructor

        public ClickEmbedderVM( ClicksVM holder, string name, string imagePath, IList<ClickVM> clicks )
        {
            _holder = holder;
            _imagePath = imagePath;
            _isSelected = false;
            _name = name;
            foreach( ClickVM click in clicks )
            {
                Add( click );
            }
            
            _changeSelectionCmd = new VMCommand( DoSelect );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Will be called by the UI (via a Command) whenever the clickEmbedder is selected
        /// </summary>
        private void DoSelect()
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