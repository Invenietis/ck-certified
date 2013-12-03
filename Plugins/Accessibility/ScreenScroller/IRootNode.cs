using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScreenScroller
{
    public interface IRootNode
    {
        NodeViewModel CurrentNode { get; }
        int MaxLapCount { get; }

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
