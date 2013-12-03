using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenScroller
{
    public class NodeViewModel : INotifyPropertyChanged
    {
        bool _isRoot;
        int _level, _index;

        public bool IsRoot { get { return _isRoot; } }
        public bool IsStart { get { return Index == 0; } }
        public bool IsEnd { get { return Index == ChildNodes.Count - 1; } }
        public bool IsContinuousTrack { get { return ChildNodes.Count == 4; } }

        public int CurrentIndex { get; set; }
        public int Index { get { return _index; } }

        public int Level { get { return _level; } }
        public bool CurrentLevelIsBeingScrolled { get { return Root.CurrentNode == Parent; } }

        public int LapCount { get { return _lapCount; } }
        public bool LapsAreFinished { get { return LapCount >= Root.MaxLapCount; } }
        public bool ParentLapsAreAboutToFinish { get { return Parent.LapCount >= Root.MaxLapCount - 1; } }

        public IRootNode Root { get; set; }
        public NodeViewModel Parent { get; set; }
        public ObservableCollection<NodeViewModel> ChildNodes { get; set; }

        private NodeViewModel( IRootNode root, NodeViewModel parent, int level, int childrenCount, int maxDepth, int index )
        {
            ChildNodes = new ObservableCollection<NodeViewModel>();
            Parent = parent;
            Root = root;
            Root.LevelChanged += OnLevelChanged;

            _level = level;
            _index = index;

            if( _level < maxDepth )
            {
                for( int i = 0; i < childrenCount; i++ )
                {
                    ChildNodes.Add( new NodeViewModel( root, this, _level + 1, childrenCount, maxDepth, i ) );
                }
            }
        }

        public void Dispose()
        {
            Root.LevelChanged -= OnLevelChanged;
        }

        void OnLevelChanged( object sender, LevelChangedEventArgs e )
        {
            OnPropertyChanged( "CurrentLevelIsBeingScrolled" );
            OnPropertyChanged( "ParentLapsAreAboutToFinish" );
        }

        internal NodeViewModel( ScreenScrollerPlugin root, int childrenCount, int maxDepth, bool isRoot )
        {
            ChildNodes = new ObservableCollection<NodeViewModel>();
            Parent = root;
            Root = root;
            _level = 0;

            if( _level < maxDepth )
            {
                for( int i = 0; i < childrenCount; i++ )
                {
                    ChildNodes.Add( new NodeViewModel( root, this, _level + 1, childrenCount, maxDepth, i ) );
                }
            }
        }

        internal NodeViewModel()
        {
            _isRoot = true;
        }

        int _lapCount;
        bool _childOrSelfIsActive;
        bool _hasJustBeenEntered;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>false if the node has made its final lap during the last MoveNext.</returns>
        internal bool MoveNext()
        {
            if( LapsAreFinished )
            {
                _lapCount = 0;
                CurrentIndex = 0;
                IsVisible = false;
                return false;
            }

            if( _hasJustBeenEntered )
            {
                _hasJustBeenEntered = false;
                IsHighlighted = false;
            }
            else
            {
                if( CurrentIndex == ChildNodes.Count - 1 ) //If we are at the end of a lap, we set the 
                {
                    CurrentIndex = 0;
                    _lapCount++;

                    ChildNodes.ElementAt( 0 ).LapCompleted();
                    OnPropertyChanged( "LapsAreFinished" );
                }
                else CurrentIndex++;
            }

            ChildNodes.ElementAt( CurrentIndex ).IsHighlighted = true;

            return true;
        }

        internal void LapCompleted()
        {
            OnPropertyChanged( "ParentLapsAreAboutToFinish" );
        }

        internal void Entered()
        {
            _hasJustBeenEntered = true;
            IsVisible = true;
        }

        internal void ExitAll()
        {
            CurrentIndex = 0;
            _lapCount = 0;
            IsVisible = false;
            foreach( var node in ChildNodes )
            {
                node.ExitAll();
            }

            OnPropertyChanged( "ParentLapsAreAboutToFinish" );
        }

        public bool IsVisible
        {
            get { return _childOrSelfIsActive; }
            set
            {
                _childOrSelfIsActive = value;
                OnPropertyChanged( "IsVisible" );
            }
        }

        string _image;
        public string Image
        {
            get
            {
                if( String.IsNullOrWhiteSpace( _image ) )
                {
                    double colCount, rowCount;

                    colCount = rowCount = Math.Sqrt( ChildNodes.Count );
                    double val = ( Index + 1 ) / rowCount;
                    double truncatedValue = Math.Truncate( val );
                    bool isTruncatedValueEven = ( truncatedValue % 2 ) == 0;

                    if( ChildNodes.Count == 0 )
                    {
                        return "x";
                    }
                    else if( IsStart )
                    {
                        //We are on the first node
                        _image = ">";
                    }
                    else if( IsEnd )
                    {
                        if( IsContinuousTrack )
                        {
                            _image = "^";
                        }
                        else
                        {
                            _image = "END";
                        }
                    }
                    else if( val % 1 == 0 ) //if the value has nothing past the decimal point, we are at the end of a row (or at the beginning/end of the track)
                    {
                        //We are on the end of a row
                        _image = "v";
                    }
                    else if( isTruncatedValueEven )
                    {
                        //The next element is on the right of this element
                        _image = ">";
                    }
                    else
                    {
                        //The next element is on the left of this element
                        _image = "<";
                    }
                }
                return _image;
            }
        }

        bool _isHighlighted;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;
                if( value )
                {
                    IsVisible = true;
                }
                OnPropertyChanged( "CurrentLevelIsBeingScrolled" );
                OnPropertyChanged( "IsHighlighted" );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged( string name )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( name ) );
        }
    }
}
