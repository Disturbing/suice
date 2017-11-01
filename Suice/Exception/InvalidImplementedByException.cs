namespace Suice.Exception
{
    /// <summary>
    /// Thrown when implemented by attribute was not setup correctly.
    /// ImplementedBy attribute must be define a class that inherits the type that the implemented by attribute is attached to.
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal class InvalidImplementedByException : System.Exception
    {
        private const string EXCEPTION_MESSAGE = "ImplementedBy attribute on interface {0} " +
                                                "has invalid set implementation class: {1} which does not inherit {0}";

        public InvalidImplementedByException(string interfaceType, string instanceType)
            : base(string.Format(EXCEPTION_MESSAGE, interfaceType, instanceType)) { }
    }
}
