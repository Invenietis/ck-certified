using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CK.Plugin;

namespace CK.WindowManager.Model
{
    public interface ITopMostService : IDynamicService
    {

        bool RegisterTopMostElement( string levelName, Window window );

        bool UnregisterTopMostElement( Window window );

    }
}
