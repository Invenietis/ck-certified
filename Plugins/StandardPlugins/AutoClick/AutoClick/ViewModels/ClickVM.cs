using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.StandardPlugins.AutoClick.Model;
using System.Collections.ObjectModel;

namespace CK.StandardPlugins.AutoClick.ViewModel
{
    public class ClickVM : ObservableCollection<ClickInstruction>
    {
        private ClickEmbedderVM _holder;
        public ClickEmbedderVM Holder { get { return _holder; } internal set { _holder = value; } }

        private string _name;
        public string Name { get { return _name; } set { _name = value; } }

        public ClickVM( ClickEmbedderVM holder, string name, IList<ClickInstruction> clickInstructions )
        {
            _holder = holder;
            _name = name;
            foreach( ClickInstruction instr in clickInstructions )
            {
                Add( instr );
            }
        }
    }
}
