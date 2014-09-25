#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\ThirdLevel\WordPredictionViewModel.cs) is part of CiviKey. 
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
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    public class WordPredictionViewModel : ConfigBase
    {
        AppViewModel _app;
        IList<IPluginProxy> _wordPredictionPlugins;
        IList<Guid> _wordPredictionPluginIds;

        public WordPredictionViewModel( AppViewModel app )
            : base( "{4DC42B82-4B29-4896-A548-3086AA9421D7}", R.WordPredictionConfig, app )
        {
            _app = app;
            _wordPredictionPlugins = new List<IPluginProxy>();
            _wordPredictionPluginIds = new List<Guid>();

            _wordPredictionPluginIds.Add( new Guid( "{1756C34D-EF4F-45DA-9224-1232E96964D2}" ) );
            _wordPredictionPluginIds.Add( new Guid( "{1764F522-A9E9-40E5-B821-25E12D10DC65}" ) );
            _wordPredictionPluginIds.Add( new Guid( "{669622D4-4E7E-4CCE-96B1-6189DC5CD5D6}" ) );
            _wordPredictionPluginIds.Add( new Guid( "{4DC42B82-4B29-4896-A548-3086AA9421D7}" ) );
            _wordPredictionPluginIds.Add( new Guid( "{8789CDCC-A7BB-46E5-B119-28DC48C9A8B3}" ) );
            _wordPredictionPluginIds.Add( new Guid( "{69E910CC-C51B-4B80-86D3-E86B6C668C61}" ) );
            _wordPredictionPluginIds.Add( new Guid( "{86777945-654D-4A56-B301-5E92B498A685}" ) );
            _wordPredictionPluginIds.Add( new Guid( "{B2A76BF2-E9D2-4B0B-ABD4-270958E17DA0}" ) );
            _wordPredictionPluginIds.Add( new Guid( "{55C2A080-30EB-4CC6-B602-FCBBF97C8BA5}" ) );

            GetPlugins();
        }

        private void GetPlugins()
        {
            foreach( var pluginId in _wordPredictionPluginIds )
            {
                IPluginProxy pluginProxy = _app.PluginRunner.PluginHost.FindLoadedPlugin( pluginId, true );
                if( pluginProxy != null )
                {
                    _wordPredictionPlugins.Add( pluginProxy );
                }
            }
        }

        public int MaxSuggestedWords
        {
            get { return Config != null ? Config.GetOrSet( "WordPredictionMaxSuggestedWords", 5 ) : 5; }
            set
            {
                if( Config != null ) Config.Set( "WordPredictionMaxSuggestedWords", value );
            }
        }

        public bool FilterAlreadyShownWords
        {
            get { return Config != null ? Config.GetOrSet( "FilterAlreadyShownWords", false ) : true; }
            set
            {
                if( Config != null ) Config.Set( "FilterAlreadyShownWords", value );
            }
        }

        public bool InsertSpaceAfterPredictedWord
        {
            get { return Config != null ? Config.GetOrSet( "InsertSpaceAfterPredictedWord", true ) : true; }
            set
            {
                if( Config != null ) Config.Set( "InsertSpaceAfterPredictedWord", value );
            }
        }

        public bool UsesSemanticPrediction
        {
            get { return Config != null ? (string)Config["Engine"] == "sem-sybille" : false; }
            set
            {
                if( Config != null ) Config.Set( "Engine", value == true ? "sem-sybille" : "sybille" );
            }
        }

        public bool DisplayContextEditor
        {
            get { return Config != null ? Config.GetOrSet( "DisplayContextEditor", false ) : false; }
            set
            {
                if( Config != null ) Config.Set( "DisplayContextEditor", value );
            }
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {

            NotifyOfPropertyChange( () => MaxSuggestedWords );
            NotifyOfPropertyChange( () => InsertSpaceAfterPredictedWord );
            NotifyOfPropertyChange( () => UsesSemanticPrediction );
            NotifyOfPropertyChange( () => FilterAlreadyShownWords );
            NotifyOfPropertyChange( () => DisplayContextEditor );
        }

        protected override void OnInitialize()
        {
            var g = this.AddActivableSection( R.WordPredictionSectionName.ToLower(), R.WordPredictionConfig, this, h => h.ActivateWordPrediction, this );

            var c = new ConfigItemProperty<int>( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, e => e.MaxSuggestedWords ) );
            c.DisplayName = R.WordPredictionMaxSuggestedWords;
            g.Items.Add( c );

            var p = new ConfigItemProperty<bool>( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, e => e.InsertSpaceAfterPredictedWord ) );
            p.DisplayName = R.WordPredictionInsertSpace;
            g.Items.Add( p );

            var engine = new ConfigItemProperty<bool>( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, e => e.UsesSemanticPrediction ) );
            engine.DisplayName = R.WordPredictionUseSemanticPrediction;
            g.Items.Add( engine );

            var filter = new ConfigItemProperty<bool>( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, e => e.FilterAlreadyShownWords ) );
            filter.DisplayName = R.WordPredictionFilterAlreadySuggestedWord;
            g.Items.Add( filter );

            var contextEditor = new ConfigItemProperty<bool>( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, e => e.DisplayContextEditor ) );
            contextEditor.DisplayName = R.WordPredictionDisplayPredictionEditorWindow;
            g.Items.Add( contextEditor );

            base.OnInitialize();
        }

        public bool ActivateWordPrediction
        {
            get
            {
                if( _wordPredictionPlugins.Count == 0 ) GetPlugins();
                if( _wordPredictionPlugins.Count == 0 ) return false;
                foreach( var plugin in _wordPredictionPlugins )
                {
                    if( plugin.Status.IsStoppingOrStopped || plugin.Status.IsStoppedOrDisabled ) return false;
                }

                return true;
            }
            set
            {
                using( var waiting = _app.ShowBusyIndicator() )
                {
                    if( value )
                    {
                        foreach( var pluginId in _wordPredictionPluginIds )
                        {
                            _app.StartPlugin( pluginId );
                        }
                    }
                    else
                    {
                        foreach( var pluginId in _wordPredictionPluginIds )
                        {
                            _app.StopPlugin( pluginId );
                        }
                    }
                }

                NotifyOfPropertyChange( () => ActivateWordPrediction );
            }
        }
    }
}
