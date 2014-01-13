using System;
using System.Windows;

namespace ScreenScroller
{
    public interface IRootNode
    {
        NodeViewModel CurrentNode { get; }
        int MaxLapCount { get; }
        int SquareSize { get; }

        ResourceDictionary ImageDictionary { get; }

        event EventHandler<LevelChangedEventArgs> LevelChanged;
    }

    public class LevelChangedEventArgs : EventArgs
    {
        public int PreviousLevel { get; private set; }
        public int NextLevel { get; private set; }

        public LevelChangedEventArgs(int previousLevel, int nextLevel)
        {
            PreviousLevel = previousLevel;
            NextLevel = nextLevel;
        }
    }
}
