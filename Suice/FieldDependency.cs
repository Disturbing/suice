using System.Reflection;

namespace Suice
{
    /// <summary>
    /// Field dependency container.
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal struct FieldDependency
    {
        internal readonly FieldInfo FieldInfo;
        internal readonly object DependencyInstance;

        internal FieldDependency(FieldInfo fieldInfo, object dependencyInstance)
        {
            FieldInfo = fieldInfo;
            DependencyInstance = dependencyInstance;
        }
    }
}