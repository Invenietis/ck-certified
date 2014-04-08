using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonServices
{
    public interface IContextSaver
    {
        void SaveContext();
        void SaveContext( Uri address );
        void SaveSystemConfig();
        void SaveUserConfig();
        void SaveUserConfig( Uri address, bool setAdressAsCurrent );
    }
}
