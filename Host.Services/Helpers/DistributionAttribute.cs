using System;

namespace Host.Services.Helpers
{
    [AttributeUsage( AttributeTargets.Assembly )]
    public class DistributionAttribute : Attribute
    {
        public string DistributionName { get; private set; }
        public DistributionAttribute() : this( string.Empty ) { }
        public DistributionAttribute( string distributionName ) { DistributionName = distributionName; }

    }
}
