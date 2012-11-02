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
            : base( "{1764F522-A9E9-40E5-B821-25E12D10DC65}", R.WordPredictionConfig, app )
        {
        }

        public int MaxSuggestedWords
        {
            get { return Config != null ? Config.GetOrSet( "MaxSuggestedWords", 10 ) : 10; }
            set
            {
                if( Config != null ) Config.Set( "MaxSuggestedWords", value );
            }
        }

        protected override void NotifyOfPropertiesChange()
        {
            base.NotifyOfPropertiesChange();
            NotifyOfPropertyChange( () => MaxSuggestedWords );
        }

        protected override void OnInitialize()
        {
            var g = this.AddActivableSection( R.WordPredictionSectionName, R.WordPredictionConfig );

            var c = new ConfigItemProperty<int>( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, e => e.MaxSuggestedWords ) );
            c.DisplayName = R.WordPredictionMaxSuggestedWords;
            g.Items.Add( c );
            
            base.OnInitialize();
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            NotifyOfPropertyChange( () => MaxSuggestedWords );
        }
    }
}
