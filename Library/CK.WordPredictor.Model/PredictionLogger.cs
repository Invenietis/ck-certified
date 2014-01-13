using CK.Core;

namespace CK.WordPredictor
{
    public static class PredictionLogger
    {
        static DefaultActivityLogger _logger;

        public static IActivityLogger Instance
        {
            get
            {
                if( _logger == null )
                {
                    _logger = new DefaultActivityLogger();
                    _logger.Tap.Register( new ActivityLoggerConsoleSink() );
                }
                return _logger;
            }

        }
    }
}
