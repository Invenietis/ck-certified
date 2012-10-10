using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Context;
using CK.Keyboard.Model;
using CK.Plugin;

namespace ContextEditor
{
    public interface IKeyboardEditorRoot : IKeyboardBackupManager
    {
        IService<IKeyboardContext> KeyboardContext { get; set; }
        IContext Context { get; set; }
    }
}
