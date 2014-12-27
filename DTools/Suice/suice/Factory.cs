namespace DTools.Suice
{
    /// <summary>
    /// Abstracted factory pattern for the Suice.
    /// 
    /// @author DisTurBinG
    /// </summary>
    public abstract class Factory<T> : Provider
    {
        protected Factory() : base(typeof (T), typeof (T)) { }

        protected override object ProvideObject()
        {
            return Provide();
        }

        public new abstract T Provide();
    }
}