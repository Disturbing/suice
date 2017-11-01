namespace Suice.Exception
{
    /// <summary>
    /// Thrown when a dependency attempts to inject itself as a dependency.
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal class InjectToSelfException : System.Exception
    {
        private const string EXCEPTION_MESSAGE = "Dependency {0} is attempting to inject itself through field injection!";

        public InjectToSelfException(string typeName)
            : base(string.Format(EXCEPTION_MESSAGE, typeName)) { }
    }
}
