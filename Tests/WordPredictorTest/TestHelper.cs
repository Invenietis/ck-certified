using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Config;
using Moq;

namespace WordPredictorTest
{
    public class TestHelper
    {
        public static string SybilleResourceFullPath = @"F:\Users\Cedric\Documents\Dev\__Dev4\Civikey\ck-certified\Plugins\Accessibility\CK.WordPredictor\";

        public static Mock<IPluginConfigAccessor> MockPluginConfigAccessor()
        {
            var configAccessor = new Mock<IPluginConfigAccessor>();
            var pluginConfig = new Mock<IObjectPluginConfig>();
            configAccessor.Setup( e => e.User ).Returns( pluginConfig.Object );

            return configAccessor;
        }
    }
}
