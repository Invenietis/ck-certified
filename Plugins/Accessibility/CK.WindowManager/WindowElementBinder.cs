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

        public ICKReadOnlyCollection<IWindowElement> GetAttachedElements( IWindowElement referential )
        {
            if( referential == null ) throw new ArgumentNullException( "referential" );

            List<IBinding> bindings = null;
            if( _bindings.TryGetValue( referential, out bindings ) )
            {
                IList<IWindowElement> list = new List<IWindowElement>();

                // TODO: improved algo
                GetAttachedElements( referential, bindings, list );
                list.Remove( referential );
                return list.ToReadOnlyCollection();
            }
            return CKReadOnlyListEmpty<IWindowElement>.Empty;
        }

        void GetAttachedElements( IWindowElement referential, List<IBinding> bindings, IList<IWindowElement> attached )
        {
            if( bindings == null || bindings.Count == 0 ) return;

            for( int i = 0; i < bindings.Count; ++i )
            {
                IBinding binding = bindings[i];
                if( binding.Master != referential && attached.IndexOf( binding.Master ) == -1 )
                {
                    attached.Add( binding.Master );
                    GetAttachedElements( binding.Master, _bindings[binding.Master], attached );
                }
                else if( binding.Slave != referential && attached.IndexOf( binding.Slave ) == -1 )
                {
                    attached.Add( binding.Slave );
                    GetAttachedElements( binding.Slave, _bindings[binding.Slave], attached );
                }
            }
        }


        public void Attach( IWindowElement master, IWindowElement slave, BindingPosition position )
        {
            if( master == null ) throw new ArgumentNullException( "master" );
            if( slave == null ) throw new ArgumentNullException( "slave" );

            var binding = new SimpleBinding
            {
                Master = master,
                Slave = slave,
                Position = position
            };

            List<IBinding> list = null;

            // If an attachement already exists, this attachement is canceled. 
            // The usage is to detach and the re attach. Cannot replace alread attached window elements.
            if( _bindings.TryGetValue( master, out list ) && list.Contains( binding ) )
            {
                // If the binding exists in master / slave, the binding must exist in slave / master.
                Debug.Assert( _bindings.TryGetValue( slave, out list ) && list.Contains( binding ) );
                // In addition, the binding position must be the opposite
                return;
            }

            var evt = new WindowBindingEventArgs { Binding = binding, BindingType = BindingEventType.Attach };
            if( BeforeBinding != null ) BeforeBinding( this, evt );

            if( evt.Canceled == false )
            {
                Link( master, binding );
                var oppositeBinding = new SimpleBinding
                {
                    Master = slave,
                    Slave = master,
                    Position = WindowElementBinder.GetOppositePosition( position )
                };
                Link( slave, oppositeBinding );

                var evtAfter = new WindowBindedEventArgs { Binding = binding, BindingType = BindingEventType.Attach };
                if( AfterBinding != null ) AfterBinding( this, evtAfter );
            }
        }

        internal static BindingPosition GetOppositePosition( BindingPosition position )
        {
            if( position == BindingPosition.Bottom ) return BindingPosition.Top;
            if( position == BindingPosition.Top ) return BindingPosition.Bottom;
            if( position == BindingPosition.Left ) return BindingPosition.Right;
            return BindingPosition.Left;
        }

        private void Link( IWindowElement window, SimpleBinding binding )
        {
            var bindings = _bindings.GetOrSet( window, ( w ) => new List<IBinding>() );
            bindings.Add( binding );
        }

        public void Detach( IWindowElement me, IWindowElement other )
        {
            if( me == null ) throw new ArgumentNullException( "me" );
            if( other == null ) throw new ArgumentNullException( "other" );

            var binding = new SimpleBinding 
            { 
                Master = me, 
                Slave = other 
            };

            List<IBinding> list = null;
            bool isBindingExists = _bindings.TryGetValue( binding.Master, out list ) && list.Contains( binding );
            if( isBindingExists )
            {
                var evt = new WindowBindingEventArgs { Binding = binding, BindingType = BindingEventType.Detach };
                if( BeforeBinding != null ) BeforeBinding( this, evt );

                if( evt.Canceled == false )
                {
                    var bindingsA = _bindings[binding.Master];
                    if( bindingsA != null ) bindingsA.Remove( binding );

                    var bindingsB = _bindings[binding.Slave];
                    if( bindingsB != null ) bindingsB.Remove( binding );

                    var evtAfter = new WindowBindedEventArgs { Binding = binding, BindingType = BindingEventType.Detach };
                    if( AfterBinding != null ) AfterBinding( this, evtAfter );
                }
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
