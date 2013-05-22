#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.ViewModel\VMContext.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using CK.Keyboard.Model;
using CK.Context;
using System.ComponentModel;
using CK.Plugin.Config;
using CK.Core;
using CK.WPF.Controls;

namespace KeyboardEditor.ViewModels
{
    public abstract class VMContext : VMBase, IDisposable
    {
        public VMContext( IContext ctx, IKeyboardContext kbctx, IPluginConfigAccessor config, IPluginConfigAccessor skinConfiguration )
            : this( ctx, kbctx, config, skinConfiguration, new List<IKeyboard>() { } )
        {
        }

        public VMContext( IContext ctx, IKeyboardContext kbctx, IPluginConfigAccessor config, IPluginConfigAccessor skinConfiguration, IKeyboard keyboardToCreate )
            : this( ctx, kbctx, config, skinConfiguration, new List<IKeyboard>() { keyboardToCreate } )
        {
        }

        public VMContext( IContext ctx, IKeyboardContext kbctx, IPluginConfigAccessor config, IPluginConfigAccessor skinConfiguration, IList<IKeyboard> keyboardsToCreate )
        {
        }



        //EventHandler<CurrentKeyboardChangedEventArgs> _evCurrentKeyboardChanged;
        //Dictionary<object, VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>> _dic;
        //PropertyChangedEventHandler _evUserConfigurationChanged;
        //EventHandler<KeyboardEventArgs> _evKeyboardDestroyed;
        //EventHandler<KeyboardEventArgs> _evKeyboardCreated;
        //ObservableCollection<VMKeyboardEditable> _keyboards;
        //IKeyboardContext _kbctx;
        //VMKeyboardEditable _currentKeyboard;
        //IContext _ctx;

        //IPluginConfigAccessor _config;
        //public IPluginConfigAccessor Config
        //{
        //    get
        //    {
        //        return _config;
        //    }
        //    set { _config = value; }
        //}

        //public IPluginConfigAccessor SkinConfiguration { get; set; }

        //public IKeyboardContext KeyboardContext { get { return _kbctx; } }

        //public IContext Context { get { return _ctx; } }

        //public ObservableCollection<VMKeyboardEditable> Keyboards
        //{
        //    get { return _keyboards; }
        //}

        //public VMKeyboardEditable Obtain( IKeyboard keyboard )
        //{
        //    VMKeyboardEditable k = FindViewModel<VMKeyboardEditable>( keyboard );
        //    if( k == null ) throw new Exception( "Context mismatch." );
        //    return k;
        //}

        //public VMZoneEditable Obtain( IZone zone )
        //{
        //    VMZoneEditable z = FindViewModel<VMZoneEditable>( zone );
        //    if( z == null )
        //    {
        //        if( zone.Context != _kbctx )
        //            throw new Exception( "Context mismatch." );
        //        z = CreateZone( zone );
        //        _dic.Add( zone, z );
        //    }
        //    return z;
        //}

        //public VMKeyEditable Obtain( IKey key )
        //{
        //    VMKeyEditable k = FindViewModel<VMKeyEditable>( key );
        //    if( k == null )
        //    {
        //        if( key.Context != _kbctx )
        //            throw new Exception( "Context mismatch." );
        //        k = CreateKey( key );
        //        _dic.Add( key, k );
        //    }
        //    return k;
        //}

        //public VMKeyModeEditable Obtain( IKeyMode keyMode )
        //{
        //    VMKeyModeEditable km = FindViewModel<VMKeyModeEditable>( keyMode );
        //    if( km == null )
        //    {
        //        if( keyMode.Context != _kbctx )
        //            throw new Exception( "Context mismatch." );
        //        km = CreateKeyMode( keyMode );
        //        if( km != null ) //the viewmodel can be null, if the implementation doesn't use this level of objects (SimpleSkin doesn't use these templates)
        //            _dic.Add( keyMode, km );
        //    }
        //    return km;
        //}

        //public VMLayoutKeyModeEditable Obtain( ILayoutKeyMode layoutKeyMode )
        //{
        //    VMLayoutKeyModeEditable lkm = FindViewModel<VMLayoutKeyModeEditable>( layoutKeyMode );
        //    if( lkm == null )
        //    {
        //        if( layoutKeyMode.Context != _kbctx )
        //            throw new Exception( "Context mismatch." );
        //        lkm = CreateLayoutKeyMode( layoutKeyMode );
        //        if( lkm != null ) //the viewmodel can be null, if the implementation doesn't use this level of objects (SimpleSkin doesn't use these templates)
        //            _dic.Add( layoutKeyMode, lkm );
        //    }
        //    return lkm;
        //}

        //public VMKeyboardEditable KeyboardVM
        //{
        //    get { return _currentKeyboard; }
        //    set { _currentKeyboard = value; OnPropertyChanged( "KeyboardVM" ); }
        //}


        //public VMContext( IContext ctx, IKeyboardContext kbctx, IPluginConfigAccessor config, IPluginConfigAccessor skinConfiguration )
        //    : this( ctx, kbctx, config, skinConfiguration, new List<IKeyboard>() { } )
        //{
        //}

        //public VMContext( IContext ctx, IKeyboardContext kbctx, IPluginConfigAccessor config, IPluginConfigAccessor skinConfiguration, IKeyboard keyboardToCreate )
        //    : this( ctx, kbctx, config, skinConfiguration, new List<IKeyboard>() { keyboardToCreate } )
        //{
        //}

        //public VMContext( IContext ctx, IKeyboardContext kbctx, IPluginConfigAccessor config, IPluginConfigAccessor skinConfiguration, IList<IKeyboard> keyboardsToCreate )
        //{
        //    if( keyboardsToCreate == null ) throw new ArgumentException( "The keyboardToCreate list must not be null. Use another constructor." );

        //    OnBeforeCreate();
        //    _dic = new Dictionary<object, VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>>();
        //    _keyboards = new ObservableCollection<VMKeyboardEditable>();
        //    SkinConfiguration = skinConfiguration;
        //    Config = config;

        //    _kbctx = kbctx;
        //    _ctx = ctx;
        //    if( _kbctx.Keyboards.Count() > 0 )
        //    {
        //        IKeyboard _tempKeyboard;
        //        if( keyboardsToCreate.Count == 0 )
        //        {
        //            keyboardsToCreate = _kbctx.Keyboards.ToList();
        //            _tempKeyboard = _kbctx.CurrentKeyboard;
        //        }
        //        else _tempKeyboard = keyboardsToCreate.First();

        //        foreach( IKeyboard keyboard in keyboardsToCreate )
        //        {
        //            VMKeyboardEditable kb = CreateKeyboard( keyboard );
        //            _dic.Add( keyboard, kb );
        //            _keyboards.Add( kb );
        //        }

        //        _currentKeyboard = Obtain( _tempKeyboard );

        //    }

        //    _evKeyboardCreated = new EventHandler<KeyboardEventArgs>( OnKeyboardCreated );
        //    _evCurrentKeyboardChanged = new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
        //    _evKeyboardDestroyed = new EventHandler<KeyboardEventArgs>( OnKeyboardDestroyed );
        //    _evUserConfigurationChanged = new PropertyChangedEventHandler( OnUserConfigurationChanged );

        //    _kbctx.Keyboards.KeyboardCreated += _evKeyboardCreated;
        //    _kbctx.CurrentKeyboardChanged += _evCurrentKeyboardChanged;
        //    _kbctx.Keyboards.KeyboardDestroyed += _evKeyboardDestroyed;
        //    _ctx.ConfigManager.UserConfiguration.PropertyChanged += _evUserConfigurationChanged;

        //}

        //protected virtual void OnBeforeCreate()
        //{
        //}

        //protected abstract VMLayoutKeyModeEditable CreateLayoutKeyMode( ILayoutKeyMode lkm );
        //protected abstract VMKeyModeEditable CreateKeyMode( IKeyMode km );
        //protected abstract VMKeyboardEditable CreateKeyboard( IKeyboard kb );
        //protected abstract VMZoneEditable CreateZone( IZone z );
        //protected abstract VMKeyEditable CreateKey( IKey k );

        //T FindViewModel<T>( object m )
        //    where T : VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>
        //{
        //    VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> vm;
        //    _dic.TryGetValue( m, out vm );
        //    return (T)vm;
        //}

        //public void Dispose()
        //{
        //    OnDispose();
        //    _kbctx.Keyboards.KeyboardCreated -= _evKeyboardCreated;
        //    _kbctx.CurrentKeyboardChanged -= _evCurrentKeyboardChanged;
        //    _kbctx.Keyboards.KeyboardDestroyed -= _evKeyboardDestroyed;
        //    _ctx.ConfigManager.UserConfiguration.PropertyChanged -= _evUserConfigurationChanged;

        //    foreach( var keyboard in _keyboards )
        //    {
        //        keyboard.Dispose();
        //    }

        //    _keyboards.Clear();
        //    _dic.Clear();
        //}

        //protected virtual void OnDispose()
        //{
        //}

        //internal virtual void OnModelDestroy( object m )
        //{
        //    VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> vm;
        //    if( _dic.TryGetValue( m, out vm ) )
        //    {
        //        vm.Dispose();
        //        _dic.Remove( m );
        //    }
        //}

        //#region OnXXXXXXXXX
        //internal virtual void OnKeyboardCreated( object sender, KeyboardEventArgs e )
        //{
        //    VMKeyboardEditable k = CreateKeyboard( e.Keyboard );
        //    _dic.Add( e.Keyboard, k );
        //    _keyboards.Add( k );
        //}

        ////This behavior is linked to the current keyboard.
        ////It should be overridden in the KeyboardEditor, for the KeyboardEditor is not linked to the application's current keyboard.
        //protected virtual void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        //{
        //    if( e.Current != null )
        //    {
        //        _currentKeyboard = Obtain( e.Current );
        //        OnPropertyChanged( "KeyboardVM" );
        //        _currentKeyboard.TriggerPropertyChanged();
        //    }
        //}

        //internal virtual void OnUserConfigurationChanged( object sender, PropertyChangedEventArgs e )
        //{
        //    //If the CurrentContext has changed, but not because a new context has been loaded (happens when the userConf if changed but the context is kept the same).
        //    if( e.PropertyName == "CurrentContextProfile" )
        //    {
        //        OnPropertyChanged( "KeyboardVM" );
        //    }
        //}

        //internal virtual void OnKeyboardDestroyed( object sender, KeyboardEventArgs e )
        //{
        //    _keyboards.Remove( Obtain( e.Keyboard ) );
        //    OnModelDestroy( e.Keyboard );
        //}
        //#endregion

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
