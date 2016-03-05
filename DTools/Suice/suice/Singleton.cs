using System;

namespace DTools.Suice
{
    /// <summary>
    /// Singleton attribute flag, which marks a class should be instantiated as a singleton instance in the Injector.
    /// 
    /// @author DisTurBinG
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class Singleton : Attribute
    {
        public Scope Scope;
        private class InvalidSingletonScopeException : System.Exception { }

        public Singleton(Scope scope = Suice.Scope.SINGLETON)
        {
            if (scope == Scope.NO_SCOPE) {
                throw new InvalidSingletonScopeException();
            }

            Scope = scope;
        }
    }
}