using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Config;
using CK.Core;

namespace Certified.Tests
{
    class MockPluginConfigAccessor : IPluginConfigAccessor
    {
        #region IPluginConfigAccessor Members

        INamedVersionedUniqueId _idEdited;
		object _system;
		object _user;
		object _context;
        IConfigContainer _configContainer;

        public MockPluginConfigAccessor( INamedVersionedUniqueId idEdited, object user, object system, object ctx, IConfigContainer sharedDic )
		{
            _idEdited = idEdited;
            _system = system;
            _user = user;
            _context = ctx;
            _configContainer = sharedDic;
        }

        public event EventHandler<ConfigChangedEventArgs>  ConfigChanged;

        public IObjectPluginConfig Context
        {
            get { return Get( _context ); }
        }

        IObjectPluginConfig Get( object o )
        {
            return _configContainer.GetObjectPluginConfig( o, _idEdited );
        }

        public IObjectPluginConfig System
        {
            get { return Get( _system ); }
        }

        public IObjectPluginConfig User
        {
            get { return Get( _user ); }
        }

        public IObjectPluginConfig this[object o]
        {
            get { return Get( o ); }
        }

        #endregion
    }
}
