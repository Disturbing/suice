using System;
using System.Collections.Generic;

namespace Toolbox.Injection
{
	/// <summary>
	/// Implement Abstract Module to register custom binding rules
	/// 
	/// @author DisTurBinG
	/// </summary>
	public abstract class AbstractModule
	{
		private Injector injector;

		internal HashSet<IBinding> Bindings = new HashSet<IBinding>();

		protected Binding<T> Bind<T>()
		{
			Binding<T> binding = new Binding<T>();
			Bindings.Add (binding);
			return binding;
		}

		public abstract void Configure();
	}
}

