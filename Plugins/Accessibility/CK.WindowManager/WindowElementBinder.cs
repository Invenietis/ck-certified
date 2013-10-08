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

        public WindowElementBinder()
        {
            _bindings = new Dictionary<IWindowElement, List<IBinding>>();
        }

        public event EventHandler<WindowBindingEventArgs> BeforeBinding;

        public event EventHandler<WindowBindedEventArgs> AfterBinding;

        public ISpatialBinding GetBinding( IWindowElement referential )
        {
            if( referential == null ) throw new ArgumentNullException( "referential" );
            
            SpatialBinding b = null;
            _spatialBindings.TryGetValue( referential, out b );
            return b;
        }

        public void Attach( IWindowElement master, IWindowElement slave, BindingPosition position )
        {
            if( master == null ) throw new ArgumentNullException( "master" );
            if( slave == null ) throw new ArgumentNullException( "slave" );

            // Spatial binding point of view

            bool isNew = true;
            SpatialBinding spatialBinding = null;
            if( _spatialBindings.TryGetValue( master, out spatialBinding ) )
            {
                isNew = false;
                if( position == BindingPosition.Top && spatialBinding.Top != null ) return;
                if( position == BindingPosition.Left && spatialBinding.Left != null ) return;
                if( position == BindingPosition.Bottom && spatialBinding.Bottom != null ) return;
                if( position == BindingPosition.Right && spatialBinding.Right != null ) return;
            }
            else spatialBinding = new SpatialBinding( master );

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

            if( evt.Canceled == false )
            {
                if( isNew ) _spatialBindings.Add( master, spatialBinding );

                SpatialBinding slaveSpatialBinding = null;
                if( position == BindingPosition.Top ) spatialBinding.Top = slaveSpatialBinding = new SpatialBinding( slave ) { Bottom = spatialBinding };
                if( position == BindingPosition.Left ) spatialBinding.Left = slaveSpatialBinding = new SpatialBinding( slave ) { Right = spatialBinding };
                if( position == BindingPosition.Bottom ) spatialBinding.Bottom = slaveSpatialBinding = new SpatialBinding( slave ) { Top = spatialBinding };
                if( position == BindingPosition.Right ) spatialBinding.Right = slaveSpatialBinding = new SpatialBinding( slave ) { Left = spatialBinding };

                if( isNew )
                {
                    Debug.Assert( slaveSpatialBinding != null );
                    _spatialBindings.Add( slave, slaveSpatialBinding );
                }
                var evtAfter = new WindowBindedEventArgs
                {
                    Binding = binding,
                    BindingType = BindingEventType.Attach
                };
                if( AfterBinding != null )
                    AfterBinding( this, evtAfter );
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

        public void Detach( IWindowElement me, IWindowElement other )
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
                        Debug.Assert( spatialBinding.Bottom.Top != null );
                        Detach( other, me );
                        spatialBinding.Bottom = null;
                    }
                    if( spatialBinding.Left != null && spatialBinding.Left.Window == other )
                    {
                        Debug.Assert( spatialBinding.Left.Right != null );
                        Detach( other, me );
                        spatialBinding.Left = null;
                    }
                    if( spatialBinding.Top != null && spatialBinding.Top.Window == other )
                    {
                        Debug.Assert( spatialBinding.Top.Bottom != null );
                        Detach( other, me );
                        spatialBinding.Top = null;
                    }
                    if( spatialBinding.Right != null && spatialBinding.Right.Window == other )
                    {
                        Debug.Assert( spatialBinding.Right.Left != null );
                        Detach( other, me );
                        spatialBinding.Right = null;
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
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
