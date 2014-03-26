using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.WindowManager.Model;

namespace CK.WindowManager
{
    public interface IUnbindButtonManager : IDynamicService
    {
        WindowElement CreateButton( ISpatialBinding spatialBinding, ISpatialBinding slaveSpatialBinding, BindingPosition position );

        void DeleteButton( IWindowElement button );
    }
}
