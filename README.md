Suice – The Ultimate C# Dependency Injection Framework

The Suice project is an open-source project with the goal of creating an efficient Just In Time (JIT) dependency injection framework with the same awesome features Google’s Guice.
Suice provides a strong depdency injection framework which will aid you in creating great test driven APIs. Destroying design patterns such as the singleton and replacing the ‘new’ keyword with [Inject] and factories will aid you in creating clean testable code.

Overview
The Suice framework begins with the Injector class.  You must first instantiate an injector instance and register desired modules.

This is an example of creating the Injector and initializing it after registering a module.

<code>
Using CmnTools.Suice;

public class MainAppClass
{
	private static Injector _injector;

	static void Main(string[] args)
	{
		SetupInjector();
	}

	private static void SetupInjector()
	{
		_injector = new Injector();
		RegisterModules(_injector);
		_injector.Init();
	}

	private static void RegisterModules(Injector injector)
	{
		injector.RegisterModule(new ExampleModule());
	} 
}

</code>

Modules are classes that define factories, singletons and instances to specific implementations.  For instance, if you want to specify that the INetworkService will be bound to the NetworkService implementation as a singleton instance type, you would do the following.

<code>
using CmnTools.Suice;
public class ExampleModule : AbstractModule
{
	Bind<INetworkService>().To<NetworkService>().In(Scope.SINGLETON);
}

</code>

There are two scope types:
* Scope.SINGLETON
* Scope.NO_SCOPE
If you define a scope as NO_SCOPE, which is by default for all bindings, it will create a new instance for every request of the dependency type.
Modules also allow you to implement the factory pattern through methods. The following example shows how to create a new connection using the factory pattern which requires the dependency ISocket.  For example

<code>
[Provides(Scope.NO_SCOPE)]
public IConnection provideNewNetworkConnection(ISocket socket) {
	IConnection connection = new Connection(socket);
	connection.Connect();
	return connection; 
}

</code>

Modules are great for defining conditional runtime or compile-time implementations for different platforms.  Although, the easiest way to create a dependency is using Just In Time (JIT) attributes.  JIT attributes are flags on classes and interfaces that can be defined to automatically create dependencies through reflection. There are several JIT attributes.

First, the Singleton attribute.  You may mark a class as a [Singleton], as shown in the following example, which will bind that class and inject necessary dependencies through field injection or constructor injection. 

The act of injection is providing a dependency to an instance.   Shown in the example above in creating a network connection, the ISocket is a dependency for IConnection.  In order to inject into a class through a constructor or parameter, you must attach the [Inject] attribute. 

Here’s an example of using Constructor Injection with the Singleton attribute:

<code>

using CmnTools.Suice;

[Singleton]
public class AchievementService
{
	private readonly IAchievementDao _achievementDao;

	[Inject]
	public AchievementService(IAchievementDao achievementDao) {
		_achievementDao = achievementDao;
	} 
}

</code>

For best practices, it’s best not to ever do business logic in a constructor.  If you wish to have initialization logic, you can implement the interface InitializeDepenency which will be called after class has been constructed as shown below:

<code>

using CmnTools.Suice;

[Singleton]
public class StaticData : InitializeDependency
{
	private readonly XmlParser _xmlParser;

	private string data;

	[Inject]
	public StaticData (XmlParser _xmlParser) {
		_achievementDao = achievementDao;
	} 

	public void Initialize() {
		this.data = _xmlParser.ReadData(“Path/To/File”);
	}
}

</code>

Field injection should only be used if and only if you require circular dependency.  Due to the limitations of proxies, circular dependency is not supported through constructor injection.   It is not a best practice to use field injection because it limits the business logic from being testable. Additionally, if circular dependency use case occurs, it is usually a good sign that a better design may be available for the solution.

The following shows how you can inject a dependency through field injection:

<code>

using CmnTools.Suice;

[Singleton]
public class StaticData : IStaticData
{
	[Inject]
	private XmlParser _xmlParser;

	void LoadData() {

	}

	void UnloadData() {

	}

	Template GetTemplate(int templateId) {
		return null;
	}
}

</code>

It’s best to always create an interface for your dependencies so they may be Mocked during test driven development.  This can be done using the JIT attribute ImplementedBy.  Implemented can be placed on an interface with the defined class type that will be implemented during run time.  Here’s an example:

<code>

using CmnTools.Suice;

[ImplementedBy(typeof(StaticData))]
public interface IStaticData
{
	void LoadData();
	void UnloadData();

	Template GetTemplate(int templateId); 
}

</code>

This code will work perfectly fine with the example singleton StaticData implementation above.
Finally, we can provide define JIT factories using the ProvidedBy attribute.  This attribute can be set on an interface which defines a class which implements Provider<InterfaceToProvide>.  Here’s an example:

<code>

using CmnTools.Suice;

[ProvidedBy(typeof(ItemExampleProvider))]
public interface IItemExample {
	int GetItemId(); 
}

public class ItemExampleProvider : Provider<IItemExample> {

	private readonly IStaticData _staticData;

	public override IItemExample Provide() {
		IItemExample itemExample = new ItemExample(_staticdata.GetTemplate(Rnd.Get(0,1))(;

		return itemExample;
	}
}

</code>


 

