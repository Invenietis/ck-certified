#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor.Sybille\SybilleWordPredictorService.cs) is part of CiviKey. 
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
using System.IO;
using CK.Context;
using CK.Plugin;
using CK.WordPredictor.Engines;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    [Plugin( "{1764F522-A9E9-40E5-B821-25E12D10DC65}", PublicName = "Sybille", Categories = new[] { "Prediction" } , Version="1.0")]
    public class SybilleWordPredictorService : WordPredictorServiceBase
    {
        IWordPredictorEngineFactory _engineFactory;

        [RequiredService]
        public IContext Context { get; set; }

        Func<string> _userPath;

        internal Func<string> UserPath
        {
            get
            {
                if( _userPath == null )
                {
                    return () =>
                    {
                        Uri userUri = Context.ConfigManager.SystemConfiguration.CurrentUserProfile.Address;
                        
                        return Path.GetDirectoryName( userUri.LocalPath );
                    };
                }
                return _userPath;
            }
            set
            {
                _userPath = value;
            }
        }

        protected override IWordPredictorEngineFactory EngineFactory
        {
            get
            {
                return _engineFactory ?? (_engineFactory = new SybilleWordPredictorEngineFactory( PluginDirectoryPath, UserPath, Feature ));
            }
        }
    }
}
