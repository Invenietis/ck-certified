using System;
using System.IO;
using System.Reflection;
using CK.Context;
using CK.Plugin;

namespace Host
{
    /// <summary>
    /// Singleton host. Its private constructor is safe (no exceptions can be 
    /// thrown except an out of memory: we can safely ignore this pathological case).
    /// </summary>
    public class CivikeyStandardHost : StandardContextHost
    {
        Version _version;
        bool _firstApplySucceed;

        /// <summary>
        /// Gets the current version of the Civikey-Standard application.
        /// It is stored in the system configuration file and updated by the installer.
        /// </summary>
        public Version Version
        {
            get { return _version ?? (_version = new Version( (string)SystemConfig.GetOrSet( "Version", "2.5" ) )); }
        }

        /// <summary>
        /// Note that the subAppName ("2.5") is just a way to have another folder 
        /// to isolate a big version if needed once.
        /// It is not really used currently.
        /// </summary>
        private CivikeyStandardHost()
            : base( "Civikey-Standard", "2.5" )
        {
        }

        /// <summary>
        /// Singleton instance.
        /// </summary>
        static readonly public CivikeyStandardHost Instance = new CivikeyStandardHost();

        public override IContext CreateContext()
        {
            IContext ctx = base.CreateContext();

            // Discover available plugins.
            string pluginPath = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "Plugins" );
            if( Directory.Exists( pluginPath ) ) ctx.PluginRunner.Discoverer.Discover( new DirectoryInfo( pluginPath ), true );

            RequirementLayer hostRequirements = new RequirementLayer( "CivikeyStandardHost" );
            hostRequirements.PluginRequirements.AddOrSet( new Guid( "{D46A46BF-69ED-4762-AB49-3340E4AEBBEB}" ), RunningRequirement.MustExistAndRun );

            ctx.PluginRunner.Add( hostRequirements );

            // Load or initialize the ctx.
            Instance.LoadContext();

            _firstApplySucceed = Context.PluginRunner.Apply();
            return ctx;
        }
    }
}
