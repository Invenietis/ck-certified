using System;
using System.Collections.Generic;
using CK.Plugin;
using CK.WindowManager.Model;
using CK.Core;
using System.Diagnostics;

namespace CK.WindowManager
{
    [Plugin( "{F6B5D818-3C04-4A46-AD65-AFC5458A394C}", Categories = new string[] { "Accessibility" }, PublicName = "CK.WindowManager.WindowElementBinder", Version = "1.0.0" )]
    public class WindowElementBinder : IWindowBinder, IPlugin
    {
        IDictionary<IWindowElement,List<IBinding>> _bindings;
        DefaultActivityLogger _logger;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWindowManager WindowManager { get; set; }

        IDictionary<IWindowElement, SpatialBinding> _spatialBindings = new Dictionary<IWindowElement, SpatialBinding>();

        public WindowElementBinder()
        {
            _bindings = new Dictionary<IWindowElement, List<IBinding>>();
            _logger = new DefaultActivityLogger();
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

        public void Bind( IWindowElement master, IWindowElement slave, BindingPosition position )
        {
            if( master == null ) throw new ArgumentNullException( "master" );
            if( slave == null ) throw new ArgumentNullException( "slave" );

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

        public void Unbind( IWindowElement me, IWindowElement other )
        {
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
                        spatialBinding.Bottom = null;
                        Unbind( other, me );
                    }
                    if( spatialBinding.Left != null && spatialBinding.Left.Window == other )
                    {
                        spatialBinding.Left = null;
                        Unbind( other, me );
                    }
                    if( spatialBinding.Top != null && spatialBinding.Top.Window == other )
                    {
                        spatialBinding.Top = null;
                        Unbind( other, me );
                    }
                    if( spatialBinding.Right != null && spatialBinding.Right.Window == other )
                    {
                        spatialBinding.Right = null;
                        Unbind( other, me );
                    }
                    if( spatialBinding.IsAlone )
                        _spatialBindings.Remove( me );
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
            WindowManager.Unregistered += WindowManager_Unregistered;
        }

        void WindowManager_Unregistered( object sender, WindowElementEventArgs e )
        {
            var binding = GetBinding( e.Window );
            if( binding.Bottom != null ) Unbind( binding.Window,  binding.Bottom.Window );
            if( binding.Top != null ) Unbind( binding.Window, binding.Top.Window );
            if( binding.Left != null ) Unbind( binding.Window, binding.Left.Window );
            if( binding.Right != null ) Unbind( binding.Window, binding.Right.Window );
        }

        public void Stop()
        {
            WindowManager.Unregistered -= WindowManager_Unregistered;
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
                _binder.Bind( _simpleBinding.Target, _simpleBinding.Origin, _simpleBinding.Position );
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
                _binder.Unbind( _simpleBinding.Target, _simpleBinding.Origin );
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
