namespace DTools.Suice.Exception
{
    /// <summary>
    /// Thrown when ProvidedBy attribute was not setup properly.
    /// 
    /// The ProvidedBy attribute must point towards an abstract provider which provides an instance
    /// which inherits the type which the ProvidedBy attribute is attached to.
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal class InvalidProvidedByException : System.Exception
    {
        private const string EXCEPTION_MESSAGE = "Provided invalid ProviderType to ProvidedBy Attribute on class: {0}." +
                                                "Attribute must specficy an Factory implementation!";

        public InvalidProvidedByException(string providedByInterface)
            : base(string.Format(EXCEPTION_MESSAGE, providedByInterface)) { }
    }
}
