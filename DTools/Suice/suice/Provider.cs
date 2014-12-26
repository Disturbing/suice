namespace DTools.Suice
{
    /// <summary>
    /// Abstracted factory pattern for the Dependency Injector
    /// 
    /// @author DisTurBinG
    /// </summary>
    public abstract class Provider<T> : AbstractProvider
    {
        protected Provider() : base(typeof (T), typeof (T)) { }

        protected override object ProvideObject()
        {
            return Provide();
        }

        public new abstract T Provide();
    }
}