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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Reflection;
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
            _wordPredictionPluginIds.Add( new Guid( "{C78A5CC8-449F-4A73-88B4-A8CDC3D88534}" ) );

            GetPlugins();
        }

        IObjectPluginConfig _layoutConfig;
        public IObjectPluginConfig LayoutConfig
        {
            get
            {
                if( _layoutConfig == null )
                {
                    IPluginProxy pluginProxy = _app.PluginRunner.PluginHost.FindLoadedPlugin( new Guid( "{C78A5CC8-449F-4A73-88B4-A8CDC3D88534}" ), true );
                    if( pluginProxy != null )
                    {
                        _layoutConfig = _app.ConfigContainer.GetObjectPluginConfig( _app.CivikeyHost.Context.ConfigManager.UserConfiguration, pluginProxy );
                    }
                }
                return _layoutConfig;
            }
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

        public Color LetterColor
        {
            get { return LayoutConfig != null ? (Color)(LayoutConfig["LetterColor"] ?? Colors.Transparent) : Colors.Transparent; }
            set
            {
                if( LayoutConfig != null ) 
                {
                    if( value == Colors.Transparent )
                    {
                        LayoutConfig.Remove( "LetterColor" );
                        NotifyOfPropertyChange( () => LetterColor );
                    }
                    else LayoutConfig.Set( "LetterColor", value );
                }
            }
        }

        public Color BackgroundColor
        {
            get { return LayoutConfig != null ? (Color)(LayoutConfig["Background"] ?? Colors.Transparent) : Colors.Transparent; }
            set
            {
                if( LayoutConfig != null )
                {
                    if( value == Colors.Transparent )
                    {
                        LayoutConfig.Remove( "Background" );
                        NotifyOfPropertyChange( () => BackgroundColor );
                    }
                    else LayoutConfig.Set( "Background", value );
                }
            }
        }

        public Color HighlightBackgroundColor
        {
            get { return LayoutConfig != null ? (Color)(LayoutConfig["HighlightBackground"] ?? Colors.Transparent) : Colors.Transparent; }
            set
            {
                if( LayoutConfig != null )
                {
                    if( value == Colors.Transparent )
                    {
                        LayoutConfig.Remove( "HighlightBackground" );
                        NotifyOfPropertyChange( () => HighlightBackgroundColor );
                    }
                    else LayoutConfig.Set( "HighlightBackground", value );
                }
            }
        }

        public Color HighlightFontColor
        {
            get { return LayoutConfig != null ? (Color)(LayoutConfig["HighlightFontColor"] ?? Colors.Transparent) : Colors.Transparent; }
            set
            {
                if( LayoutConfig != null )
                {
                    if( value == Colors.Transparent )
                    {
                        LayoutConfig.Remove( "HighlightFontColor" );
                        NotifyOfPropertyChange( () => HighlightFontColor );
                    }
                    else LayoutConfig.Set( "HighlightFontColor", value );
                }
            }
        }

        public FontFamily FontFamily
        {
            get
            {
                if( LayoutConfig != null && LayoutConfig.Contains( "FontFamily" ) )
                {
                    if( ((string)(LayoutConfig["FontFamily"])).Contains( "pack://" ) )
                    {
                        string[] split = ((string)(LayoutConfig["FontFamily"])).Split( '|' );
                        return new FontFamily( new Uri( split[0] ), split[1] );
                    }
                    else
                    {
                        return new FontFamily( (string)(LayoutConfig["FontFamily"] ?? "Arial") );
                    }
                }
                return null;
            }
            set
            {
                if( LayoutConfig != null )
                {
                    if( value == null )
                    {
                        LayoutConfig.Remove( "FontFamily" );
                        NotifyOfPropertyChange( () => FontFamily );
                    }
                    else
                    {
                        if( value.BaseUri == null )
                        {
                            if( LayoutConfig != null ) LayoutConfig.Set( "FontFamily", value.ToString() );
                        }
                        else
                        {
                            if( LayoutConfig != null ) LayoutConfig.Set( "FontFamily", value.BaseUri.OriginalString + "|" + value.ToString() );
                        }
                    }
                }
            }
        }

        public double? FontSize
        {
            get { return LayoutConfig != null ? (double?)(LayoutConfig["FontSize"] ?? null) : null; }
            set
            {
                if( LayoutConfig != null )
                {
                    if( value == null )
                    {
                        LayoutConfig.Remove( "FontSize" );
                        NotifyOfPropertyChange( () => FontSize );
                    }
                    else LayoutConfig.Set( "FontSize", value );
                }
            }
        }

        public bool UseCustomLayout
        {
            get { return LayoutConfig != null ? LayoutConfig.GetOrSet( "UseCustomLayout", false ) : false; }
            set
            {
                if( LayoutConfig != null ) LayoutConfig.Set( "UseCustomLayout", value );
                NotifyOfPropertyChange( () => UseCustomLayout );
                UpdateCustomLayoutProperties();
            }
        }


        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            NotifyOfPropertyChange( () => MaxSuggestedWords );
            NotifyOfPropertyChange( () => InsertSpaceAfterPredictedWord );
            NotifyOfPropertyChange( () => UsesSemanticPrediction );
            NotifyOfPropertyChange( () => FilterAlreadyShownWords );
            NotifyOfPropertyChange( () => DisplayContextEditor );
            NotifyOfPropertyChange( () => LetterColor );
            NotifyOfPropertyChange( () => BackgroundColor );
            NotifyOfPropertyChange( () => HighlightBackgroundColor );
            NotifyOfPropertyChange( () => HighlightFontColor );
            NotifyOfPropertyChange( () => FontFamily );
            NotifyOfPropertyChange( () => FontSize );
            NotifyOfPropertyChange( () => UseCustomLayout );
        }

        private void UpdateCustomLayoutProperties()
        {
            NotifyOfPropertyChange( () => LetterColor );
            NotifyOfPropertyChange( () => BackgroundColor );
            NotifyOfPropertyChange( () => HighlightBackgroundColor );
            NotifyOfPropertyChange( () => HighlightFontColor );
            NotifyOfPropertyChange( () => FontFamily );
            NotifyOfPropertyChange( () => FontSize );
        }

        ComboBoxItem _comboBox;
        CancelConfigItem<string> _cancelFontSize;
        protected override void OnInitialize()
        {
            var g = this.AddActivableSection( R.WordPredictionSectionName.ToLower(), R.WordPredictionConfig, this, h => h.ActivateWordPrediction, this );

            var c = new ConfigItemProperty<int>( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, e => e.MaxSuggestedWords ) );
            c.DisplayName = R.WordPredictionMaxSuggestedWords;
            g.Items.Add( c );

            var p = new ConfigItemProperty<bool>( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, e => e.InsertSpaceAfterPredictedWord ) );
            p.DisplayName = R.WordPredictionInsertSpace;
            g.Items.Add( p );

            var engine = new ConfigItemProperty<bool>( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, e => e.UsesSemanticPrediction ) );
            engine.DisplayName = R.WordPredictionUseSemanticPrediction;
            g.Items.Add( engine );

            var filter = new ConfigItemProperty<bool>( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, e => e.FilterAlreadyShownWords ) );
            filter.DisplayName = R.WordPredictionFilterAlreadySuggestedWord;
            g.Items.Add( filter );

            var contextEditor = new ConfigItemProperty<bool>( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, e => e.DisplayContextEditor ) );
            contextEditor.DisplayName = R.WordPredictionDisplayPredictionEditorWindow;
            g.Items.Add( contextEditor );

            var useCutomLayout = g.AddActivableSection( R.CustomLayout.ToLower(), R.CustomLayout, this, h => h.UseCustomLayout, this );


            var letterColor = new ConfigItemProperty<Color>( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, e => e.LetterColor ) );
            letterColor.DisplayName = R.CustomFontColor;
            var cancelLetterColor = new CancelConfigItem<Color>( ConfigManager, letterColor, Colors.Transparent );
            useCutomLayout.Items.Add( cancelLetterColor );

            var backgroundColor = new ConfigItemProperty<Color>( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, e => e.BackgroundColor ) );
            backgroundColor.DisplayName = R.CustomBackground;
            var cancelBackgroundColor = new CancelConfigItem<Color>( ConfigManager, backgroundColor, Colors.Transparent );
            useCutomLayout.Items.Add( cancelBackgroundColor );

            var highlightBackgroundColor = new ConfigItemProperty<Color>( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, e => e.HighlightBackgroundColor ) );
            highlightBackgroundColor.DisplayName = R.CustomHighlightBackground;
            var cancelHighlightBackgroundColor = new CancelConfigItem<Color>( ConfigManager, highlightBackgroundColor, Colors.Transparent );
            useCutomLayout.Items.Add( cancelHighlightBackgroundColor );

            var highlightLetterColor = new ConfigItemProperty<Color>( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, e => e.HighlightFontColor ) );
            highlightLetterColor.DisplayName = R.CustomHighlightFontColor;
            var cancelHighlightLetterColor = new CancelConfigItem<Color>( ConfigManager, highlightLetterColor, Colors.Transparent );
            useCutomLayout.Items.Add( cancelHighlightLetterColor );

            var fontFamily = new ConfigItemProperty<FontFamily>( ConfigManager, this, ReflectionHelper.GetPropertyInfo( this, e => e.FontFamily ) );
            fontFamily.DisplayName = R.CustomFontFamily;
            var cancelFontFamily = new CancelConfigItem<FontFamily>( ConfigManager, fontFamily, null );
            useCutomLayout.Items.Add( cancelFontFamily );

            _comboBox = new ComboBoxItem( _app.ConfigManager, R.CustomFontSize, FontSizes );
            if( LayoutConfig != null && LayoutConfig.Contains( "FontSize" ) ) _comboBox.SelectedItem = LayoutConfig["FontSize"].ToString();
            _comboBox.SelectedItemChanged += comboBox_SelectedItemChanged;
            _cancelFontSize = new CancelConfigItem<string>( ConfigManager, _comboBox, ReflectionHelper.GetPropertyInfo( _comboBox, e => e.SelectedItem ), null );
            _cancelFontSize.ContentItem = _comboBox;
            useCutomLayout.Items.Add( _cancelFontSize );

            base.OnInitialize();
        }

        private void comboBox_SelectedItemChanged( object sender, SelectedItemChangedEventArgs e )
        {
            if( string.IsNullOrEmpty( e.Item ) ) FontSize = null;
            else FontSize = int.Parse( e.Item );
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

        IEnumerable<string> _sizes;
        IEnumerable<string> GetSizes( int from, int to )
        {
            for( int i = from; i <= to; i++ ) yield return i.ToString();
        }
        public IEnumerable<string> FontSizes { get { return _sizes ?? (_sizes = GetSizes( 10, 50 )); } }
    }
}
