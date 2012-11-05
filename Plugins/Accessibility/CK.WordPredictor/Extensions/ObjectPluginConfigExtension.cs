using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Config;

namespace CK.WordPredictor
{
    public static class ObjectPluginConfigExtension
    {
        public static T TryGet<T>( this IObjectPluginConfig config, string key, T defaultValue )
        {
            if( config.Contains( key ) )
                return (T)config[key];

            return defaultValue;
        }
    }
}
