using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Keyboard.Model;

namespace ContextEditor.ViewModels
{
    public class KeyboardProfileViewModel : WizardPage
    {
        SimpleKeyboardViewModel _viemModel;
        IKeyboard _model;
        ContextEditor _root;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        /// <param name="model">The keyboard to create or modify</param>
        public KeyboardProfileViewModel( ContextEditor root, WizardManager wizardManager, IKeyboard model )
            : base( wizardManager, false )
        {
            _root = root;
            _model = model;
            _viemModel = new SimpleKeyboardViewModel( model );
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        public KeyboardProfileViewModel( ContextEditor root, WizardManager wizardManager )
            : base( wizardManager, false )
        {
            _viemModel = new SimpleKeyboardViewModel();
            _root = root;
        }

        /// <summary>
        /// Gets or sets the name of the keyboard
        /// </summary>
        public string Name
        {
            get { return _viemModel.Name; }
            set
            {
                if( value != _viemModel.Name )
                {
                    _viemModel.Name = value;
                    NotifyOfPropertyChange( () => Name );
                    NotifyOfPropertyChange( () => CanGoFurther );
                }
            }
        }

        /// <summary>
        /// Gets or sets the height of the keyboard
        /// </summary>
        public int Height
        {
            get { return _viemModel.Height; }
            set
            {
                if( value != Height )
                {
                    _viemModel.Height = value;
                    NotifyOfPropertyChange( () => Height );
                    NotifyOfPropertyChange( () => CanGoFurther );
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the keyboard
        /// </summary>
        public int Width
        {
            get { return _viemModel.Width; }
            set
            {
                if( value != this.Width )
                {
                    _viemModel.Width = value;
                    NotifyOfPropertyChange( () => Width );
                    NotifyOfPropertyChange( () => CanGoFurther );
                }
            }
        }

        /// <summary>
        /// Checks that all informations has been filled and that there are no keyboards that have the selected name.
        /// </summary>
        /// <returns></returns>
        public override bool CheckCanGoFurther()
        {
            return ( Width > 0
                && Height > 0
                && !String.IsNullOrWhiteSpace( Name ) );
        }

        public override bool OnBeforeNext()
        {
            //if the model is null, it means that we are creating a new keyboard.
            //So we do create it with the user's information.
            if( _model == null )
            {
                _model = _root.KeyboardContext.Service.Keyboards.Create( Name );
            }
            else _model.Rename( Name );

            Debug.Assert( _model != null, "The keyboard should be created even if the name is already used." );

            _model.CurrentLayout.H = Height;
            _model.CurrentLayout.W = Width;

            Next = new KeyboardEditionViewModel( _root, WizardManager, _model );

            return _model != null;
        }
    }
}
