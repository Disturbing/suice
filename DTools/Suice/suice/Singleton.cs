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
        private class InvalidSingletonScopeException : System.Exception { }

        public readonly Scope Scope;

        public Singleton(Scope scope = Scope.SINGLETON)
        {
            if (scope == Scope.NO_SCOPE) {
                throw new InvalidSingletonScopeException();
            }

            Scope = scope;
        }
    }
}