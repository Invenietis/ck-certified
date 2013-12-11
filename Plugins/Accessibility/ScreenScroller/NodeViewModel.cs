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
        public bool IsLeaf { get { return ChildNodes.Count == 0; } }

        public bool IsStart { get { return Index == 0; } }
        public bool IsEnd { get { return Index == ChildNodes.Count - 1; } }
        public bool IsContinuousTrack { get { return ChildNodes.Count == 4; } }

        /// <summary>
        /// Gets the index of the child currently scrolled on
        /// </summary>
        public int CurrentIndex { get; set; }
        /// <summary>
        /// Gets the index of this node among its parent's nodes
        /// </summary>
        public int Index { get { return _index; } }
        /// <summary>
        /// Gets the Level of this node
        /// </summary>
        public int Level { get { return _level; } }
        /// <summary>
        /// Gets whether this node or one of its siblings are being scrolled on (true if this element's parent is the current node)
        /// </summary>
        public bool IsParentTheCurrentNode { get { return Root.CurrentNode == Parent; } }
        /// <summary>
        /// Gets whether this Node is the current
        /// </summary>
        public bool IsCurrentNode { get { return Root.CurrentNode == this; } }
        /// <summary>
        /// Gets the number of laps done so far (should return 0 when the node is not the current one)
        /// </summary>
        public int LapCount { get { return _lapCount; } }
        /// <summary>
        /// Gets whether the number of laps of this node exceeds the Root's MaxLapCount. 
        /// Used to know whether we should go up one level
        /// </summary>
        public bool LapsAreFinished { get { return LapCount >= Root.MaxLapCount; } }
        /// <summary>
        /// Gets whether the lapcount of the parent is near to the Root's MaxLapCount.
        /// Used to warn the user that the next time we step on the starting cell of the track, we are going up one level
        /// </summary>
        public bool ParentLapsAreAboutToFinish { get { return Parent.LapCount >= Root.MaxLapCount - 1; } }

        public IRootNode Root { get; set; }
        public NodeViewModel Parent { get; set; }
        public ObservableCollection<NodeViewModel> ChildNodes { get; set; }

        /// <summary>
        /// Private constructor, used internally to create the childnodes of the root nodes.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        /// <param name="childrenCount"></param>
        /// <param name="maxDepth"></param>
        /// <param name="index"></param>
        private NodeViewModel( IRootNode root, NodeViewModel parent, int level, int childrenCount, int maxDepth, int index )
        {
            ChildNodes = new ObservableCollection<NodeViewModel>();
            Parent = parent;
            Root = root;
            Root.LevelChanged += OnLevelChanged;

            _level = level;
            _index = index;

            InitializeCoordinates( index );

            if( _level < maxDepth )
            {
                for( int i = 0; i < childrenCount; i++ )
                {
                    ChildNodes.Add( new NodeViewModel( root, this, _level + 1, childrenCount, maxDepth, i ) );
                }
            }
        }

        private void InitializeCoordinates( int index )
        {
            Row = (int)Math.Truncate( (double)( ( index ) / Root.SquareSize ) );

            while( index >= Root.SquareSize )
            {
                index = index - Root.SquareSize;
            }

            if( Row != 0 && Row % 2 != 0 )
                Column = Root.SquareSize - index - 1;
            else //if we are on an odd row number, columns are reversed (see the TrackUniformGrid for more information)
                Column = index;
        }

        public double OffsetHeight { get { return IsRoot ? Height * Row : ( Height * Row ) + Parent.OffsetHeight; } }
        public double OffsetWidth { get { return IsRoot ? Width * Column : ( Width * Column ) + Parent.OffsetWidth; } }

        /// <summary>
        /// Ctor of the root nodes (one for each screen)
        /// </summary>
        /// <param name="root"></param>
        /// <param name="childrenCount"></param>
        /// <param name="maxDepth"></param>
        /// <param name="isRoot"></param>
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

        /// <summary>
        /// Cotor used by the absolute root of the tree (the plugin)
        /// </summary>
        internal NodeViewModel()
        {
            _isRoot = true;
        }

        public void Dispose()
        {
            Root.LevelChanged -= OnLevelChanged;
        }

        void OnLevelChanged( object sender, LevelChangedEventArgs e )
        {
            OnPropertyChanged( "ParentLapsAreAboutToFinish" );
            OnPropertyChanged( "IsParentTheCurrentNode" );
            OnPropertyChanged( "IsCurrentNode" );
            OnPropertyChanged( "Image" );
        }

        int _lapCount;
        bool _childOrSelfIsActive;
        bool _hasJustBeenEntered;

        /// <summary>
        /// Highlights the next childnode of this node.
        /// Returns false if the node has made its final lap during the last MoveNext.
        /// </summary>
        /// <returns>false if the node has made its final lap during the last MoveNext. (its state is then completely flushed)</returns>
        internal bool MoveNext()
        {
            if( ChildNodes.Count == 0 ) return false;

            if( ChildNodes.Count > 1 )
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
                    IsHighlighted = false; //Removing the highlight (the element is still visible, since its children need to be shown)
                }
                else
                {
                    if( CurrentIndex == ChildNodes.Count - 1 )
                    {
                        CurrentIndex = 0;
                        _lapCount++;

                        ChildNodes.ElementAt( 0 ).LapCompleted();
                        OnPropertyChanged( "LapsAreFinished" );
                    }
                    else CurrentIndex++;
                }
            }

            ChildNodes.ElementAt( CurrentIndex ).IsHighlighted = true;

            return true;
        }

        internal void LapCompleted()
        {
            OnPropertyChanged( "ParentLapsAreAboutToFinish" );
            OnPropertyChanged( "IsParentLastTick" );
            OnPropertyChanged( "Image" );
        }

        /// <summary>
        /// Called when this node is entered : when it becomes the currentnode.
        /// If one of the children is selected and then the cursor goes up one level, this node keeps its current state (its lap count and whether or not the GoUpOneLevel image is animating)
        /// </summary>
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
            OnPropertyChanged( "IsParentTheCurrentNode" );
            OnPropertyChanged( "IsParentLastTick" );
            OnPropertyChanged( "LapsAreFinished" );
            OnPropertyChanged( "IsHighlighted" );
            OnPropertyChanged( "IsCurrentNode" );
            OnPropertyChanged( "Image" );
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

        ResourceDictionary _resourceDictionary = new ResourceDictionary()
        {
            Source = new Uri( "pack://application:,,,/ScreenScroller;component/Views/Paths.xaml", UriKind.Absolute )
        };

        /// <summary>
        /// Gets whether the parent was scrolling on its children and has finished its last lap/
        /// Returns true if this is the absolute last tick of the aprent before going up
        /// </summary>
        public bool IsParentLastTick { get { return CurrentIndex == 0 && Parent.LapCount == Root.MaxLapCount; } }

        object _image;
        public object Image
        {
            get
            {
                if( IsParentLastTick )
                {
                    return _resourceDictionary["OutArrow"];
                }

                if( _image == null || LapsAreFinished )
                {
                    double colCount, rowCount;

                    colCount = rowCount = Math.Sqrt( ChildNodes.Count );
                    double val = ( Index + 1 ) / rowCount;
                    double truncatedValue = Math.Truncate( val );
                    bool isTruncatedValueEven = ( truncatedValue % 2 ) == 0;

                    if( IsLeaf )
                    {
                        //We are on a leaf, we display a "click" image
                        _image = _resourceDictionary["ClickPath"];
                    }
                    else if( IsStart )
                    {
                        //We are on the first node
                        _image = _resourceDictionary["RightArrow"]; //">";
                    }
                    else if( IsEnd )
                    {
                        //When working with 4x4 grids, the track is continuous (the end cell is right below the start cell),
                        //we only need an up arrow to explain the user what's going on next.
                        if( IsContinuousTrack )
                        {
                            _image = _resourceDictionary["TopArrow"];//"^";
                        }
                        else //Otherwise, this image will clearly display the fact that we are going back to the start.
                        {
                            _image = _resourceDictionary["EndArrow"];
                        }
                    }
                    else if( val % 1 == 0 )
                    {
                        //if the value has nothing past the decimal point, 
                        //we are at the end of a row (or at the beginning/end of the track, which are handled above)
                        _image = _resourceDictionary["DownArrow"];
                    }
                    else if( isTruncatedValueEven )
                    {
                        //The next element is on the right of this element, because we are on an even row
                        _image = _resourceDictionary["RightArrow"];
                    }
                    else
                    {
                        //The next element is on the left of this element, because we are on an odd row
                        _image = _resourceDictionary["LeftArrow"];
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
                OnPropertyChanged( "IsParentTheCurrentNode" );
                OnPropertyChanged( "IsHighlighted" );
            }
        }

        public int Row { get; private set; }
        public int Column { get; private set; }

        double _width;
        public double Width { get { return _width; } set { _width = value; OnPropertyChanged( "Width" ); Console.Out.WriteLine( "WIDTH CHANGED : " + value ); } }

        double _height;
        public double Height { get { return _height; } set { _height = value; OnPropertyChanged( "Height" ); Console.Out.WriteLine( "HEIGHT CHANGED : " + value ); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged( string name )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( name ) );
        }
    }
}
