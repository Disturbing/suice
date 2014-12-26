namespace DTools.Suice.Exception
{
    /// <summary>
    /// A valid constructor of the following must be defined for dependencies in Suice.
    /// 
    /// 1) No Constructor
    /// 2) An empty Constructor
    /// 3) A single non-empty constructor with the [Inject] attribute attached to it
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal class InvalidDependencyConstructorException : System.Exception
    {
        private const string exceptionMessage = "Could not find valid constructor for type {0}." +
                                                "Type must have no constructor, an empty constructor, or a constructor with an [Inject] attribute!";

        public InvalidDependencyConstructorException(string typeName)
            : base(string.Format(exceptionMessage, typeName)) { }
    }
}
