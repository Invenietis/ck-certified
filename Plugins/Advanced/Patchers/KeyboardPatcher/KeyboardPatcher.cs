#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Patchers\KeyboardPatcher\KeyboardPatcher.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public delegate T CKFunc<T, O, N>( out O errorMessage, out N noRetry );

    /// <summary>
    /// This plugin hosts keyboard updates.
    /// Each patch's state is saved in the User configuration, so it will be applied on each user individually.
    /// 
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
            _patchs.Add( new CKPatch( "KeyboardPatch", 1, R.Patch1Desc, CanExecutePatchV1, ApplyPatchV1, true ) );

            //There shouldn't be two patches with the same name
            Debug.Assert( _patchs.Select( x => x.Info.PatchNumber ).Distinct().ToArray().Length == _patchs.Count );
            
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
                        Config.User.Set( patch.Info.ConfPathString, CKPatchStatus.Installed );
                    }

                    if( stopReminder )
                    {
                        Config.User.Set( patch.Info.ConfPathString, CKPatchStatus.Error );
                        Config.User.Set( patch.Info.ConfPathErrorString, errorMessage );
                    }
                }
            }
        }

        public void Stop()
        {

        }

        public void Teardown()
        {
            _patchs = null;
        }

        //PatchV1 adds a button designed to toggle the host's window's visibility.
        #region PatchV1

        public bool CanExecutePatchV1( out string errorMessage, out bool noRetry )
        {
            noRetry = false;
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
                        if( EditedConfiguration == null )
                        {
                            errorMessage = R.SkinConfigurationNull;
                            return false;
                        }
                        return true;
                    }
                    else errorMessage = String.Format( R.MinimizeHostKeyAlreadyAdded, DateTime.Now );
                }
                else errorMessage = String.Format( R.AzertyKeyboardNotFound, DateTime.Now );
            }
            noRetry = true;
            return false;
        }

        public void ApplyPatchV1()
        {
            string zoneName = "CustomFunctions";
            string keyboardName = "Azerty";

            IKeyboard keyboard = KeyboardContext.Service.Keyboards[keyboardName];

            #region Adding descriptions
            {
                IKeyCollection keys = keyboard.Zones[zoneName].Keys;
                IKeyMode keyMode = keys.Select( k => k.KeyModes.FindBest( KeyboardContext.Service.EmptyMode ) ).Where( km => km.UpLabel == "Exit" ).FirstOrDefault();
                if( keyMode != null )
                {
                    keyMode.Description = R.Patch1CloseCiviKey;
                }

                keyMode = keys.Select( k => k.KeyModes.FindBest( KeyboardContext.Service.EmptyMode ) ).Where( km => km.UpLabel == "Hide" ).FirstOrDefault();
                if( keyMode != null )
                {
                    keyMode.Description = R.Patch1Hide;
                }
            }
            #endregion

            #region Adding MinimizeHost
            {
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

        #endregion

    }

    /// <summary>
    /// Represents a CiviKey patch. (for example used to update the Keyboard Context)
    /// </summary>
    public class CKPatch
    {
        public CKFunc<bool, string, bool> CanExecute { get; private set; }
        public Func<bool> IsCorrectlyApplied { get; private set; }
        public bool ShouldAskUser { get; private set; }
        public CKPatchInfo Info { get; private set; }
        public Action Action { get; private set; }

        /// <summary>
        /// Gets the message prompted tp the user when he is proposed the patch.
        /// The dialog will propose Yes & No as possible answers.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This method asks the user is he wants to apply the patch (if <see cref="ShouldAskUser"/> is set to True)
        /// It then launches the CanExecute method, to check whether the system is in the right state to apply this patch.
        /// If so, launched the Action method, that applies the patch.
        /// At the end, launched IsCorrectlyApplied to check whether the patch has been correctly applied (optional).
        /// </summary>
        /// <param name="errorMessage">the errormessage if there are errors</param>
        /// <param name="noRetry">gets whether something heappened in this method, that asks not to retry applying this patch</param>
        /// <returns></returns>
        public bool TryApplyPatch( out string errorMessage, out bool noRetry )
        {
            errorMessage = String.Empty;
            noRetry = false;
            string desc = String.IsNullOrWhiteSpace( Description ) ? R.KeyboardUpdateDescription : Description;

            ModalViewModel mvm = new ModalViewModel( R.KeyboardUpdateTitle, desc, true, R.RememberMyDecision );
            mvm.Buttons.Add( new ModalButton( mvm, R.Yes, null, ModalResult.Yes ) );
            mvm.Buttons.Add( new ModalButton( mvm, R.No, null, ModalResult.No ) );
            if( ShouldAskUser )
            {
                CustomMsgBox msgBox = new CustomMsgBox( ref mvm );
                msgBox.ShowDialog();
            }

            if( !ShouldAskUser || mvm.ModalResult == ModalResult.Yes )
            {
                if( CanExecute == null || CanExecute( out errorMessage, out noRetry ) )
                {
                    if( Action != null )
                        Action.Invoke();

                    if( IsCorrectlyApplied == null || IsCorrectlyApplied() )
                        return true;
                }
            }
            else
            {
                errorMessage = String.Format( R.UserCancelled, DateTime.Now );
            }

            if( mvm.IsCheckboxChecked )
                noRetry = true;

            return false;
        }

        public CKPatch( string name, int patchNumber, CKFunc<bool, string, bool> canExecute, Action action, bool shouldAskUser )
            : this( name, patchNumber, canExecute, action, null, shouldAskUser )
        {
        }

        public CKPatch( string name, int patchNumber, string description, CKFunc<bool, string, bool> canExecute, Action action, bool shouldAskUser )
            : this( name, patchNumber, description, canExecute, action, null, shouldAskUser )
        {
        }

        public CKPatch( string name, int patchNumber, CKFunc<bool, string, bool> canExecute, Action action, Func<bool> isCorrectlyApplied, bool shouldAskUser )
            : this( name, patchNumber, "", canExecute, action, null, shouldAskUser )
        {
        }

        public CKPatch( string name, int patchNumber, string description, CKFunc<bool, string, bool> canExecute, Action action, Func<bool> isCorrectlyApplied, bool shouldAskUser )
        {
            Info = new CKPatchInfo( name, patchNumber );
            IsCorrectlyApplied = isCorrectlyApplied;
            ShouldAskUser = shouldAskUser;
            Description = description;
            CanExecute = canExecute;
            Action = action;
        }
    }

    public class CKPatchInfo : IComparable<CKPatchInfo>
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

        public int CompareTo( CKPatchInfo other )
        {
            if( this.PatchNumber > other.PatchNumber ) return 1;
            else if( this.PatchNumber == other.PatchNumber ) return 0;
            return -1;
        }
    }

    public enum CKPatchStatus
    {
        NotInstalled = 0,
        Error = 1,
        Installed = 2
    }
}
