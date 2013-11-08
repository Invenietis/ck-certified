using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Plugin;

namespace Help.Services
{
    public interface IHaveDefaultHelp : IDynamicService
    {
        Stream GetDefaultHelp();
    }
}
