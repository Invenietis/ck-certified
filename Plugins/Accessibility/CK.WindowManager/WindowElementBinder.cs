using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public WindowElementBinder()
        {
            _bindings = new Dictionary<IWindowElement, List<IBinding>>();
            _logger = new DefaultActivityLogger();
            _logger.Tap.Register( new ActivityLoggerConsoleSink() );
        }

        public event EventHandler<WindowBindedEventArgs> PreviewBinding;

        public event EventHandler<WindowBindingEventArgs> BeforeBinding;

        public event EventHandler<WindowBindedEventArgs> AfterBinding;

        public ISpatialBinding GetBinding( IWindowElement referential )
        {
            if( referential == null ) throw new ArgumentNullException( "referential" );

            SpatialBinding b = null;
            _spatialBindings.TryGetValue( referential, out b );
            return b ?? new SpatialBinding( referential );
        }

        private bool CanBind( IWindowElement master, IWindowElement slave, BindingPosition position, out SpatialBinding spatialBinding, out SpatialBinding slaveSpatialBinding )
        {
            slaveSpatialBinding = null;
            using( _logger.OpenGroup( LogLevel.Info, "Master binding..." ) )
            {
                if( _spatialBindings.TryGetValue( master, out spatialBinding ) )
                {
                    if( position == BindingPosition.Top && spatialBinding.Top != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", spatialBinding.Top.Window.Name, master.Name, position );
                        return false;
                    }
                    else if( position == BindingPosition.Left && spatialBinding.Left != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", spatialBinding.Left.Window.Name, master.Name, position );
                        return false;
                    }
                    else if( position == BindingPosition.Bottom && spatialBinding.Bottom != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", spatialBinding.Bottom.Window.Name, master.Name, position );
                        return false;
                    }
                    else if( position == BindingPosition.Right && spatialBinding.Right != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", spatialBinding.Right.Window.Name, master.Name, position );
                        return false;
                    }
                    else _logger.Trace( "{0} already exists in bindings but no window attached at position {1}.", master.Name, position );
                }
                else
                {
                    _logger.Trace( "Fresh new window" );
                    //spatialBinding = new SpatialBinding( master );
                }
            }

            using( _logger.OpenGroup( LogLevel.Info, "Slave binding..." ) )
            {
                if( _spatialBindings.TryGetValue( slave, out slaveSpatialBinding ) )
                {
                    if( position == BindingPosition.Top && slaveSpatialBinding.Bottom != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", slaveSpatialBinding.Bottom.Window.Name, master.Name, position );
                        return false;
                    }
                    else if( position == BindingPosition.Left && slaveSpatialBinding.Right != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", slaveSpatialBinding.Right.Window.Name, master.Name, position );
                        return false;
                    }
                    else if( position == BindingPosition.Bottom && slaveSpatialBinding.Top != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", slaveSpatialBinding.Top.Window.Name, master.Name, position );
                        return false;
                    }
                    else if( position == BindingPosition.Right && slaveSpatialBinding.Left != null )
                    {
                        _logger.Trace( "{0} is already bound to {1} at position {2}.", slaveSpatialBinding.Left.Window.Name, master.Name, position );
                        return false;
                    }
                    else _logger.Trace( "{0} already exists in bindings but no window attached at position {1}.", slave.Name, position );
                }
                else
                {
                    _logger.Trace( "Fresh new window" );
                    //spatialBinding = new SpatialBinding( slave );
                }
            }

            return true;
        }

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
                _binder.Bind( _simpleBinding.Master, _simpleBinding.Slave, _simpleBinding.Position );
            }
        }

        public IBindResult PreviewBind( IWindowElement master, IWindowElement slave, BindingPosition position )
        {
            if( master == null ) throw new ArgumentNullException( "master" );
            if( slave == null ) throw new ArgumentNullException( "slave" );

            SpatialBinding spatialBinding = null;
            SpatialBinding slaveSpatialBinding = null;

            if( CanBind( master, slave, position, out spatialBinding, out slaveSpatialBinding ) )
            {
                var binding = new SimpleBinding
                {
                    Master = master,
                    Slave = slave,
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

        public IBindResult PreviewUnbind( IWindowElement master, IWindowElement slave )
        {
            var binding = new SimpleBinding
            {
                Master = master,
                Slave = slave
            };

            var evt = new WindowBindedEventArgs
            {
                Binding = binding,
                BindingType = BindingEventType.Detach
            };

            if( PreviewBinding != null )
                PreviewBinding( this, evt );

            return NullResult.Default;
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
                        Master = master,
                        Slave = slave,
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

        IDictionary<IWindowElement, SpatialBinding> _spatialBindings = new Dictionary<IWindowElement, SpatialBinding>();

        internal static BindingPosition GetOppositePosition( BindingPosition position )
        {
            if( position == BindingPosition.Bottom ) return BindingPosition.Top;
            if( position == BindingPosition.Top ) return BindingPosition.Bottom;
            if( position == BindingPosition.Left ) return BindingPosition.Right;
            return BindingPosition.Left;
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
                    Master = me,
                    Slave = other
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

        class SimpleBinding : IBinding
        {
            public IWindowElement Master { get; set; }

            public IWindowElement Slave { get; set; }

            public BindingPosition Position { get; set; }

            public override bool Equals( object obj )
            {
                var binding = obj as IBinding;
                if( binding != null )
                {
                    return ReferenceEquals( binding.Master, Master ) && ReferenceEquals( binding.Slave, Slave );
                }
                return false;
            }

            public override int GetHashCode()
            {
                return Master.GetHashCode() ^ Slave.GetHashCode();
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

    }
}
