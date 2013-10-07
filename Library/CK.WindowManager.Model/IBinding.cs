using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.WindowManager.Model
{
    /// <summary>
    /// Represents a binding between two <see cref="IWindowElement2"/>.
    /// </summary>
    public interface IBinding
    {
        IWindowElement First { get; }

        IWindowElement Second { get; }
    }
}
