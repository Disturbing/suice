using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace Toolbox.Injection
{
    /// <summary>
    /// Suice Injection is Google's Guice port to C#.
    /// 
    /// Injector manages all dependencies within the Application.
    /// Must call Init function after construction.
    /// Automatically handles JIT Dependencies.
    /// 
    /// Currently supports Providers, Singletons, Field Injection, ImplementedBy and ProvidedBy functionalities.
    /// 
    /// Circular Dependency is not supported via Constructor Injection at this time.
    /// 
    /// @author DisTurBinG
    /// </summary>
    public class Injector
    {
        private const BindingFlags DEPENDENCY_VARIABLE_FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;

		private readonly Dictionary<Type, AbstractProvider> providersMap = new Dictionary<Type, AbstractProvider>();

		private readonly Dictionary<Type, IBinding> bindingMap = new Dictionary<Type, IBinding> ();

        public void Init()
        {
            float timeStarted = Time.realtimeSinceStartup;

            InjectDependencies();

            Debug.Log(string.Format("Injector initialized {0} dependencies in: {1}s",
                providersMap.Count,
                Time.realtimeSinceStartup - timeStarted));
        }

        private void InjectDependencies()
        {
			IEnumerable<Type> assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();

            RegisterJustInTimeDependencies(assemblyTypes);

            foreach (KeyValuePair<Type, AbstractProvider> kvp in providersMap
                .Where(x => !(x.Value is SingletonMethodProvider) && x.Value is SingletonProvider))
            {
                GetDependency(kvp.Key);
            }
        }

		public void RegisterModule(AbstractModule module)
		{
            module.Configure();
			RegisterBindings (module.Bindings);
			CreateProvidersFromMethods (module);
		}

		private void CreateProvidersFromBindings(AbstractModule module)
		{
			foreach(IBinding binding in module.Bindings)
			{
				CreateProvider (binding);
			}
		}

		private void CreateProvidersFromMethods(AbstractModule module)
		{
			foreach(MethodInfo methodInfo in module.GetType().GetMethodsWithAttribute<Provides>())
			{
				Provides provides = (Provides)Attribute.GetCustomAttribute (methodInfo, typeof(Provides));

				if (provides.Scope == Scope.NO_SCOPE)
				{
					RegisterProvider(methodInfo.ReturnType, new  MethodProvider (module, methodInfo));
				}
				else if (provides.Scope == Scope.SINGLETON)
				{
					RegisterProvider (methodInfo.ReturnType, new SingletonMethodProvider (module, methodInfo));
				}
				else
				{
					throw new Exception (string.Format (
						"AbstractModule {0}#{1} has unhandled scope type {2}",
						module.GetType().FullName,
						methodInfo.Name,
						provides.Scope));
				}
			}
		}

		private void RegisterProvider(Type bindedType, AbstractProvider provider)
		{
			try
			{
                Debug.Log("Registering provider type: " + bindedType + " to provider: " + provider);
				providersMap.Add(bindedType, provider);
			}
			catch(ArgumentException e)
			{
				throw new Exception(
					string.Format("Attempted to bind {0} to {1} twice! May only specify one implementation type!",
			              bindedType,
			              provider.ProvidedType));
			}
		}

		private void RegisterBindings(IEnumerable<IBinding> bindings)
		{
			foreach (IBinding binding in bindings)
			{
				CreateProvider (binding);
			}
		}

		private object[] GetMethodDependencies(Type dependencyType, MethodBase methodInfo)
		{
			ParameterInfo[] parameterInfos = methodInfo.GetParameters ();
			object[] parameters = new object[parameterInfos.Length];

			for (int i=0; i < parameterInfos.Length; i++)
			{
			    Type parameterType = parameterInfos[i].ParameterType;

                if (dependencyType == parameterType)
                {
                    throw new Exception("Dependency " + dependencyType.FullName + 
                        " is attempting to inject itself through method/constructor injection!");
                }

                parameters[i] = GetDependency(parameterType);
			}

			return parameters;
		}

		private FieldDependency[] GetFieldDependencies(Type type)
		{
            FieldInfo[] fieldInfos = type.GetFields(DEPENDENCY_VARIABLE_FLAGS)
                .Where(f => f.GetCustomAttributes(typeof(Inject), true).Length > 0).ToArray();
			FieldDependency[] fieldDependencies = new FieldDependency[fieldInfos.Length];

			for (int i=0; i < fieldInfos.Length; i++)
			{
			    Type fieldType = fieldInfos[i].FieldType;

                if (fieldType == type)
                {
                    throw new Exception("Dependency " + type.FullName + " is attempting to inject itself through field injection!");
                }

				fieldDependencies[i] = new FieldDependency(fieldInfos[i], GetDependency (fieldType));
			}

			return fieldDependencies;
		}

		private void CreateProvider(IBinding binding)
		{
            Debug.Log("Attempting to register binding: " + binding.BindedType);

			if (binding.Scope == Scope.NO_SCOPE)
			{
				RegisterProvider(binding.TypeToBind,
					new NoScopeProvider (binding.BindedType));
			}
			else if (binding.Scope == Scope.SINGLETON)
			{
				SingletonProvider singletonProvider = new SingletonProvider (binding.BindedType);

				if (binding.BindedInstance != null)
				{
					singletonProvider.SetInstance (binding.BindedInstance);
				}

                Debug.Log("Registering singleton binder!: " + binding.TypeToBind);

				RegisterProvider(binding.TypeToBind, singletonProvider);
			}
			else
			{
				throw new Exception (string.Format (
					"Binding {0} has unhandled scope type {1}",
					binding.TypeToBind.FullName,
					binding.Scope));
			}
		}

		private ConstructorInfo GetConstructor(Type bindedType)
		{
			ConstructorInfo[] constructorInfos = bindedType.GetConstructors();
			ConstructorInfo constructorInfo = null;

			if (constructorInfos.Length == 0)
			{
			    Debug.Log("Returning default constructor!");
				constructorInfo = bindedType.GetConstructor (Type.EmptyTypes);
			} 
			else if (constructorInfos.Length == 1 && IsValidConstructor(constructorInfos[0]))
			{
                Debug.Log("Returning constructor with params : " + constructorInfos[0].GetParameters().Length);
				constructorInfo = constructorInfos [0];
			}

			if (constructorInfo == null)
			{
				throw new Exception (string.Format("Could not find valid constructor for type {0}." +
					"Must provide no constructor, an empty constructor, or a constructor with an [Inject] attribute!",
					bindedType.FullName));
			}

			return constructorInfo;
		}

		private bool IsValidConstructor(ConstructorInfo constructorInfo)
		{
			return constructorInfo.GetParameters().Length == 0 ||
				   constructorInfo.GetMemberInfoAttribute<Inject>() != null;
		}

		private object GetDependency(Type type)
		{
			AbstractProvider provider;

			if (!providersMap.TryGetValue(type, out provider))
			{
                //TODO: Something to experiment with
				//RegisterProvider (type, provider = new NoScopeProvider (type));
			    throw new Exception("Attempted to get dependency: " + type.FullName + " that doesn't exist!");
			}

			if (!provider.IsInitialized)
            {
                IMethodConstructor methodConstructor = provider as IMethodConstructor;
                MethodBase methodBase;

                if (methodConstructor == null)
                {
                    methodBase = GetConstructor(provider.ProvidedType);
                }
                else
                {
                    methodBase = methodConstructor.GetMethodConstructor();
                }

                object[] constructorDependencies = GetMethodDependencies(type, methodBase);


				provider.SetConstructorDependencies(constructorDependencies);

                SingletonProvider singletonProvider = provider as SingletonProvider;

                if (singletonProvider != null)
                {
                    singletonProvider.CreateSingletonInstance();
                }
                else if (provider is ProviderProxy)
                {
                    // Setup proxied provider
                    GetDependency(provider.ProvidedType);
                }
		    }

			object dependency = provider.Provide();

            if (!provider.IsInitialized || provider is NoScopeProvider)
            {
                // This allows singletons to have ciruclar dependency
                provider.IsInitialized = true;

                FieldDependency[] fieldDependencies = GetFieldDependencies(dependency.GetType());

                foreach (FieldDependency fieldDependency in fieldDependencies)
                {
                    fieldDependency.FieldInfo.SetValue(dependency, fieldDependency.DependencyInstance);
                }

                InitializeDependency iDependency = dependency as InitializeDependency;

                if (iDependency != null)
                {
                    iDependency.Initialize();
                }
            }
            else
            {
                provider.IsInitialized = true;
            }

		    return dependency;
		}

		private object[] GetDependencies(params Type[] types)
		{
			object[] dependencies = new object[types.Length];

			for(int i=0; i < types.Length; i++)
			{
				dependencies [i] = GetDependency (types [i]);
			}

			return dependencies;
		}

		private void RegisterJustInTimeDependencies(IEnumerable<Type> types)
		{
			foreach (Type type in types)
			{
				AttemptRegisterDependency (type);
			}
		}

		private bool AttemptRegisterDependency(Type type)
		{
			return AttemptRegisterSingleton (type) || AttemptRegisterProvider(type) || AttemptRegisterBinding(type);
		}

		private bool AttemptRegisterSingleton(Type type)
		{
			Singleton singleton = type.GetTypeAttribute<Singleton>();

			bool success = singleton != null;

			if (success)
			{
				RegisterProvider (type, new SingletonProvider (type));
			}

			return success;
		}
	
		private bool AttemptRegisterProvider(Type type)
		{
            ProvidedBy providedBy = type.GetTypeAttribute<ProvidedBy>();
			bool success = providedBy != null;

			if (success)
			{
				if (providedBy.ProviderType.IsSubclassOf(typeof (AbstractProvider)))
				{
					SingletonProvider singletonProvider = new SingletonProvider (providedBy.ProviderType);
				    ProviderProxy providerProxy = new ProviderProxy(singletonProvider, providedBy.ProviderType);
					RegisterProvider (providedBy.ProviderType, singletonProvider);
                    RegisterProvider(type, providerProxy);
				}
				else
				{
					throw new Exception (string.Format("Provided invalid ProviderType to ProvidedBy attribute on Type: {0}.  Must provide IProvider class!",
					                      type.FullName));
				}
			}

			return success;
		}

		private bool AttemptRegisterBinding(Type type)
		{
            ImplementedBy implementedBy = type.GetTypeAttribute<ImplementedBy>();
			bool success = implementedBy != null;

			if (success)
			{
				Type bindedType = implementedBy.ImplementedType;
                bool isSingleton = bindedType.GetTypeAttribute<Singleton>() != null;
				AbstractProvider provider;

                //TODO: Protect impl by to be used properly
/*
                if (!bindedType.isi(type))
                {
                    throw new Exception(string.Format("ImplementedBy attribute on interface {0} " +
                        "has invalid set implementation class: {1} which does not inherit {0}",
                        type.FullName,
                        bindedType.FullName));
                }
 */

				if (isSingleton) {
					provider = new SingletonProvider (bindedType);
				} else {
                    provider = new NoScopeProvider(bindedType);
				}
				
				RegisterProvider (type, provider);
			}

			return success;
		}

        private static void InitializeDependency(object dependency)
        {
            InitializeDependency initalizeDependency = dependency as InitializeDependency;

            if (initalizeDependency != null)
            {
                initalizeDependency.Initialize();
            }
        }
    }
}
