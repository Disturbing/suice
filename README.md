# Suice – The Ultimate C# Dependency Injection Framework "Inspired by Google's Guice"

## Overview
The Suice project is an open-source project with the goal of creating an efficient Just In Time (JIT) dependency injection framework with the same awesome features Google’s Guice.
Suice provides a strong depdency injection framework which will aid you in creating great test driven APIs. Destroying design patterns such as the singleton and replacing the ‘new’ keyword with [Inject] and factories will aid you in creating clean testable code.

## Startup and Setup
The Suice framework begins with the Injector class.  You must first instantiate an injector instance and register desired modules.

This is an example of creating the Injector and initializing it after registering a module.

```
using DTools.Suice;

public class MainAppClass
{
	private static Injector injector;

	static void Main(string[] args)
	{
		SetupInjector();
	}

	private static void SetupInjector()
	{
		injector = new Injector();
		RegisterModules(injector);
		injector.Init();
	}

	private static void RegisterModules(Injector injector)
	{
		injector.RegisterModule(new ExampleModule());
	} 
}
```

## Modules
Modules are classes that define factories, singletons and instances to specific implementations.  For instance, if you want to specify that the INetworkService will be bound to the NetworkService implementation as a singleton instance type, you would do the following.

Modules also allow you to implement the factory pattern through methods using the Provides attribute. The following example shows how to create a new connection using the factory pattern which requires the dependency ISocket.  

```
using DTools.Suice;

public class ExampleModule : AbstractModule
{
	public override void Configure() {
		Bind<INetworkService>().To<NetworkService>().In(Scope.SINGLETON);
	}
	
	[Provides(Scope.NO_SCOPE)]
	public IConnection provideNewNetworkConnection(ISocket socket) {
		IConnection connection = new Connection(socket);
		connection.Connect();
		
		return connection; 
	}
}
```

## Dependency Scopes

There are three scope types:
* Scope.SINGLETON - On Demand based singleton which creates a single shared dependency instance when a class requests it.
* Scope.NO_SCOPE - Create a new instance for every request of this class.
* Scope.EAGER_SINGLETON - Instantiates instance even if it is not referenced as a dependency to another class.  This will create it at startup.  Should have at least ones of these in every application to kickoff the instantiation of others.

> TIP: Modules are great for defining conditional runtime or compile-time implementations for different platforms.  Although, the easiest way to create a dependency is using Just In Time (JIT) attributes.  JIT attributes are flags on classes and interfaces that can be defined to automatically create dependencies through reflection. There are several JIT attributes.

First, the Singleton attribute.  You may mark a class as a [Singleton], as shown in the following example, which will bind that class and inject necessary dependencies through field injection or constructor injection. 

The act of injection is providing a dependency to an instance.   Shown in the example above in creating a network connection, the ISocket is a dependency for IConnection.  In order to inject into a class through a constructor or parameter, you must attach the [Inject] attribute. 

Here’s an example of using Constructor Injection with the Singleton attribute:

```
using DTools.Suice;

[Singleton]
public class AchievementService : IInitializable, IAchievementService
{
	private readonly IAchievementDao achievementDao;
	private AchievementTemplates[] templates;

	[Inject]
	public AchievementService(IAchievementDao achievementDao) {
		this.achievementDao = achievementDao;
	} 

	public void Initialize() {
		this.templates = achievementDao.LoadTemplates();
	}
}
```

For best practices, it’s best not to ever do business logic in a constructor.  If you wish to have initialization logic, you can implement the interface IInitializable which will be called after class has been constructed as shown above as well!

## Field Injection

Field injection is a feature I plan to take out in the near future.  Suice 2.0 provides circular dependency support through constructor injection, which is the best way to plan your clean code for 3 key reasons:

1) Field injection is not easily testable via Unit Testing.  You have to mark the fields public in order to mock them.
2) Can't mark field injection as readonly, which means someone else can accidently modify it!
3) Seeing too many values being passed into a constructor usually warns that the class is doing too much work and can be separated.

The following shows how you can inject a dependency through field injection:

```
using DTools.Suice;

[Singleton]
public class AchievementService : IAchievementService 
{
	[Inject]
	private IAchievementDao achievementDao;
	
	public StaticData()
	{
	
	}
}
```

It’s best to always create an interface for your dependencies so they may be Mocked during test driven development and is an overall great process to make things modular and flexible.  This can be done using the JIT attribute ImplementedBy.  Implemented can be placed on an interface with the defined class type that will be implemented during run time.  Here’s an example:

```
using CmnTools.Suice;

[ImplementedBy(typeof(StaticData))]
public interface IStaticData
{
	void LoadData();
	void UnloadData();
	Template GetTemplate(int templateId); 
}

[Singleton]
public class StaticData : IStaticData
{
	public void LoadData() {} 
	public void UnloadData() {}
	public Template GetTemplate(int templateId) { return null; }
}
```

## Circular Dependency

This is a huge feature and one of the reasons why I built suice in the first place.  Sometimes Dep A needs Dep B and Dep B needs Dep A.  It's much easier to do this versus creating a bunch of event / actions as callbacks with a one way relation depending on the solution.

Although some people would disagree that circular dependency usually flags that there's an issue with the system as a whole.  My company actually stopped using Suice for this reason, after building a complex 500K+ Line project or even bigger, when a circular dependency comes in and coming up with a solution to avoid it sometimes takes days.  It's better to hack with it and flag for a future refactor later.  Therefore, I've made this available!

I'm using proxies in order to solve the solution of circular dependency, and it requires this library - https://github.com/castleproject/Core

Due to the limitations of C#, I've only supported Interfaces, as regular classes can't be properly proxied unless you make all the functions virtual!  I miss Java <3 because Guice can properly override all of these.  This means that circular dependencies can only reference interfaces, not their implementations which can be easily setup in the IStaticData example above.  At the end of the day - in C# to make things properly testable with any mocking framework, you must use interfaces as they have the same limitations of not being able to override normal functions.

## Dynamic Providers

Huge request from private individuals.  People are tired of making factories manually, and they wanted post-startup injection into new constructed classes during runtime.  Guice solved this using dynamic providers and I just never implemented it here.

Imagine wanting a list of items, and all those list of items you want access to their services to send requests for buying for instance.  You would need the Buy Service and need to create a dynamic list on runtime based on whatever you want.

This is how you would do it:

```
// NOTE: This is sudo code below

[Singleton]
public class BuyService
{
	public void Buy(int shopId)
	{
		//Do cool stuff with shopId;
	}
}

[Singleton]
public class ShopService
{
	private readonly List<ShopItem> shopItems = new List<ShopItem>();
	private readonly IProvider<ShopItem> shopItemProvider;
	private readonly PopupService popupService;
	
	public ShopService(IProvider<ShopItem> shopItems, PopupService popupService)
	{
		this.shopItems = shopItems;
		this.popupService = popupService;
	}
	
	public void SetupShop(ShopItemTemplate[] templates)
	{
		foreach(ShopItemTemplate template in templates) {
			ShopItem shopItem = shopItemProvider.Provide();
			shopItem.Template = template;
			shopItems.Add(shopItem);
		}
	}
	
	public void ShowShopUI()
	{
		popupService.ShowShop(shopItems);
	}
}

// This can be injectable because it will be created via IProvider!
public class ShopItem
{
	private readonly BuyService buyService;
	public ShopItemTemplate Template;
	
	public void OnClick()
	{
		buyService.Buy(Template.Id);
	}
}
```

One feature to note, in the future, we should be able to pass dynamic constructor requirements through the IProvider.  IE: provider.Provide(template) which will allow dynamic constructor reqs + injectable. Guice has it, suice should too :).

 

