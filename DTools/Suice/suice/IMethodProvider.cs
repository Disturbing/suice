using System.Reflection;

namespace DTools.Suice
{
    /// <summary>
    /// Implementation for obtaining methods for construction via providers
    /// 
    /// @author DisTurBinG
    /// </summary>
    public interface IMethodProvider
    {
        MethodInfo GetMethod();
    }
}