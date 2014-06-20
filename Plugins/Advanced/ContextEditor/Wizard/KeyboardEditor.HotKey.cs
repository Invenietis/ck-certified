#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\KeyboardEditor.HotKey.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
