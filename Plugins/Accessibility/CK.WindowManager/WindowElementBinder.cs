using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.WindowManager.Model;
using CK.Core;

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
            List<IBinding> bindings = _bindings[referential];
            IList<IWindowElement> list = new List<IWindowElement>();

            // TODO: improved algo
            GetAttachedElements( referential, bindings, list );
            list.Remove( referential );
            return list.ToReadOnlyCollection();
        }

        void GetAttachedElements( IWindowElement referential, List<IBinding> bindings, IList<IWindowElement> attached )
        {
            if( bindings == null || bindings.Count == 0 ) return;

            for( int i = 0; i < bindings.Count; ++i )
            {
                IBinding binding = bindings[i];
                if( binding.First != referential && attached.IndexOf( binding.First ) == -1 )
                {
                    attached.Add( binding.First );
                    GetAttachedElements( binding.First, _bindings[binding.First], attached );
                }
                else if( binding.Second != referential && attached.IndexOf( binding.Second ) == -1 )
                {
                    attached.Add( binding.Second );
                    GetAttachedElements( binding.Second, _bindings[binding.Second], attached );
                }
            }
        }


        public void Attach( IWindowElement first, IWindowElement second )
        {
            var binding = new SimpleBinding { First = first, Second = second };

            var evt = new WindowBindingEventArgs { Binding = binding, BindingType = BindingEventType.Attach };
            if( BeforeBinding != null ) BeforeBinding( this, evt );

            if( evt.Canceled == false )
            {
                Link( first, binding );
                Link( second, binding );

                var evtAfter = new WindowBindedEventArgs { Binding = binding, BindingType = BindingEventType.Attach };
                if( AfterBinding != null ) AfterBinding( this, evtAfter );
            }
        }

        private void Link( IWindowElement window, SimpleBinding binding )
        {
            var bindings = _bindings.GetOrSet( window, ( w ) => new List<IBinding>() );
            bindings.Add( binding );
        }

        public void Detach( IBinding binding )
        {
            var evt = new WindowBindingEventArgs { Binding = binding, BindingType = BindingEventType.Detach };
            if( BeforeBinding != null ) BeforeBinding( this, evt );

            if( evt.Canceled == false )
            {
                var bindingsA = _bindings[binding.First];
                if( bindingsA != null ) bindingsA.Remove( binding );

                var bindingsB = _bindings[binding.First];
                if( bindingsB != null ) bindingsB.Remove( binding );

                var evtAfter = new WindowBindedEventArgs { Binding = binding, BindingType = BindingEventType.Detach };
                if( AfterBinding != null ) AfterBinding( this, evtAfter );
            }
        }

        class SimpleBinding : IBinding
        {
            public IWindowElement First { get; set; }

            public IWindowElement Second { get; set; }

            public override bool Equals( object obj )
            {
                var binding = obj as IBinding;
                if( binding != null )
                {
                    return ReferenceEquals( binding.First, First ) && ReferenceEquals( binding.Second, Second );
                }
                return false;
            }

            public override int GetHashCode()
            {
                return First.GetHashCode() ^ Second.GetHashCode();
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
