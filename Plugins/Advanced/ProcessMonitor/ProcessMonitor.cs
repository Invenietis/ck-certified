using System;
using System.Diagnostics;
using System.Management;
using CK.Plugin;
using CommonServices;

namespace ProcessMonitor
{
    /// <summary>
    /// Plugin that monitors the starting and stopping of processes through WMI, and the process of the foreground window when it changes.
    /// </summary>
    [Plugin( ProcessMonitor.PluginGuidString, PublicName = ProcessMonitor.PluginName, Version = ProcessMonitor.PluginVersion,
        Categories = new string[] { "Advanced" },
        Description = "Event-based process monitor. Watches the system for when a process starts and stops, anf for the process of the foreground window changes." )]
    public class ProcessMonitor : IProcessMonitor, IPlugin
    {
        public const string PluginGuidString = "{DA526BD2-75D2-4C4F-AAD6-E6FC815F49F5}";
        public static readonly Guid PluginGuid = new Guid( PluginGuidString );
        public const string PluginName = "ProcessMonitor";
        public const string PluginVersion = "1.0.0";

        public event EventHandler<ProcessEventArgs> ProcessStarted;
        public event EventHandler<ProcessEventArgs> ProcessStopped;
        public event EventHandler<ProcessEventArgs> ForegroundProcessChanged;

        ManagementEventWatcher _processStartEventWatcher;
        ManagementEventWatcher _processStopEventWatcher;
        ActiveWindowProcessMonitor _activeWindowMonitor;

        public ProcessMonitor()
        {
        }

        public bool Setup( IPluginSetupInfo info )
        {
            _processStartEventWatcher = new ManagementEventWatcher( new WqlEventQuery( "SELECT * FROM Win32_ProcessStartTrace" ) );
            _processStopEventWatcher = new ManagementEventWatcher( new WqlEventQuery( "SELECT * FROM Win32_ProcessStopTrace" ) );

            _processStartEventWatcher.EventArrived += new EventArrivedEventHandler( delegate( object sender, EventArrivedEventArgs e )
            {
                OnProcessStart( Convert.ToInt32( e.NewEvent.Properties["ProcessID"].Value ) );
            } );
            _processStopEventWatcher.EventArrived += new EventArrivedEventHandler( delegate( object sender, EventArrivedEventArgs e )
            {
                OnProcessStop( e.NewEvent.Properties["ProcessName"].Value.ToString() );
            } );

            return true;
        }

        public void Start()
        {
            _processStartEventWatcher.Start();
            _processStopEventWatcher.Start();

            _activeWindowMonitor = new ActiveWindowProcessMonitor();
            _activeWindowMonitor.ActiveWindowProcessChanged += _activeWindowMonitor_ActiveWindowProcessChanged;
        }

        void _activeWindowMonitor_ActiveWindowProcessChanged( object sender, ActiveWindowProcessEventArgs e )
        {
            if( ForegroundProcessChanged != null )
            {
                var args = new ProcessEventArgs( e.ActiveWindowProcess );
                ForegroundProcessChanged( this, args );
            }
        }

        public void Stop()
        {
            _activeWindowMonitor.ActiveWindowProcessChanged -= _activeWindowMonitor_ActiveWindowProcessChanged;
            _activeWindowMonitor.Dispose();
            _activeWindowMonitor = null;

            _processStartEventWatcher.Stop();
            _processStopEventWatcher.Stop();
        }

        public void Teardown()
        {
            _processStartEventWatcher.Dispose();
            _processStopEventWatcher.Dispose();

            _processStartEventWatcher = null;
            _processStopEventWatcher = null;
        }

        void OnProcessStart( int processId )
        {
            if( ProcessStarted != null )
            {
                Process p = Process.GetProcessById( processId );
                var args = new ProcessEventArgs( p );
                ProcessStarted( this, args );
            }
        }

        void OnProcessStop( string processName )
        {
            if( ProcessStopped != null )
            {
                var args = new ProcessEventArgs( processName );
                ProcessStopped( this, args );
            }
        }
    }
}
