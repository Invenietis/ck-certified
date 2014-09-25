#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\TextTemplate\TextTemplateCommandHandler.cs) is part of CiviKey. 
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
using BasicCommandHandlers.Resources;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Plugins.SendInputDriver;
using CommonServices;
using ProtocolManagerModel;

namespace BasicCommandHandlers
{
    [Plugin( TextTemplateCommandHandler.PluginIdString,
           PublicName = PluginPublicName,
           Version = TextTemplateCommandHandler.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class TextTemplateCommandHandler : BasicCommandHandler, ITextTemplateCommandHandlerService
    {
        const string PluginIdString = "{78D84978-7A59-4211-BE04-DD25B5E2FDC1}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Text template Command Handler";

        const string PROTOCOL_BASE = "placeholder";
        const string PROTOCOL = PROTOCOL_BASE + ":";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IProtocolEditorsManager> ProtocolManagerService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITextTemplateService> TextTemplate { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ISendStringService> SendString { get; set; }

        [ConfigurationAccessor( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" )]
        public IPluginConfigAccessor SkinConfiguration { get; set; }

        public override void Start()
        {
            base.Start();
            ProtocolManagerService.Service.Register(
                                        new VMProtocolEditorMetaData(
                                        PROTOCOL_BASE,
                                        R.TextTemplateTitle,
                                        R.TextTemplateDescription,
                                        () => { return new TextTemplateCommandParameterManager( TextTemplate.Service, SendString.Service ); } ),
                                        typeof( ITextTemplateCommandHandlerService ) );
        }

        public override void Stop()
        {
            ProtocolManagerService.Service.Unregister( PROTOCOL_BASE );
            base.Stop();
        }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            Command cmd = new Command( e.Command );
            if( cmd.Name != PROTOCOL_BASE ) return;

            OpenEditor( cmd.Content );
        }

        #region ITextTemplateCommandHandlerService Members

        public void OpenEditor( string template )
        {
            TextTemplate.Service.OpenEditor( template );
        }

        #endregion
    }

    public class Command
    {
        static readonly string SeparationToken = ":";
        public string Name { get; private set; }
        public string Content { get; private set; }

        public Command( string cmd )
        {
            int pos = cmd.IndexOf( SeparationToken );
            if( pos < 0 ) return;

            Name = cmd.Substring( 0, pos );
            Content = cmd.Substring( pos + SeparationToken.Length );
        }
    }
}
