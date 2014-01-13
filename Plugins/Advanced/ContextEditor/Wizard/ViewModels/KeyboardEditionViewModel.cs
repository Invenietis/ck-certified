using System;
using System.Collections.Generic;
using CK.Keyboard.Model;
using KeyboardEditor.Resources;
using CK.Plugin;

namespace KeyboardEditor.ViewModels
{
    public class KeyboardEditionViewModel : HelpAwareWizardPage
    {
        public VMContextEditable EditedContext { get { return Root.EditedContext; } }

        public KeyboardEditionViewModel( IKeyboardEditorRoot root, WizardManager wizardManager, IKeyboard editedKeyboard )
            : base( root, wizardManager, false )
        {
            Root.EditedContext = new VMContextEditable( root, editedKeyboard, Root.Config, root.SkinConfiguration );
            Next = new SavingStepViewModel( Root, WizardManager, EditedContext.KeyboardVM.Model );

            Title = String.Format( R.KeyboardEditionStepTitle, editedKeyboard.Name );
            Description = R.KeyboardEditionStepDesc;
        }

        public override bool OnBeforeNext()
        {
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
            return base.OnBeforeGoBack();
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
