namespace Suice.Exception
{
    /// <summary>
    /// When two dependencies require each other during constructor injection, this exception will be throw.
    /// Circular dependency is not possible due to the limitations of the language's proxy capabilities.
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal class CircularDependencyException : System.Exception
    {
        private const string EXCEPTION_MESSAGE =
            "Detected Circular Dependency! Circular Map: {0}.  Use Field Injection as an alternative.";

        public CircularDependencyException(string circularDependencyMap)
            : base(string.Format(EXCEPTION_MESSAGE, circularDependencyMap)) { }
    }
}
