#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor\WordPredictorFeature.cs) is part of CiviKey. 
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

using System.ComponentModel;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion, Categories = new string[] { "Advanced", "Prediction" } )]
    public class WordPredictorFeature : IPlugin, IWordPredictorFeature
    {
        #region Plugin description

        const string PluginGuidString = "{4DC42B82-4B29-4896-A548-3086AA9421D7}";
        const string PluginVersion = "1.0.0";
        const string PluginPublicName = "WordPredictor Feature";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        private IKeyboardContextPredictionFactory _predictionContextFactory;
        private IKeyboardContextPredictionFactory _autonomousPredictionContextFactory;

        public event PropertyChangedEventHandler PropertyChanged;

        public IPluginConfigAccessor Config { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardContext Context { get; set; }

        public bool InsertSpaceAfterPredictedWord
        {
            get { return Config.User.TryGet( "InsertSpaceAfterPredictedWord", true ); }
        }

        public bool DisplayContextEditor
        {
            get { return Config.User.TryGet( "DisplayContextEditor", false ); }
        }

        public bool FilterAlreadyShownWords
        {
            get
            {
                return Config.User.TryGet( "FilterAlreadyShownWords", true );
            }
        }

        public int MaxSuggestedWords
        {
            get { return Config.User.GetOrSet( "WordPredictionMaxSuggestedWords", 5 ); }
        }

        public string Engine
        {
            get { return Config.User.TryGet( "Engine", "sem-sybille" ); }
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( PropertyChanged != null )
                PropertyChanged( this, new PropertyChangedEventArgs( e.Key ) );
        }

        public bool Setup( IPluginSetupInfo info )
        {
            Config.ConfigChanged += OnConfigChanged;
            return true;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
            Config.ConfigChanged -= OnConfigChanged;
        }

        public IKeyboardContextPredictionFactory PredictionContextFactory
        {
            get { return _predictionContextFactory ?? (_predictionContextFactory = new DefaultKeyboardContextPredictionFactory( Context, this )); }
            set { _predictionContextFactory = value; }
        }

        public IKeyboardContextPredictionFactory AutonomousKeyboardPredictionFactory
        {
            get { return _autonomousPredictionContextFactory ?? (_autonomousPredictionContextFactory = new AutonomousKeyboardPredictionFactory( Context, this )); }
            set { _autonomousPredictionContextFactory = value; }
        }

    }
}
