namespace DTools.Suice.Exception
{
    /// <summary>
    /// When attempting to inject a dependency that does not exist, this will be thrown.
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal class InvalidDependencyException : System.Exception
    {
        private const string EXCEPTION_MESSAGE = "Requested dependency {0} which does not exist!";

        public InvalidDependencyException(string requestedType)
            : base(string.Format(EXCEPTION_MESSAGE, requestedType)) { }
    }
}
