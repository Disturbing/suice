using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CmnTools.Suice
{
    /// <summary>
    /// Implementation for obtaining methods for construction via providers
    /// 
    /// @author DisTurBinG
    /// </summary>
    public interface IMethodConstructor
    {
        MethodInfo GetMethodConstructor();
    }
}
