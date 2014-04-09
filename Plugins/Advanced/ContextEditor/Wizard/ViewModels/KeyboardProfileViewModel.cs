using System;
using System.Diagnostics;
using CK.Keyboard.Model;
using CK.Windows.App;
using CK.WPF.Wizard;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    /// <summary>
    /// The <see cref="WizardPage"/> enables editing a keyboards basic properties : its name, width and height.
    /// </summary>
    public class KeyboardProfileViewModel : HelpAwareWizardPage
    {
        SimpleKeyboardViewModel _viemModel;
        string _backupFileName;
        IKeyboard _model;

        //Gets whether this step has already been passed. If so, and if the user wants to go back, 
        //we need to destroy/cancel what has been done by the user until then.
        //For keyboards are renamed/created after this step is finished.
        bool _stepAchieved;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        /// <param name="model">The keyboard to create or modify</param>
        public KeyboardProfileViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IKeyboard model )
            : base( root, wizardManager, false )
        {
            _model = model;
            _viemModel = new SimpleKeyboardViewModel( model );

            _backupFileName = Root.BackupKeyboard( model );

            Title = R.KeyboardProfileTitle;
            Description = R.KeyboardProfileDesc;
        }

        /// <summary>
        /// Ctor, creates a new <see cref="SimpleKeyboardViewModel"/>.
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        public KeyboardProfileViewModel( IKeyboardEditorRoot root, WizardManager wizardManager )
            : base( root, wizardManager, false )
        {
            _viemModel = new SimpleKeyboardViewModel();
            _backupFileName = "";

            Title = R.KeyboardProfileTitle;
            Description = R.KeyboardProfileDesc;
        }

        #region Properties

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

        //TODO
        bool _keepRatio;
        public bool KeepRatio
        {
            get { return _keepRatio; }
            set { _keepRatio = value; NotifyOfPropertyChange( () => KeepRatio ); }
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

        #endregion

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

        public override bool OnBeforeGoBack()
        {
            if( _stepAchieved )
            {
                ModalViewModel mvm = new ModalViewModel( R.KeyboardProfileBackPopInTitle, R.KeyboardProfileBackPopInDesc );
                mvm.Buttons.Add( new ModalButton( mvm, R.KeyboardProfileBackPopInYes, ModalResult.Yes ) );
                mvm.Buttons.Add( new ModalButton( mvm, R.KeyboardProfileBackPopInNo, ModalResult.No ) );
                CustomMsgBox msgBox = new CustomMsgBox( ref mvm );
                msgBox.ShowDialog();

                if( mvm.ModalResult != ModalResult.Yes )
                {
                    return false;
                }

                Root.CancelModifications();
                _stepAchieved = false;
            }

            Root.EnsureBackupIsClean();
            return true;
        }

        public override bool OnBeforeNext()
        {
            //if the model is null, it means that we are creating a new keyboard.
            //So we do create it with the user's information.
            if( _model == null )
            {
                _model = Root.KeyboardContext.Service.Keyboards.Create( Name );
                Root.KeyboardBackup = new KeyboardBackup( _model, _backupFileName );
            }
            else if( _model.Name != Name ) _model.Rename( Name );

            Debug.Assert( _model != null, "The keyboard should be created even if the name is already used." );

            _model.CurrentLayout.H = Height;
            _model.CurrentLayout.W = Width;

            //If the EditedContext is not null, it means that we were modifying a keyboard before finishing and going back all the way to modifying another keyboard.
            //We dispose the previous KeyboardEditionViewModel, to unregister events before setting it to null.
            if( Root.EditedContext != null )
            {
                Root.EditedContext.Dispose();
                Root.EditedContext = null;
            }

            Next = new KeyboardEditionViewModel( Root, WizardManager, _model );
            _stepAchieved = true;

            return _model != null;
        }
    }
}
