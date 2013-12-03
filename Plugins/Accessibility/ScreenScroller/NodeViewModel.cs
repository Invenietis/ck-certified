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
        private NodeViewModel( NodeViewModel parent, int level, int childrenCount, int maxDepth, int index )
        {
            _level = level;
            ChildNodes = new ObservableCollection<NodeViewModel>();
            _index = index;

            if( _level < maxDepth )
            {
                for( int i = 0; i < childrenCount; i++ )
                {
                    ChildNodes.Add( new NodeViewModel( this, _level + 1, childrenCount, maxDepth, i ) );
                }
            }

            Parent = parent;
        }

        public NodeViewModel( NodeViewModel root, int x, int y, int childrenCount, int maxDepth, bool isRoot )
        {
            ChildNodes = new ObservableCollection<NodeViewModel>();
            Parent = Root = root;
            _level = 0;
            _x = x;
            _y = y;

            if( _level < maxDepth )
            {
                for( int i = 0; i < childrenCount; i++ )
                {
                    ChildNodes.Add( new NodeViewModel( this, _level + 1, childrenCount, maxDepth, i ) );
                }
            }
        }

        internal NodeViewModel()
        {
            _isRoot = true;
        }

        internal void MoveNext()
        {
            if( _entered )
            {
                _entered = false;
                IsHighlighted = false;
            }
            else
            {
                if( CurrentIndex == ChildNodes.Count - 1 ) CurrentIndex = 0;
                else CurrentIndex++;
            }

            ChildNodes.ElementAt( CurrentIndex ).IsHighlighted = true;
        }

        bool _entered;
        internal void Entered()
        {
            _entered = true;
        }

        public int X { get { return _x; } }
        public int Y { get { return _y; } }
        public int Index { get { return _index; } }

        int _x, _y, _index;
        int _level;
        bool _isRoot;
        Screen _screen;

        public ObservableCollection<NodeViewModel> ChildNodes { get; set; }
        public int Thickness { get { return 4 - Level > 0 ? 4 - Level : 0; } }
        public int Level { get { return _level; } }
        public int CurrentIndex { get; set; }
        public NodeViewModel Root { get; set; }
        public NodeViewModel Parent { get; set; }

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
                    else if( val == 0 )
                    {
                        //We are on the first node
                        _image = "->";
                    }
                    else if( Index + 1 == ChildNodes.Count )
                    {
                        //We are at the end of the track
                        _image = "END";
                    }
                    else if( val % 1 == 0 ) //if the value has nothing past the decimal point, we are at the end of a row (or at the beginning/end of the track)
                    {
                        //We are on the end of a row
                        _image = "v";
                    }
                    else if( isTruncatedValueEven )
                    {
                        //The next element is on the right of this element
                        _image = "->";
                    }
                    else
                    {
                        //The next element is on the left of this element
                        _image = "<-";
                    }

                }
                return _image;
            }
        }

        bool _isHighlighted;
        public bool IsHighlighted { get { return _isHighlighted; } set { _isHighlighted = value; OnPropertyChanged( "IsHighlighted" ); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged( string name )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( name ) );
        }
    }
}
