#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\CK.WindowManager\WindowElementBinder.cs) is part of CiviKey. 
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
using CK.Plugin;
using CK.WindowManager.Model;
using CK.Core;
using System.Diagnostics;
using System.Threading;
using CK.Storage;
using System.Xml;
using CK.Plugin.Config;
using System.Linq;
using System.Windows;
using CK.Windows;
using System.Windows.Threading;

namespace CK.WindowManager
{
    [Plugin( "{F6B5D818-3C04-4A46-AD65-AFC5458A394C}", Categories = new string[] { "Accessibility" }, PublicName = "CK.WindowManager.WindowElementBinder", Version = "1.0.0" )]
    public class WindowElementBinder : IWindowBinder, IPlugin
    {
        IDictionary<IWindowElement,List<IBinding>> _bindings;
        DefaultActivityLogger _logger;
        SerializableBindings _persistantBindings;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IUnbindButtonManager> UnbindButtonManager { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        IDictionary<IWindowElement, SpatialBinding> _spatialBindings = new Dictionary<IWindowElement, SpatialBinding>();

        public WindowElementBinder()
        {
            _bindings = new Dictionary<IWindowElement, List<IBinding>>();
            _logger = new DefaultActivityLogger();
            _persistantBindings = new SerializableBindings();
            //_logger.Tap.Register( new ActivityLoggerConsoleSink() );
        }

        public ISpatialBinding GetBinding( IWindowElement referential )
        {
            if( referential == null ) throw new ArgumentNullException( "referential" );

            SpatialBinding b = null;
            _spatialBindings.TryGetValue( referential, out b );
            return b ?? new SpatialBinding( referential );
        }

        private bool CanBind( IWindowElement target, IWindowElement origin, BindingPosition position, out SpatialBinding targetSpatialBinding, out SpatialBinding originSpatialBinding )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );

            originSpatialBinding = null;
            using( _logger.OpenGroup( LogLevel.Info, "Master binding..." ) )
            {
                if( _spatialBindings.TryGetValue( target, out targetSpatialBinding ) )
                {
                    if( position == BindingPosition.Top && targetSpatialBinding.Top != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", targetSpatialBinding.Top.Window.Name, target.Name, position );
                        return false;
                    }
                    else if( position == BindingPosition.Left && targetSpatialBinding.Left != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", targetSpatialBinding.Left.Window.Name, target.Name, position );
                        return false;
                    }
                    else if( position == BindingPosition.Bottom && targetSpatialBinding.Bottom != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", targetSpatialBinding.Bottom.Window.Name, target.Name, position );
                        return false;
                    }
                    else if( position == BindingPosition.Right && targetSpatialBinding.Right != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", targetSpatialBinding.Right.Window.Name, target.Name, position );
                        return false;
                    }
                    else _logger.Trace( "{0} already exists in bindings but no window attached at position {1}.", target.Name, position );
                }
                else
                {
                    _logger.Trace( "Fresh new window" );
                }
            }

            using( _logger.OpenGroup( LogLevel.Info, "Slave binding..." ) )
            {
                if( _spatialBindings.TryGetValue( origin, out originSpatialBinding ) )
                {
                    if( position == BindingPosition.Top && originSpatialBinding.Bottom != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", originSpatialBinding.Bottom.Window.Name, target.Name, position );
                        return false;
                    }
                    else if( position == BindingPosition.Left && originSpatialBinding.Right != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", originSpatialBinding.Right.Window.Name, target.Name, position );
                        return false;
                    }
                    else if( position == BindingPosition.Bottom && originSpatialBinding.Top != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", originSpatialBinding.Top.Window.Name, target.Name, position );
                        return false;
                    }
                    else if( position == BindingPosition.Right && originSpatialBinding.Left != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", originSpatialBinding.Left.Window.Name, target.Name, position );
                        return false;
                    }
                    else _logger.Trace( "{0} already exists in bindings but no window attached at position {1}.", origin.Name, position );
                }
                else
                {
                    _logger.Trace( "Fresh new window" );
                    //spatialBinding = new SpatialBinding( slave );
                }
            }

            return true;
        }

        public IBindResult PreviewBind( IWindowElement target, IWindowElement origin, BindingPosition position )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

            if( target == null ) throw new ArgumentNullException( "master" );
            if( origin == null ) throw new ArgumentNullException( "slave" );

            SpatialBinding targetSpatialBinding = null;
            SpatialBinding originSpatialBinding = null;

            if( CanBind( target, origin, position, out targetSpatialBinding, out originSpatialBinding ) )
            {
                var binding = new SimpleBinding
                {
                    Target = target,
                    Origin = origin,
                    Position = position
                };

                var evt = new WindowBindedEventArgs
                {
                    Binding = binding,
                    BindingType = BindingEventType.Attach
                };

                if( PreviewBinding != null )
                    PreviewBinding( this, evt );

                return new BindResult( this, binding );
            }
            return NullResult.Default;
        }

        public IBindResult PreviewUnbind( IWindowElement target, IWindowElement origin )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

            var binding = new SimpleBinding
            {
                Target = target,
                Origin = origin
            };

            var evt = new WindowBindedEventArgs
            {
                Binding = binding,
                BindingType = BindingEventType.Detach
            };

            if( PreviewBinding != null )
                PreviewBinding( this, evt );

            return new UnbindResult( this, binding );
        }

        public void Bind( IWindowElement master, IWindowElement slave, BindingPosition position, bool saveBinding = false )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

            if( master == null ) throw new ArgumentNullException( "master" );
            if( slave == null ) throw new ArgumentNullException( "slave" );

            //Console.WriteLine( "BIND thread id: {0} TimeSpan : {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now.Ticks );

            // Spatial binding point of view
            using( _logger.OpenGroup( LogLevel.Info, "Attaching {0} on {1} at {2}", master.Name, slave.Name, position.ToString() ) )
            {
                SpatialBinding spatialBinding = null;
                SpatialBinding slaveSpatialBinding = null;

                if( CanBind( master, slave, position, out spatialBinding, out slaveSpatialBinding ) )
                {
                    _logger.Trace( "Before binding..." );

                    var binding = new SimpleBinding
                    {
                        Target = master,
                        Origin = slave,
                        Position = position
                    };

                    var evt = new WindowBindingEventArgs
                    {
                        Binding = binding,
                        BindingType = BindingEventType.Attach
                    };

                    if( BeforeBinding != null )
                        BeforeBinding( this, evt );

                    if( evt.Canceled == true )
                    {
                        _logger.Trace( "...canceled. The reason was {0}.", evt.CancelReason ?? "No Reason" );
                    }
                    else
                    {
                        if( spatialBinding == null )
                        {
                            spatialBinding = new SpatialBinding( master );
                            _spatialBindings.Add( master, spatialBinding );
                        }
                        if( slaveSpatialBinding == null )
                        {
                            slaveSpatialBinding = new SpatialBinding( slave );
                            _spatialBindings.Add( slave, slaveSpatialBinding );
                        }

                        Debug.Assert( spatialBinding != null );
                        Debug.Assert( slaveSpatialBinding != null );

                        //TODO : FIXWITHDOCKING

                        if( position == BindingPosition.Top )
                        {
                            spatialBinding.Top = slaveSpatialBinding;
                            slaveSpatialBinding.Bottom = spatialBinding;
                        }
                        if( position == BindingPosition.Left )
                        {
                            spatialBinding.Left = slaveSpatialBinding;
                            slaveSpatialBinding.Right = spatialBinding;
                        }
                        if( position == BindingPosition.Bottom )
                        {
                            spatialBinding.Bottom = slaveSpatialBinding;
                            slaveSpatialBinding.Top = spatialBinding;
                        }
                        if( position == BindingPosition.Right )
                        {
                            spatialBinding.Right = slaveSpatialBinding;
                            slaveSpatialBinding.Left = spatialBinding;
                        }

                        if( saveBinding )
                            _persistantBindings.Add( binding );

                        var evtAfter = new WindowBindedEventArgs
                        {
                            Binding = binding,
                            BindingType = BindingEventType.Attach
                        };

                        _logger.Trace( "After binding..." );
                        if( AfterBinding != null )
                            AfterBinding( this, evtAfter );
                    }
                }
            }
        }

        public void Unbind( IWindowElement me, IWindowElement other, bool saveBinding = true )
        {
            if( Dispatcher.CurrentDispatcher != Application.Current.Dispatcher ) throw new InvalidOperationException( "This method should only be called by the Application Thread." );

            if( me == null ) throw new ArgumentNullException( "me" );
            if( other == null ) throw new ArgumentNullException( "other" );

            SpatialBinding spatialBinding = null;
            if( _spatialBindings.TryGetValue( me, out spatialBinding ) )
            {
                var binding = new SimpleBinding
                {
                    Target = me,
                    Origin = other
                };
                var evt = new WindowBindingEventArgs { Binding = binding, BindingType = BindingEventType.Detach };
                if( BeforeBinding != null ) BeforeBinding( this, evt );

                if( evt.Canceled == false )
                {
                    Debug.Assert( me == spatialBinding.Window );

                    if( spatialBinding.Bottom != null && spatialBinding.Bottom.Window == other )
                    {
                        //UnbindButtonManager.Service.RemoveButton( spatialBinding.Bottom.UnbindButton );
                        spatialBinding.Bottom = null;
                        Unbind( other, me, saveBinding );
                    }
                    if( spatialBinding.Left != null && spatialBinding.Left.Window == other )
                    {
                        //UnbindButtonManager.Service.RemoveButton( spatialBinding.Left.UnbindButton );
                        spatialBinding.Left = null;
                        Unbind( other, me, saveBinding );
                    }
                    if( spatialBinding.Top != null && spatialBinding.Top.Window == other )
                    {
                        //UnbindButtonManager.Service.RemoveButton( spatialBinding.Top.UnbindButton );
                        spatialBinding.Top = null;
                        Unbind( other, me, saveBinding );
                    }
                    if( spatialBinding.Right != null && spatialBinding.Right.Window == other )
                    {
                        //UnbindButtonManager.Service.RemoveButton( spatialBinding.Right.UnbindButton );
                        spatialBinding.Right = null;
                        Unbind( other, me, saveBinding );
                    }

                    if( spatialBinding.IsAlone )
                        _spatialBindings.Remove( me );

                    if( !saveBinding )
                        _persistantBindings.Remove( binding );
                }

                var evtAfter = new WindowBindedEventArgs { Binding = binding, BindingType = BindingEventType.Detach };
                if( AfterBinding != null ) AfterBinding( this, evtAfter );
            }
        }

        #region Binding Impl

        class SimpleBinding : IBinding
        {
            public IWindowElement Target { get; set; }

            public IWindowElement Origin { get; set; }

            public BindingPosition Position { get; set; }

            public override bool Equals( object obj )
            {
                var binding = obj as IBinding;
                if( binding != null )
                {
                    return ReferenceEquals( binding.Target, Target ) && ReferenceEquals( binding.Origin, Origin );
                }
                return false;
            }

            public override int GetHashCode()
            {
                return Target.GetHashCode() ^ Origin.GetHashCode();
            }
        }

        class SpatialBinding : ISpatialBinding
        {
            public SpatialBinding( IWindowElement w )
            {
                Window = w;
            }

            public IWindowElement Window { get; private set; }

            public ISpatialBinding Left { get; set; }

            public ISpatialBinding Right { get; set; }

            public ISpatialBinding Bottom { get; set; }

            public ISpatialBinding Top { get; set; }

            public bool IsAlone
            {
                get { return Left == null && Right == null && Top == null && Bottom == null; }
            }
        }

        class SerializableBindings : IStructuredSerializable
        {
            public class SerializableBinding : IStructuredSerializable
            {
                public string Target { get; set; }
                public string Origin { get; set; }
                public BindingPosition Position { get; set; }

                internal SerializableBinding( string target, string origin, BindingPosition position )
                {
                    Target = target;
                    Origin = origin;
                    Position = position;
                }

                #region IStructuredSerializable Members

                public void ReadContent( IStructuredReader sr )
                {
                    XmlReader r = sr.Xml;
                    r.Read();
                    r.ReadStartElement( "Bind" );
                    Target = r.GetAttribute( "Master" );
                    Origin = r.GetAttribute( "Origin" );
                    Position = r.GetAttributeEnum( "Position", BindingPosition.None );
                    r.Read();
                }

                public void WriteContent( IStructuredWriter sw )
                {
                    XmlWriter w = sw.Xml;
                    w.WriteAttributeString( "Master", Target );
                    w.WriteAttributeString( "Slave", Origin );
                    w.WriteAttributeString( "Position", Position.ToString() );
                }

                #endregion
            }

            public List<SerializableBinding> Bindings { get; set; }

            public SerializableBindings()
            {
                Bindings = new List<SerializableBinding>();
            }

            public void Add( string target, string origin, BindingPosition position )
            {
                Bindings.Add( new SerializableBinding( target, origin, position ) );
            }

            public void Add( CK.WindowManager.WindowElementBinder.SimpleBinding binding )
            {
                Bindings.Add( new SerializableBinding( binding.Target.Name, binding.Origin.Name, binding.Position ) );
            }

            public bool Contains( SimpleBinding binding )
            {
                return Bindings.Any( sb => Comparer( sb, binding ) );
            }

            bool Comparer( SerializableBinding sb, SimpleBinding s )
            {
                return (sb.Target == s.Target.Name && sb.Origin == s.Origin.Name && sb.Position == s.Position)
                    || (sb.Target == s.Origin.Name && sb.Origin == s.Target.Name && sb.Position == GetOppositePosition( s.Position ));
            }

            public void Remove( CK.WindowManager.WindowElementBinder.SimpleBinding binding )
            {
                //TODO performance
                var serBind = Bindings.Where( sb => (sb.Origin == binding.Origin.Name && sb.Target == binding.Target.Name)
                                      || (sb.Target == binding.Origin.Name && sb.Origin == binding.Target.Name) ).FirstOrDefault();
                if( serBind != null ) Bindings.Remove( serBind );
            }

            #region IStructuredSerializable Members

            public void ReadContent( IStructuredReader sr )
            {
                XmlReader r = sr.Xml;
                r.Read();
                r.ReadStartElement( "Bindings" );
                if( r.IsStartElement( "Bind" ) )
                {
                    while( r.IsStartElement( "Bind" ) )
                    {
                        Bindings.Add( new SerializableBinding( r.GetAttribute( 0 ), r.GetAttribute( 1 ), r.GetAttributeEnum<BindingPosition>( "Position", BindingPosition.None ) ) );
                        r.Read();
                    }
                }

                r.ReadEndElement();
            }

            public void WriteContent( IStructuredWriter sw )
            {
                XmlWriter w = sw.Xml;

                w.WriteStartElement( "Bindings" );

                foreach( var b in Bindings )
                    sw.WriteInlineObjectStructuredElement( "Bind", b );

                w.WriteFullEndElement();
            }

            #endregion
        }

        #endregion

        #region Events

        public event EventHandler<WindowBindedEventArgs> PreviewBinding;

        public event EventHandler<WindowBindingEventArgs> BeforeBinding;

        public event EventHandler<WindowBindedEventArgs> AfterBinding;

        #endregion

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _persistantBindings = (SerializableBindings)Config.User["SerializableBindings"];
            if( _persistantBindings == null ) _persistantBindings = (SerializableBindings)Config.System["SerializableBindings"];
            if( _persistantBindings == null ) _persistantBindings = new SerializableBindings();

            WindowManager.Service.Unregistered += WindowManager_Unregistered;
            WindowManager.Service.Registered += OnRegistered;
        }

        void OnRegistered( object sender, WindowElementEventArgs e )
        {
            foreach( var sb in _persistantBindings.Bindings.Where( sb => sb.Origin == e.Window.Name || sb.Target == e.Window.Name ) )
            {
                if( sb.Origin == e.Window.Name )
                {
                    IWindowElement element = WindowManager.Service.GetByName( sb.Target );
                    if( element != null ) Bind( element, e.Window, sb.Position, false );
                }
                else if( sb.Target == e.Window.Name )
                {
                    IWindowElement element = WindowManager.Service.GetByName( sb.Origin );
                    if( element != null ) Bind( e.Window, element, sb.Position, false );
                }
            }
        }

        void WindowManager_Unregistered( object sender, WindowElementEventArgs e )
        {
            var binding = GetBinding( e.Window );
            if( binding.Bottom != null ) Unbind( binding.Window, binding.Bottom.Window );
            if( binding.Top != null ) Unbind( binding.Window, binding.Top.Window );
            if( binding.Left != null ) Unbind( binding.Window, binding.Left.Window );
            if( binding.Right != null ) Unbind( binding.Window, binding.Right.Window );
        }

        public void Stop()
        {
            WindowManager.Service.Registered -= OnRegistered;
            WindowManager.Service.Unregistered -= WindowManager_Unregistered;
            Config.User.Set( "SerializableBindings", _persistantBindings );
        }

        public void Teardown()
        {
        }

        #endregion

        #region Bind Results

        class NullResult : IBindResult
        {
            public static IBindResult Default = new NullResult();

            public void Seal()
            {
            }
        }

        class BindResult : IBindResult
        {
            WindowElementBinder _binder;
            SimpleBinding _simpleBinding;
            public BindResult( WindowElementBinder binder, SimpleBinding simpleBinding )
            {
                _binder = binder;
                _simpleBinding = simpleBinding;
            }

            public void Seal()
            {
                Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );
                //Console.WriteLine( "BEFORE BIND ! Origin : {0} Target : {1} ;;; BEFORE BIND thread id: {2} TimeSpan : {3}", _simpleBinding.Origin.Name, _simpleBinding.Target.Name, Thread.CurrentThread.ManagedThreadId, DateTime.Now.Ticks );

                _binder.Bind( _simpleBinding.Target, _simpleBinding.Origin, _simpleBinding.Position, true );

                //Console.WriteLine( "AFTER BIND ! Origin : {0} Target : {1} ;;; AFTER BIND thread id: {2} TimeSpan : {3}", _simpleBinding.Origin.Name, _simpleBinding.Target.Name, Thread.CurrentThread.ManagedThreadId, DateTime.Now.Ticks );
            }
        }

        class UnbindResult : IBindResult
        {
            WindowElementBinder _binder;
            SimpleBinding _simpleBinding;
            public UnbindResult( WindowElementBinder binder, SimpleBinding simpleBinding )
            {
                _binder = binder;
                _simpleBinding = simpleBinding;
            }

            public void Seal()
            {
                _binder.Unbind( _simpleBinding.Target, _simpleBinding.Origin, false );
            }
        }

        #endregion

        internal static BindingPosition GetOppositePosition( BindingPosition position )
        {
            if( position == BindingPosition.Bottom ) return BindingPosition.Top;
            if( position == BindingPosition.Top ) return BindingPosition.Bottom;
            if( position == BindingPosition.Left ) return BindingPosition.Right;
            return BindingPosition.Left;
        }
    }

}
