using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Castle.DynamicProxy.Generators;
using Suice.DynamicProxy;
using Suice.Exception;

namespace Suice
{
    /// <summary>
    /// Suice Injection is Google's Guice port to C#.
    /// 
    /// Injector manages all dependencies within the Application.
    /// 
    /// Must call Initialize function after construction.
    /// 
    /// Currently supports Providers, Singletons, Field Injection, ImplementedBy and ProvidedBy functionalities.
    /// 
    /// Circular Dependency is NOW supported via Constructor Injection at this time.
    /// 
    /// Field injection support still exists, but is not testable in standard testing practices. (IE: Can't mock a field without reflection tools)
    /// 
    /// This tool recommends following practices of not having too many dependencies per instance and using constructor injection only.
    /// 
    /// Documentation may be found: https://github.com/Disturbing/suice
    /// 
    /// @author DisTurBinG
    /// </summary>
    public class Injector
    {
        private const BindingFlags DEPENDENCY_VARIABLE_FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;

        public event Action<object> OnInitializeDependency;
        
        private readonly Dictionary<Type, Provider> providersMap = new Dictionary<Type, Provider>();
        private readonly List<Type> circularDependencyLockedTypes = new List<Type>();
        private readonly Dictionary<Type, ProxyInterceptor> proxies = new Dictionary<Type, ProxyInterceptor>(); 
        private readonly ProxyFactory proxyFactory = new ProxyFactory();

        public void Initialize(params Assembly[] assemblies)
        {
            InjectDependencies(assemblies);
        }

        private void InjectDependencies(Assembly[] assemblies)
        {
            foreach(Assembly assembly in assemblies) {
                RegisterJustInTimeDependencies(assembly.GetTypes());
                InstantiateEagerSingletons();
            }
        }

        private void InstantiateEagerSingletons()
        {
            foreach (SingletonProvider provider in providersMap.Values
                .OfType<SingletonProvider>()
                .Where(s => s.Scope == Scope.EAGER_SINGLETON).ToArray()) {
                GetDependency(provider.ProvidedType);
            }
        }

        public void RegisterModule(AbstractModule module)
        {
            module.Configure();
            RegisterBindings(module.Bindings);
            CreateProvidersFromMethods(module);
        }

        private void CreateProvidersFromMethods(AbstractModule module)
        {
            foreach (MethodInfo methodInfo in module.GetType().GetMethodsWithAttribute<Provides>()) {
                Provides provides = (Provides) Attribute.GetCustomAttribute(methodInfo, typeof (Provides));
                Type[] dependencyTypes = methodInfo.GetParameters().Select(param => param.ParameterType).ToArray();

                if (provides.Scope == Scope.NO_SCOPE) {
                    RegisterProvider(methodInfo.ReturnType, new MethodProvider(module, methodInfo, dependencyTypes));
                } else {
                    RegisterProvider(methodInfo.ReturnType,
                        new SingletonMethodProvider(
                            provides.Scope,
                            module,
                            methodInfo,
                            dependencyTypes));
                }
            }
        }

        private void RegisterProvider(Type bindedType, Provider provider)
        {
            try {
                providersMap.Add(bindedType, provider);
            } catch (ArgumentException e) {
                throw new DuplicateBindingException(bindedType.FullName, provider.ProvidedType.FullName,
                    providersMap[bindedType].ImplementedType.FullName);
            }
        }

        private void RegisterBindings(IEnumerable<IBinding> bindings)
        {
            foreach (IBinding binding in bindings) {
                CreateProvider(binding);
            }
        }

        private FieldDependency[] GetFieldDependencies(Type type)
        {
            FieldInfo[] fieldInfos = type.GetFields(DEPENDENCY_VARIABLE_FLAGS)
                                         .Where(f => f.GetCustomAttributes(typeof (Inject), true).Length > 0).ToArray();
            FieldDependency[] fieldDependencies = new FieldDependency[fieldInfos.Length];

            for (int i = 0; i < fieldInfos.Length; i++) {
                Type fieldType = fieldInfos[i].FieldType;

                if (fieldType == type) {
                    throw new InjectToSelfException(type.FullName);
                }

                fieldDependencies[i] = new FieldDependency(fieldInfos[i], GetDependency(fieldType));
            }

            return fieldDependencies;
        }

        private void CreateProvider(IBinding binding)
        {
            if (binding.Scope == Scope.NO_SCOPE) {
                RegisterProvider(binding.TypeToBind,
                    new NoScopeProvider(binding.TypeToBind, binding.BindedType, GetDependencyTypes(binding.BindedType)));
            } else {
                RegisterProvider(binding.TypeToBind,
                    new SingletonProvider(
                        binding.Scope,
                        binding.TypeToBind,
                        binding.BindedType,
                        GetDependencyTypes(binding.BindedType),
                        binding.BindedInstance));
            }
        }

        private Type[] GetDependencyTypes(Type type)
        {
            return GetConstructor(type).GetParameters().Select(param => param.ParameterType).ToArray();
        }

        private ConstructorInfo GetConstructor(Type bindedType)
        {
            ConstructorInfo[] constructorInfos = bindedType.GetConstructors();
            ConstructorInfo constructorInfo = null;

            if (constructorInfos.Length == 0)
            {
                constructorInfo = bindedType.GetConstructor(Type.EmptyTypes);
            }
            else if (constructorInfos.Length == 1 && IsValidConstructor(constructorInfos[0]))
            {
                constructorInfo = constructorInfos[0];
            }

            if (constructorInfo == null)
            {
                throw new InvalidDependencyConstructorException(bindedType.FullName);
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
            Provider provider;

            if (providersMap.TryGetValue(type, out provider) == false) {
                if (type.IsAssignableToGenericType(typeof(IProvider<>))) {
                    CreateDynamicProvider(type, out provider);
                } else {
                    throw new InvalidDependencyException(type.FullName);
                }
            }

            return circularDependencyLockedTypes.Contains(type) 
                ? CreateProxy(type)
                : CreateDependency(type, provider);
        }

        private void CreateDynamicProvider(Type type, out Provider provider)
        {
            Type providedType = type.GetGenericArguments()[0];
            ImplementedBy implementedBy = providedType.GetTypeAttribute<ImplementedBy>();

            Type implementedType = (implementedBy == null)
                ? providedType
                : implementedBy.ImplementedType;

            Type dynamicProviderType = typeof(DynamicProvider<>).MakeGenericType(providedType);
            providersMap.Add(type, provider = (Provider) dynamicProviderType.New(
                providedType,
                implementedType,
                GetDependencyTypes(implementedType)
                ));
        }

        private object CreateDependency(Type type, Provider provider)
        {
            if (!provider.IsInitialized) {
                PrepareInstantation(type, provider);
            }

            object dependency = provider.Provide();

            if (!provider.IsInitialized || provider is NoScopeProvider) {
                InitializeAfterInstantiation(provider, dependency);
            }
            return dependency;
        }

        private object CreateProxy(Type type)
        {
            ProxyInterceptor proxyInterceptor;
            object dependency = proxyFactory.CreateTransparentInterfaceProxy(type, out proxyInterceptor);
            proxies.Add(type, proxyInterceptor);

            return dependency;
        }

        private void InitializeAfterInstantiation(Provider provider, object dependency)
        {
            provider.IsInitialized = true;

            InitializeDependencyFields(dependency);

            ProxyInterceptor changeProxyInterceptorTarget;

            if (proxies.TryGetValue(provider.ProvidedType, out changeProxyInterceptorTarget)) {
                changeProxyInterceptorTarget.Initialize(dependency);
            }

            BroadcastDependencyInitialization(dependency);


        }

        private void PrepareInstantation(Type type, Provider provider)
        {
            circularDependencyLockedTypes.Add(type);

            InitializeDependencies(provider);

            circularDependencyLockedTypes.Remove(type);
        }

        private void InitializeDependencies(Provider provider)
        {
            for (int i=0; i < provider.Dependencies.Length; i++) {
                provider.Dependencies[i] = GetDependency(provider.DependencyTypes[i]);
            }
        }

        private void InitializeDependencyFields(object dependency)
        {
            FieldDependency[] fieldDependencies = GetFieldDependencies(dependency.GetType());

            foreach (FieldDependency fieldDependency in fieldDependencies) {
                fieldDependency.FieldInfo.SetValue(dependency, fieldDependency.DependencyInstance);
            }
        }

        private void BroadcastDependencyInitialization(object dependency)
        {
            IInitializable iInitializable = dependency as IInitializable;

            if (iInitializable != null) {
                iInitializable.Initialize();
            }

            if (OnInitializeDependency != null) {
                OnInitializeDependency(dependency);
            }
        }

        private void RegisterJustInTimeDependencies(IEnumerable<Type> types) {
            foreach (Type type in types) {
                AttemptRegisterDependency(type);
            }
        }

        private bool AttemptRegisterDependency(Type type)
        {
            return AttemptRegisterBinding(type) || AttemptRegisterSingleton(type);
        }

        private bool AttemptRegisterSingleton(Type type)
        {
            bool foundImplementedByInterface = type.GetInterfaces().Count(iType => iType.GetTypeAttribute<ImplementedBy>() != null) > 0;
            Singleton singletonAttribute = type.GetTypeAttribute<Singleton>();

            bool success = !foundImplementedByInterface && singletonAttribute != null;

            if (success) {
                RegisterProvider(type, new SingletonProvider(singletonAttribute.Scope, type, GetDependencyTypes(type)));
            }

            return success;
        }

        private bool AttemptRegisterBinding(Type type)
        {
            ImplementedBy implementedBy = type.GetTypeAttribute<ImplementedBy>();
            bool success = implementedBy != null;

            if (success) {
                Type bindedType = implementedBy.ImplementedType;
                Singleton singletonAttribute = bindedType.GetTypeAttribute<Singleton>();
                Provider provider;

                if (!bindedType.GetInterfaces().Contains(type)) {
                    throw new InvalidImplementedByException(type.FullName, bindedType.FullName);
                }

                if (singletonAttribute != null) {
                    provider = new SingletonProvider(singletonAttribute.Scope, type, bindedType, GetDependencyTypes(bindedType));
                } else {
                    provider = new NoScopeProvider(type, bindedType, GetDependencyTypes(bindedType));
                }

                RegisterProvider(type, provider);
            }

            return success;
        }
    }
}