using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Host.Services
{
    /// <summary>
    /// Interface that enables manipulating the graphical host of the CiviKey application
    /// For example, gives access to minimization toggling of the host's window
    /// </summary>
    public interface IHostManipulator : IMinimizable
    {
    }
    
}
