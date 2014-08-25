#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\ViewModels\VMContextSimple.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using CK.Context;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Threading;
using SimpleSkin.ViewModels.Versionning;
using CK.Windows;
using System.Diagnostics;

namespace SimpleSkin.ViewModels
{
    public class VMContextCurrentKeyboardSimple : VMContextSimpleBase
    {
        public VMContextCurrentKeyboardSimple( NoFocusManager noFocusManager, IContext ctx, IKeyboardContext kbctx, IPluginConfigAccessor config )
            : base( noFocusManager, ctx, kbctx, config )
        {
        }

        protected override Func<IKeyboard> KeyboardSelector
        {
            get
            {
                //Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
                return () => KeyboardContext.Keyboards.Context.CurrentKeyboard;
            }
        }

    }

    public class VMContextActiveKeyboard : VMContextSimpleBase
    {
        string _activeKeyboardName;

        public VMContextActiveKeyboard( NoFocusManager noFocusManager, string activeKeyboardName, IContext ctx, IKeyboardContext kbctx, IPluginConfigAccessor config )
            : base( noFocusManager, ctx, kbctx, config )
        {
            _activeKeyboardName = activeKeyboardName;
            kbctx.Keyboards.KeyboardActivated += OnKeyboardActivated;
            kbctx.Keyboards.KeyboardDeactivated += OnKeyboardDeactivated;
        }

        void OnKeyboardActivated( object sender, KeyboardEventArgs e )
        {
            if( _activeKeyboardName == e.Keyboard.Name )
            {
                NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
                {
                    OnPropertyChanged( "KeyboardVM" );
                }) );
            }
        }

        void OnKeyboardDeactivated( object sender, KeyboardEventArgs e )
        {
            if( _activeKeyboardName == e.Keyboard.Name )
            {
                NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
                {
                    OnPropertyChanged( "KeyboardVM" );
                }) );
            }
        }

        protected override Func<IKeyboard> KeyboardSelector
        {
            get
            {
                //Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
                return () =>
                {
                    var kb = KeyboardContext.Keyboards[_activeKeyboardName];
                    if( kb != null ) return kb;

                    return null;
                };
            }
        }

        protected override void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
        }

        public override void Dispose()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            base.Dispose();
            KeyboardContext.Keyboards.KeyboardActivated -= OnKeyboardActivated;
            KeyboardContext.Keyboards.KeyboardDeactivated -= OnKeyboardDeactivated;
        }
    }

    public abstract class VMContextSimpleBase : VMBase, IDisposable
    {
        Dictionary<object, VMContextElement> _dic;
        ObservableCollection<VMKeyboardSimple> _keyboards;
        VMKeyboardSimple _keyboard;
        IPluginConfigAccessor _config;
        IKeyboardContext _kbctx;
        IContext _ctx;

        public ObservableCollection<VMKeyboardSimple> Keyboards { get { return _keyboards; } }

        public VMKeyboardSimple KeyboardVM
        {
            get
            {
                if( _keyboard == null )
                {
                    IKeyboard k = null;
                    k = KeyboardSelector();
                    _keyboard = Obtain( k );
                }

                return _keyboard;
            }
        }

        public IKeyboardContext KeyboardContext { get { return _kbctx; } }
        public IPluginConfigAccessor Config { get { return _config; } }
        public IContext Context { get { return _ctx; } }

        NoFocusManager _noFocusManager;
        public NoFocusManager NoFocusManager { get { return _noFocusManager; } }

        protected abstract Func<IKeyboard> KeyboardSelector { get; }

        public VMContextSimpleBase( NoFocusManager noFocusManager, IContext ctx, IKeyboardContext kbctx, IPluginConfigAccessor config )
        {
            _noFocusManager = noFocusManager;
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            _dic = new Dictionary<object, VMContextElement>();
            _keyboards = new ObservableCollection<VMKeyboardSimple>();
            _config = config;
            _kbctx = kbctx;
            _ctx = ctx;

            if( _kbctx.Keyboards.Count > 0 )
            {
                foreach( IKeyboard keyboard in _kbctx.Keyboards )
                {
                    VMKeyboardSimple kb = CreateKeyboard( keyboard );
                    _dic.Add( keyboard, kb );
                    NoFocusManager.NoFocusDispatcher.Invoke( (Action)(() =>
                    {
                        _keyboards.Add( kb );
                    }) );
                }
            }
            else
            {
                //TODO : send a notification telling the user that there are no keyboards in the current context.
            }

            RegisterEvents();
        }

        public VMKeyboardSimple Obtain( IKeyboard keyboard )
        {
            if( keyboard == null ) throw new ArgumentNullException( "keyboard" );

            VMKeyboardSimple k = FindViewModel<VMKeyboardSimple>( keyboard );
            if( k == null ) throw new Exception( "Context mismatch." );
            return k;
        }

        public VMZoneSimple Obtain( IZone zone )
        {
            VMZoneSimple z = FindViewModel<VMZoneSimple>( zone );
            if( z == null )
            {
                if( zone.Context != _kbctx )
                    throw new Exception( "Context mismatch." );
                z = CreateZone( zone );
                _dic.Add( zone, z );
            }
            return z;
        }

        public VMKeySimple Obtain( IKey key )
        {
            VMKeySimple k = FindViewModel<VMKeySimple>( key );
            if( k == null )
            {
                if( key.Context != _kbctx )
                    throw new Exception( "Context mismatch." );
                k = CreateKey( key );
                _dic.Add( key, k );
            }
            return k;
        }

        T FindViewModel<T>( object m )
            where T : VMContextElement
        {
            VMContextElement vm;
            _dic.TryGetValue( m, out vm );
            return (T)vm;
        }

        public virtual void Dispose()
        {
            UnregisterEvents();
            foreach( VMContextElement vm in _dic.Values ) vm.Dispose();
            _dic.Clear();
        }

        internal void OnModelDestroy( object m )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            VMContextElement vm;
            if( _dic.TryGetValue( m, out vm ) )
            {
                vm.Dispose();
                _dic.Remove( m );
            }
        }

        void OnKeyboardCreated( object sender, KeyboardEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            VMKeyboardSimple k = CreateKeyboard( e.Keyboard );
            _dic.Add( e.Keyboard, k );
            NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
            {
                _keyboards.Add( k );
            }) );
        }

        protected virtual void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( e.Current != null )
            {
                _keyboard = Obtain( e.Current );
                _keyboard.TriggerPropertyChanged();
                NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
                {
                    OnPropertyChanged( "KeyboardVM" );
                }) );
            }
        }

        void OnUserConfigurationChanged( object sender, PropertyChangedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            //If the CurrentContext has changed, but not because a new context has been loaded (happens when the userConf if changed but the context is kept the same).
            if( e.PropertyName == "CurrentContextProfile" )
            {
                NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
                {
                    OnPropertyChanged( "KeyboardVM" );
                }) );
            }
        }

        void OnKeyboardDestroyed( object sender, KeyboardEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            
            VMKeyboardSimple vmKeyboard = Obtain( e.Keyboard );
            vmKeyboard.Dispose();

            NoFocusManager.NoFocusDispatcher.Invoke( (Action)(() =>
            {
                _keyboards.Remove( vmKeyboard );
            }) );

            OnModelDestroy( e.Keyboard );
        }

        private void RegisterEvents()
        {
            _kbctx.Keyboards.KeyboardCreated += OnKeyboardCreated;
            _kbctx.CurrentKeyboardChanged += OnCurrentKeyboardChanged;
            _kbctx.Keyboards.KeyboardDestroyed += OnKeyboardDestroyed;
            _ctx.ConfigManager.UserConfiguration.PropertyChanged += OnUserConfigurationChanged;
        }

        private void UnregisterEvents()
        {
            _kbctx.Keyboards.KeyboardCreated -= OnKeyboardCreated;
            _kbctx.CurrentKeyboardChanged -= OnCurrentKeyboardChanged;
            _kbctx.Keyboards.KeyboardDestroyed -= OnKeyboardDestroyed;
            _ctx.ConfigManager.UserConfiguration.PropertyChanged -= OnUserConfigurationChanged;
        }

        private VMKeyboardSimple CreateKeyboard( IKeyboard kb )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            return new VMKeyboardSimple( this, kb );
        }

        private VMZoneSimple CreateZone( IZone z )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            // ToDoJL Check index
            //we give a Zone a default Index of -1, it means that it has no Index yet.
            //This index will be used by the editor and the keyscroller
            return new VMZoneSimple( this, z, Config[z].GetOrSet<int>( "Index", -1 ) );
        }

        private VMKeySimple CreateKey( IKey k )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            V150To160.EnsureKeyVersion( Config, k );
            return new VMKeySimple( this, k );
        }
    }
}
