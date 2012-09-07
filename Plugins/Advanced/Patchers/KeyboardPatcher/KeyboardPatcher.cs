using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CK.Context;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows.App;

namespace KeyboardPatcher
{
    public delegate T CKFunc<T, O>( out O errorMessage );

    /// <summary>
    /// This plugin hosts keyboard updates.
    /// </summary>
    [Plugin( StrPluginID, PublicName = "Keyboard Patcher", Version = "1.0.0", Categories = new string[] { "Patcher" } )]
    public class KeyboardPatcher : IPlugin
    {
        public const string StrPluginID = "{0F740086-85AC-46EB-87ED-12A4CA2D12D9}";
        IList<CKPatch> _patchs;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [ConfigurationAccessor( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" )]
        public IPluginConfigAccessor EditedConfiguration { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            _patchs = new List<CKPatch>();
            _patchs.Add( new CKPatch( "KeyboardPatch", 1, CanExecutePatchV1, ApplyPatchV1, true ) );
            return true;
        }

        public void Start()
        {
            TryApplyPatches();
        }

        private void TryApplyPatches()
        {
            string errorMessage = String.Empty;
            bool stopReminder = false;
            for( int i = 0; i < _patchs.Count; i++ )
            {
                CKPatch patch = _patchs[i];
                if( Config.User.GetOrSet<CKPatchStatus>( patch.Info.ConfPathString, CKPatchStatus.NotInstalled ) == CKPatchStatus.NotInstalled )
                {
                    if( patch.TryApplyPatch( out errorMessage, out stopReminder ) )
                    {
                        if( patch.IsCorrectlyApplied == null || patch.IsCorrectlyApplied() )
                            Config.User.Set( patch.Info.ConfPathString, CKPatchStatus.Installed );
                    }
                    else if( stopReminder )
                    {
                        Config.User.Set( patch.Info.ConfPathString, CKPatchStatus.Error );
                        Config.User.Set( patch.Info.ConfPathErrorString, errorMessage );
                    }
                }
            }
        }

        public void Stop()
        {
            _patchs = null;
        }

        public void Teardown()
        {

        }

        //PatchV1 adds a button designed to toggle the host's window's visibility.
        #region PatchV1

        public bool CanExecutePatchV1( out string errorMessage )
        {
            errorMessage = String.Empty;
            if( KeyboardContext != null )
            {
                IKeyboard keyboard = KeyboardContext.Service.Keyboards["Azerty"];
                if( keyboard != null )
                {
                    IKeyCollection keys = keyboard.Zones["CustomFunctions"].Keys;
                    IKey lastKey = keys.LastOrDefault();
                    if( lastKey != null && lastKey.Current.UpLabel != "MinimizeHost" )
                    {
                        return true;
                    }
                    else errorMessage = String.Format( R.MinimizeHostKeyAlreadyAdded, DateTime.Now );
                }
                else errorMessage = String.Format( R.AzertyKeyboardNotFound, DateTime.Now );
            }
            return false;
        }

        public void ApplyPatchV1()
        {
            string zoneName = "CustomFunctions";
            string keyboardName = "Azerty";

            IKeyboard keyboard = KeyboardContext.Service.Keyboards[keyboardName];

            //Adding the Key
            IKeyCollection keys = keyboard.Zones[zoneName].Keys;
            IKey newKey = keys.Create( keys.Count );
            IKeyMode newKeyEmptyMode = newKey.KeyModes.Create( KeyboardContext.Service.EmptyMode );
            newKeyEmptyMode.UpLabel = "MinimizeHost";
            newKeyEmptyMode.Description = R.DisplayConfigWindow;
            newKeyEmptyMode.Enabled = true;
            newKeyEmptyMode.OnKeyDownCommands.Commands.Add( "DynCommand(\"ToggleHostMinimized\")" );

            //Adding the layoutKey
            ILayoutKeyCollection layoutKeys = keyboard.CurrentLayout.LayoutZones[zoneName].LayoutKeys;
            ILayoutKey newLayoutKey = newKey.CurrentLayout;
            newLayoutKey.Current.X = 880;
            newLayoutKey.Current.Y = 270;
            newLayoutKey.Current.Width = 50;
            newLayoutKey.Current.Height = 50;
            newLayoutKey.Current.Visible = true;

            //Adding the image
            Image image = new Image();
            Uri src = new Uri( @"/SimpleSkin;component/Images/EditorLogo.png", UriKind.Relative );
            image.Source = new BitmapImage( src );
            EditedConfiguration[newLayoutKey].Add( "Image", image );
        }

        #endregion

    }

    public class CKPatch
    {
        public CKFunc<bool, string> CanExecute { get; private set; }
        public Func<bool> IsCorrectlyApplied { get; private set; }
        public bool ShouldAskUser { get; private set; }
        public CKPatchInfo Info { get; private set; }
        public Action Action { get; private set; }

        public bool TryApplyPatch( out string errorMessage, out bool stopReminder )
        {
            errorMessage = String.Empty;
            stopReminder = false;
            ModalViewModel mvm = new ModalViewModel( R.KeyboardUpdateTitle, R.KeyboardUpdateDescription, true, R.RememberMyDecision );
            mvm.Buttons.Add( new ModalButton( mvm, R.Yes, null, ModalResult.Yes ) );
            mvm.Buttons.Add( new ModalButton( mvm, R.No, null, ModalResult.No ) );
            if( ShouldAskUser )
            {
                CustomMsgBox msgBox = new CustomMsgBox( ref mvm );
                msgBox.ShowDialog();
            }

            if( !ShouldAskUser || mvm.ModalResult == ModalResult.Yes )
            {
                if( CanExecute( out errorMessage ) )
                {
                    Action.Invoke();
                    return true;
                }
            }
            else if( mvm.IsCheckboxChecked )
            {
                errorMessage = String.Format( R.UserCancelled, DateTime.Now );
                stopReminder = true;
            }
            return false;
        }

        public CKPatch( string name, int patchNumber, CKFunc<bool, string> canExecute, Action action, bool shouldAskUser )
            : this( name, patchNumber, canExecute, action, null, shouldAskUser )
        {
        }

        public CKPatch( string name, int patchNumber, CKFunc<bool, string> canExecute, Action action, Func<bool> isCorrectlyApplied, bool shouldAskUser )
        {
            Info = new CKPatchInfo( name, patchNumber );
            IsCorrectlyApplied = isCorrectlyApplied;
            ShouldAskUser = shouldAskUser;
            CanExecute = canExecute;
            Action = action;
        }
    }

    public class CKPatchInfo
    {
        public string ConfPathString { get { return Name + PatchNumber; } }
        public string ConfPathErrorString { get { return Name + PatchNumber + "ErrorMsg"; } }
        public int PatchNumber { get; private set; }
        public string Name { get; private set; }

        public CKPatchInfo( string name, int patchNumber )
        {
            PatchNumber = patchNumber;
            Name = name;
        }
    }

    public enum CKPatchStatus
    {
        NotInstalled = 0,
        Error = 1,
        Installed = 2
    }
}
