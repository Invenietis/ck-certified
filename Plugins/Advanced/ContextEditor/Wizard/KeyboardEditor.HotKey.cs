using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Interop;
using KeyboardEditor.s;
using KeyboardEditor.Tools;

namespace KeyboardEditor
{
    public partial class KeyboardEditor
    {
        //Dictionary<HookKey, int> _dic;

        //private void RegisterHotKeys()
        //{
            //( (AppView)_mainWindow ).HookInvoqued += KeyboardEditor_HookInvoqued;

            //_dic = new Dictionary<HookKey, int>();

            //RegisterKey( Constants.NOMOD, (int)Keys.Up );
            //RegisterKey( Constants.SHIFT, (int)Keys.Up );

            //RegisterKey( Constants.NOMOD, (int)Keys.Down );
            //RegisterKey( Constants.SHIFT, (int)Keys.Down );

            //RegisterKey( Constants.NOMOD, (int)Keys.Delete );
            //RegisterKey( Constants.SHIFT, (int)Keys.Delete );

            //RegisterKey( Constants.NOMOD, (int)Keys.Left );
            //RegisterKey( Constants.SHIFT, (int)Keys.Left );

            //RegisterKey( Constants.NOMOD, (int)Keys.Right );
            //RegisterKey( Constants.SHIFT, (int)Keys.Right );
        //}

        //private bool RegisterKey( int modifier, int keyCode )
        //{
            //int id;
            //HookKey hk = new HookKey( modifier, keyCode );
            
            //if( !_dic.ContainsKey( hk ) )
            //{
            //    HotKeyHook.Register( _interopHelper.Handle, modifier, keyCode, out id );
            //    _dic.Add( hk, id );
            //    return true;
            //}

        //    return false;
        //}

        //private void UnregisterAllHotKeys()
        //{
            //foreach( var item in _dic.Values )
            //{
            //    HotKeyHook.Unregister( _interopHelper.Handle, item );
            //}
        //}
    }
}
