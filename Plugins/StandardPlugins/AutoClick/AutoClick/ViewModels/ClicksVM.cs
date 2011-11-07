using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CK.StandardPlugins.AutoClick.Model;

namespace CK.StandardPlugins.AutoClick.ViewModel
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

            clickEmbedderVM = new ClickEmbedderVM( this, "Clic Gauche", "/Res/Images/LeftClick.png", _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Clic Gauche", new List<ClickInstruction>()
                { 
                    ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp
                } ) );            
            Add( clickEmbedderVM );
            _clickList.Clear();

            clickEmbedderVM = new ClickEmbedderVM( this, "Clic Droit", "/Res/Images/RightClick.png", _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Clic Droit", new List<ClickInstruction>() 
                { 
                   ClickInstruction.RightButtonDown, ClickInstruction.RightButtonUp
                } ) );
            Add( clickEmbedderVM );
            _clickList.Clear();

            clickEmbedderVM = new ClickEmbedderVM( this, "Double Clic", "/Res/Images/DoubleLeftClick.png", _clickList );
            clickEmbedderVM.Add( new ClickVM( clickEmbedderVM, "Double Clic", new List<ClickInstruction>() 
                { 
                    ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp, ClickInstruction.LeftButtonDown, ClickInstruction.LeftButtonUp
                } ) );
            Add( clickEmbedderVM );
            _clickList.Clear();

            clickEmbedderVM = new ClickEmbedderVM( this, "Glisser-Déposer", "/Res/Images/DragDrop.png", _clickList );
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

        #region Events

        public event SelectedClickEmbedderEventHandler ChangeSelectedClickEmbedder;

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
