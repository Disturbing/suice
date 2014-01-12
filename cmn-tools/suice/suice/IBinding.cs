using System;

namespace Toolbox.Injection
{
	public interface IBinding
	{
		Type TypeToBind { get; }

		Type BindedType { get; }

		Scope Scope { get; }

		object BindedInstance { get; }
	}
}

