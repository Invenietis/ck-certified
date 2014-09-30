#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\ViewModels\KeyboardEditionViewModel.cs) is part of CiviKey. 
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
using System.Windows.Threading;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.WPF.Wizard;
using KeyboardEditor.Resources;

namespace KeyboardEditor.ViewModels
{
    public class KeyboardEditionViewModel : HelpAwareWizardPage
    {
        DispatcherTimer _autoSaveContextTimer;

        public VMContextEditable EditedContext { get { return Root.EditedContext; } }

        public KeyboardEditionViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IKeyboard editedKeyboard )
            : base( root, wizardManager, false )
        {
            Root.EditedContext = new VMContextEditable( root, editedKeyboard, Root.Config, root.SkinConfiguration );
            Next = new SavingStepViewModel( Root, WizardManager, EditedContext.KeyboardVM.Model );

            Title = String.Format( R.KeyboardEditionStepTitle, editedKeyboard.Name );
            Description = R.KeyboardEditionStepDesc;

            _autoSaveContextTimer = new DispatcherTimer( DispatcherPriority.Background );
            _autoSaveContextTimer.Interval = new TimeSpan( 0, 0, 30 );
        }

        /// <summary>
        /// allows the auto save when the keyboard editor is open to prevent losing the changes in case of crash.
        /// </summary>
        void OnTimerTick( object sender, EventArgs e )
        {
            Root.Save();
        }

        public override bool OnBeforeNext()
        {
            _autoSaveContextTimer.Tick -= OnTimerTick;
            if( _autoSaveContextTimer.IsEnabled ) _autoSaveContextTimer.Stop();

            HashSet<string> protocols = new HashSet<string>();

            //Parse all the keys of the keyboard ot retrieve the different CommandManagers it needs
            foreach( var zone in EditedContext.KeyboardVM.Model.Zones )
            {
                foreach( var key in zone.Keys )
                {
                    foreach( var keyMode in key.KeyModes )
                    {
                        ParseCommands( protocols, keyMode.OnKeyUpCommands );
                        ParseCommands( protocols, keyMode.OnKeyDownCommands );
                        ParseCommands( protocols, keyMode.OnKeyPressedCommands );
                    }
                }

            }

            //Updating the keyboard's requirement layer
            foreach( var protocol in protocols )
            {
                Type handlingService = Root.ProtocolManagerService.Service.ProtocolEditorsProviderViewModel.GetHandlingService(protocol);
                if( handlingService != null )
                {
                    Root.EditedContext.KeyboardVM.Model.RequirementLayer.ServiceRequirements.AddOrSet( handlingService.AssemblyQualifiedName, RunningRequirement.OptionalTryStart );
                }
            }

            return base.OnBeforeNext();
        }

        private static void ParseCommands( HashSet<string> protocols, IKeyProgram keyProgram )
        {
            foreach( var command in keyProgram.Commands )
            {
                int index = command.IndexOf( ':' );
                if( index >= 0 )
                    protocols.Add( command.Substring( 0, index ) );
            }
        }

        public override bool OnBeforeGoBack()
        {
            _autoSaveContextTimer.Tick -= OnTimerTick;
            if( _autoSaveContextTimer.IsEnabled ) _autoSaveContextTimer.Stop();

            return base.OnBeforeGoBack();
        }

        public override bool OnActivating()
        {
            _autoSaveContextTimer.Tick += OnTimerTick;
            _autoSaveContextTimer.Start();

            return base.OnActivating();
        }

        object _selectedHolder;
        // Used by the binding
        public object SelectedHolder
        {
            get { return _selectedHolder; }
            set
            {
                _selectedHolder = value;
                Refresh();
            }
        }

        // Used to find the config
        internal IKeyboardElement ConfigHolder
        {
            get
            {
                string holderType = _selectedHolder.GetType().ToString();
                switch( holderType )
                {
                    case "VMKeyboardEditable":
                        return ( _selectedHolder as VMKeyboardEditable ).Model;
                    case "VMZoneEditable":
                        return ( _selectedHolder as VMZoneEditable ).Model;
                    case "VMKeyEditable":
                        return ( _selectedHolder as VMKeyEditable ).Model;
                    default:
                        break;
                }
                return null;
            }
        }
    }
}
