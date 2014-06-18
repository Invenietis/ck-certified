using CK.Keyboard.Model;
using CK.Windows.App;
using System.Collections.Generic;

namespace KeyboardEditor.ViewModels
{
    public class VMKeyModeBase : VMContextElementEditable
    {
        IKeyboardMode _modelMode;
        IKeyPropertyHolder _model;

        public VMKeyModeBase( VMContextEditable context, IKeyMode model )
            : base( context )
        {
            _model = model;
            _modelMode = model.Mode;
        }

        public VMKeyModeBase( VMContextEditable context, ILayoutKeyMode model )
            : base( context )
        {
            _model = model;
            _modelMode = model.Mode;
        }

        public override bool IsSelected { get; set; }

        /// <summary>
        /// Gets whether this LayoutKeyMode is a fallback or not.
        /// see <see cref="IKeyboardMode"/> for more explanations on the fallback concept
        /// This override checks the mode of the actual parent keyboard, instead of getting the current keyboard's mode
        /// </summary>
        public bool IsFallback
        {
            get
            {
                IKeyboardMode keyboardMode = Context.KeyboardVM.CurrentMode;
                return !keyboardMode.ContainsAll( _modelMode ) || !_modelMode.ContainsAll( keyboardMode );
            }
        }

        public bool IsCurrent { get { return _model.IsCurrent; } }

        public bool IsEmpty { get { return _modelMode.IsEmpty; } }

        public string Name { get { return _modelMode.ToString(); } }

        VMCommand _applyToCurrentModeCommand;
        /// <summary>
        /// Gets a command that sets the embedded <see cref="IKeyboardMode"/> as the holder's current one.
        /// </summary>
        public VMCommand ApplyToCurrentModeCommand
        {
            get
            {
                if( _applyToCurrentModeCommand == null )
                {
                    _applyToCurrentModeCommand = new VMCommand( () =>
                    {
                        if( !Context.KeyboardVM.CurrentMode.ContainsAll( _modelMode ) || !_modelMode.ContainsAll( Context.KeyboardVM.CurrentMode ) )
                        {
                            Context.KeyboardVM.CurrentMode = _modelMode;
                        }
                    } );
                }
                return _applyToCurrentModeCommand;
            }
        }

        /// <summary>
        /// Returns this VMKeyModeEditable's parent's layout element
        /// </summary>
        public override CK.Keyboard.Model.IKeyboardElement LayoutElement
        {
            get { return Parent.LayoutElement; }
        }

        VMContextElementEditable _parent;
        /// <summary>
        /// Returns this VMKeyModeEditable's parent
        /// </summary>
        public override VMContextElementEditable Parent
        {
            get
            {
                if( _parent == null ) _parent = Context.Obtain( _model.Key );
                return _parent;
            }
        }
        public void TriggerPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );
        }

        public void TriggerModeChanged()
        {
            OnPropertyChanged( "IsFallback" );
            OnModeChangedTriggered();
        }

        protected virtual void OnModeChangedTriggered()
        {
        }

        protected VMKeyEditable ActualParent { get { return Parent as VMKeyEditable; } }

        private IEnumerable<VMContextElementEditable> GetParents()
        {
            VMContextElementEditable elem = this;
            while( elem != null )
            {
                elem = elem.Parent;

                if( elem != null )
                    yield return elem;
            }
        }

        internal override void OnMoveUp( int pixels )
        {
            ActualParent.OnMoveUp( pixels );
        }

        internal override void OnMoveLeft( int pixels )
        {
            ActualParent.OnMoveLeft( pixels );
        }

        internal override void OnMoveDown( int pixels )
        {
            ActualParent.OnMoveDown( pixels );
        }

        internal override void OnMoveRight( int pixels )
        {
            ActualParent.OnMoveRight( pixels );
        }

        internal override void OnSuppr()
        {
            ActualParent.DeleteKey();
        }
    }
}
