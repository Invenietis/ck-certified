using CK.Core;

namespace CK.WordPredictor
{
    public static class PredictionLogger
    {
        static IActivityMonitor _logger;

        public static IActivityMonitor Instance
        {
            get
            {
                if( _logger == null )
                {
                    _logger = new ActivityMonitor();
                    _logger.Output.RegisterClient( new ActivityMonitorConsoleClient() );
                }
                return _logger;
            }

        }
    }
}
