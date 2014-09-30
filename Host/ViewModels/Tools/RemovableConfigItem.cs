using System;
using System.Windows.Input;
using CK.Windows.App;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    public class RemovableConfigItem : ConfigItem
    {
        public event EventHandler<EventArgs> RemoveClick;

        public Object ContentItem { get; private set; }

        readonly ICommand _removeCommand;
        public ICommand RemoveCommand { get { return _removeCommand; } }

        public string TooltipText { get { return R.Remove; } }

        public RemovableConfigItem( ConfigManager configManager, ConfigItem contentConfigItem )
            : base( configManager )
        {
            _removeCommand = new VMCommand( OnRemove );

            ContentItem = contentConfigItem;
        }

        void OnRemove()
        {
            if( RemoveClick != null )
            {
                RemoveClick( this, new EventArgs() );
            }
        }
    }
}
