namespace Suice.Exception
{
    /// <summary>
    /// All types can have only a single defined binding.
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal class DuplicateBindingException : System.Exception
    {
        private const string EXCEPTION_MESSAGE = "Attempted to bind type {0} twice! Attempted to bind to {1} and {2}";

        public DuplicateBindingException(string typeName, string alreadyBoundType, string requestedType)
            : base(string.Format(EXCEPTION_MESSAGE, typeName, alreadyBoundType, requestedType)) { }
    }
}
