#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\FirstLevel\RootConfigModels.cs) is part of CiviKey. 
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
using System.Collections.ObjectModel;
using System.Linq;
using CK.Keyboard.Model;
using CK.WPF.Controls;

namespace Host.ViewModels
{
    public class ContextModel : VMBase, IDisposable
    { 
        IKeyboardContext _ctx;
        AppViewModel _app;

        ObservableCollection<KeyboardModel> _keyboards;
        public ObservableCollection<KeyboardModel> Keyboards
        {
            get { return _keyboards; }
        }

        public ContextModel( AppViewModel app )
        {
            _app = app;
            _ctx = app.KeyboardContext;

            _keyboards = new ObservableCollection<KeyboardModel>();
            foreach( IKeyboard keyboard in _ctx.Keyboards )
            {
                if( keyboard.Name != "Prediction" )
                    _keyboards.Add( new KeyboardModel( keyboard ) );
            }

            RegisterEvents();
        }

        public KeyboardModel Current
        {
            get
            {
                return _ctx.CurrentKeyboard != null ? _keyboards.Where( k => k.Name == _ctx.CurrentKeyboard.Name ).Single() : null;
            }
            set
            {
                if( value != null )
                    _ctx.CurrentKeyboard = value.Model;
                else _ctx.CurrentKeyboard = null;

                OnPropertyChanged( "Current" );
            }
        }

        void OnKeyboardDestroyed( object sender, KeyboardEventArgs e )
        {
            //temporary
            if( e.Keyboard.Name != "Prediction" )
            {
                var destroyedKeyboard = _keyboards.Where( k => k.Name == e.Keyboard.Name ).Single();
                destroyedKeyboard.Dispose();

                _keyboards.Remove( destroyedKeyboard );
            }
        }

        void OnKeyboardCreated( object sender, KeyboardEventArgs e )
        {
            if( e.Keyboard.Name != "Prediction" )
            {
                _keyboards.Add( new KeyboardModel( e.Keyboard ) );
            }
        }

        private void RegisterEvents()
        {
            _ctx.Keyboards.KeyboardCreated += OnKeyboardCreated;
            _ctx.Keyboards.KeyboardDestroyed += OnKeyboardDestroyed;
            _ctx.Keyboards.CurrentChanged += OnCurrentChanged;
        }

        void OnCurrentChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            OnPropertyChanged( "Current" );
        }

        private void UnregisterEvents()
        {
            _ctx.Keyboards.KeyboardCreated -= OnKeyboardCreated;
            _ctx.Keyboards.KeyboardDestroyed -= OnKeyboardDestroyed;
            _ctx.Keyboards.CurrentChanged -= OnCurrentChanged;
        }

        public void Dispose()
        {
            UnregisterEvents();

            foreach( KeyboardModel keyboard in Keyboards )
            {
                keyboard.Dispose();
            }
        }
    }

    public class KeyboardModel : VMBase, IDisposable
    {
        IKeyboard _keyboard;
        public KeyboardModel( IKeyboard keyboard )
        {
            _keyboard = keyboard;
            RegisterEvents();
        }

        public IKeyboard Model { get { return _keyboard; } }
        public string Name { get { return _keyboard.Name; } }

        private void RegisterEvents()
        {
            _keyboard.Context.Keyboards.KeyboardRenamed += Keyboards_KeyboardRenamed;
        }

        void Keyboards_KeyboardRenamed( object sender, KeyboardRenamedEventArgs e )
        {
            OnPropertyChanged( "Name" );
        }

        private void UnregisterEvents()
        {
            _keyboard.Context.Keyboards.KeyboardRenamed -= Keyboards_KeyboardRenamed;
        }

        public void Dispose()
        {
            UnregisterEvents();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
