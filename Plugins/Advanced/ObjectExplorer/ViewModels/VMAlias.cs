namespace CK.Plugins.ObjectExplorer
{
    public class VMAlias<T> : VMICoreElement
        where T : VMICoreElement
    {
        T _wrapped;

        public override object Data { get { return _wrapped; } }

        public VMAlias( T wrapped, VMIBase parent ) :
            base( wrapped.VMIContext, parent )
        {
            _wrapped = wrapped;
        }
    }
}
