using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using CK.WPF.ViewModel;
using CK.Plugin;

namespace CK.Plugins.ObjectExplorer.ViewModels.LogViewModels
{
    public class VMLogOutputContainer : VMBase
    {
        public VMLogOutputContainer()
        {
            _counter = 0;
            _maxCount = DEFAULTMAXCOUNT;
            Entries = new ObservableCollection<VMOutputLogEntry>();
            Categories = new ObservableCollection<VMLogOutputCategory>();
        }

        public ObservableCollection<VMOutputLogEntry> Entries { get; private set; }
        public ObservableCollection<VMLogOutputCategory> Categories { get; private set; }
        ICommand _clearOutputConsoleCommand;
        const int DEFAULTMAXCOUNT = 200;
        int _maxCount;
        int _counter;

        /// <summary>
        /// Gets or sets the maximum number of entries that can be displayed.
        /// Once this number is reached, the container starts discarding the older entries
        /// </summary>
        public int MaxCount { get { return _maxCount; } set { _maxCount = value; } }

        public void Add( LogEventArgs e, string message )
        {
            Add( new VMOutputLogEntry( this, e, message, ++_counter ) );
        }

        public bool IsCategoryFiltered( string category )
        {
            foreach( VMLogOutputCategory cat in Categories )
            {
                if( cat.Name == category )
                    return cat.IsVisible;
            }
            return false;
        }

        public void PropagateVisibilityChanged( VMLogOutputCategory category )
        {
            foreach( VMOutputLogEntry entry in Entries )
            {
                if( entry.Category == category.Name )
                    entry.NotifyVisibilityChanged();
            }
        }

        public void Add( VMOutputLogEntry entry )
        {
            while( Entries.Count >= _maxCount )
                Entries.RemoveAt( 0 );

            bool exists = false;
            foreach( VMLogOutputCategory category in Categories )
            {
                if( category.Name == entry.Category )
                {
                    exists = true;
                    break;
                }
            }

            if( !exists )
                Categories.Add( new VMLogOutputCategory( this, entry.Category ) );

            Entries.Add( entry );
            OnPropertyChanged( "LogEntriesContainer" );
        }

        public void Clear()
        {
            Entries.Clear();
        }

        public ICommand ClearOutputConsoleCommand
        {
            get
            {
                if( _clearOutputConsoleCommand == null )
                {
                    _clearOutputConsoleCommand = new VMCommand( () =>
                    {
                        Clear();
                    } );
                }
                return _clearOutputConsoleCommand;
            }
        }
    }

}
