using System;
using System.Reflection;

namespace Toolbox.Injection
{
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

