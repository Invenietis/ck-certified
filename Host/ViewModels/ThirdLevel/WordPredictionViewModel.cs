using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Config;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    public class WordPredictionViewModel : ConfigBase
    {
        public WordPredictionViewModel( AppViewModel app )
            : base( "{4DC42B82-4B29-4896-A548-3086AA9421D7}", R.WordPredictionConfig, app )
        {
        }

        public int MaxSuggestedWords
        {
            get { return Config != null ? Config.GetOrSet( "MaxSuggestedWords", 5 ) : 5; }
            set
            {
                if( Config != null ) Config.Set( "MaxSuggestedWords", value );
            }
        }

        public bool FilterAlreadyShownWords
        {
            get { return Config != null ? Config.GetOrSet( "FilterAlreadyShownWords", true ) : true; }
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
            var g = this.AddActivableSection( R.WordPredictionSectionName.ToLower(), R.WordPredictionConfig );

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

            //Hidden, we'll wait for the feature to have less bugs
            //var contextEditor = new ConfigItemProperty<bool>( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, e => e.DisplayContextEditor ) );
            //contextEditor.DisplayName = R.WordPredictionDisplayPredictionEditorWindow;
            //g.Items.Add( contextEditor );


            base.OnInitialize();
        }

    }
}
