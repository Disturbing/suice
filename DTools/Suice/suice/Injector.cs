using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using DTools.Suice.Exception;

namespace DTools.Suice
{
    /// <summary>
    /// Suice Injection is Google's Guice port to C#.
    /// 
    /// Injector manages all dependencies within the Application.
    /// Must call Initialize function after construction.
    /// 
    /// Currently supports Providers, Singletons, Field Injection, ImplementedBy and ProvidedBy functionalities.
    /// 
    /// Circular Dependency is not supported via Constructor Injection at this time.  Must use field injection for circular dependencies.
    /// 
    /// Documentation may be found: https://github.com/Disturbing/suice
    /// 
    /// @author DisTurBinG
    /// </summary>
    public class Injector
    {
        private const BindingFlags DEPENDENCY_VARIABLE_FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly Dictionary<Type, AbstractProvider> providersMap = new Dictionary<Type, AbstractProvider>();

        public event Action<object> OnInitializeDependency;

        private List<Type> circularDependencyLockedTypes = new List<Type>();

        public void Initialize(params Assembly[] assemblies)
        {
            InjectDependencies(assemblies);
        }

        private void InjectDependencies(Assembly[] assemblies)
        {
            foreach(Assembly assembly in assemblies) {
                IEnumerable<Type> assemblyTypes = assembly.GetTypes();

                RegisterJustInTimeDependencies(assemblyTypes);

                foreach (KeyValuePair<Type, AbstractProvider> kvp in providersMap
                    .Where(x => !(x.Value is SingletonMethodProvider) && x.Value is SingletonProvider)) {
                        GetDependency(kvp.Key);
                }
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

                if (provides.Scope == Scope.NO_SCOPE) {
                    RegisterProvider(methodInfo.ReturnType, new MethodProvider(module, methodInfo));
                } else if (provides.Scope == Scope.SINGLETON) {
                    RegisterProvider(methodInfo.ReturnType, new SingletonMethodProvider(module, methodInfo));
                }
            }
        }

        private void RegisterProvider(Type bindedType, AbstractProvider provider)
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

        private object[] GetMethodDependencies(Type dependencyType, MethodBase methodInfo)
        {
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            object[] parameters = new object[parameterInfos.Length];

            for (int i = 0; i < parameterInfos.Length; i++) {
                Type parameterType = parameterInfos[i].ParameterType;

                if (dependencyType == parameterType) {
                    throw new InjectToSelfException(dependencyType.FullName);
                }

                parameters[i] = GetDependency(parameterType);
            }

            return parameters;
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
                                 new NoScopeProvider(binding.TypeToBind, binding.BindedType));
            } else if (binding.Scope == Scope.SINGLETON) {
                SingletonProvider singletonProvider = new SingletonProvider(binding.TypeToBind, binding.BindedType);

                if (binding.BindedInstance != null) {
                    singletonProvider.SetInstance(binding.BindedInstance);
                }

                RegisterProvider(binding.TypeToBind, singletonProvider);
            }
        }

        private ConstructorInfo GetConstructor(Type bindedType)
        {
            ConstructorInfo[] constructorInfos = bindedType.GetConstructors();
            ConstructorInfo constructorInfo = null;

            if (constructorInfos.Length == 0) {
                constructorInfo = bindedType.GetConstructor(Type.EmptyTypes);
            } else if (constructorInfos.Length == 1 && IsValidConstructor(constructorInfos[0])) {
                constructorInfo = constructorInfos[0];
            }

            if (constructorInfo == null) {
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
            AbstractProvider provider;

            if (!providersMap.TryGetValue(type, out provider)) {
                throw new InvalidDependencyException(type.FullName);
            }

            if (circularDependencyLockedTypes.Contains(type)) {
                throw new CircularDependencyException(GenerateCircularDependencyMapStr(type));
            }

            if (!provider.IsInitialized) {
                PrepareInstantation(type, provider);
            }

            object dependency = provider.Provide();

            if (!provider.IsInitialized || provider is NoScopeProvider) {
                InitializeAfterInstantiation(provider, dependency);
            }

            return dependency;
        }

        private string GenerateCircularDependencyMapStr(Type type)
        {
            string circularDependencyMapStr = string.Empty;

            for (int i = 0; i < circularDependencyLockedTypes.Count; i++) {
                circularDependencyMapStr += circularDependencyLockedTypes[i] + "->";
            }

            circularDependencyMapStr += type.FullName;
            return circularDependencyMapStr;
        }

        private void InitializeAfterInstantiation(AbstractProvider provider, object dependency)
        {
            provider.IsInitialized = true;

            InitializeDependencyFields(provider, dependency);

            BroadcastDependencyInitialization(dependency);
        }

        private void PrepareInstantation(Type type, AbstractProvider provider)
        {
            circularDependencyLockedTypes.Add(type);

            InitializeDependencies(type, provider);

            SingletonProvider singletonProvider = provider as SingletonProvider;

            if (singletonProvider != null) {
                singletonProvider.CreateSingletonInstance();
            }

            circularDependencyLockedTypes.Remove(type);
        }

        private void InitializeDependencies(Type type, AbstractProvider provider)
        {
            IMethodConstructor methodConstructor = provider as IMethodConstructor;
            ProviderProxy providerProxy = provider as ProviderProxy;

            if (providerProxy != null) {
                providerProxy.SetProviderInstance((AbstractProvider) GetDependency(providerProxy.ProviderType));
            } else if (methodConstructor != null) {
                provider.SetDependencies(GetMethodDependencies(type, methodConstructor.GetMethodConstructor()));
            } else {
                provider.SetDependencies(GetMethodDependencies(provider.ProvidedType, GetConstructor(provider.ImplementedType)));
            }
        }

        private void InitializeDependencyFields(AbstractProvider provider, object dependency)
        {
            FieldDependency[] fieldDependencies = GetFieldDependencies(dependency.GetType());

            foreach (FieldDependency fieldDependency in fieldDependencies) {
                fieldDependency.FieldInfo.SetValue(dependency, fieldDependency.DependencyInstance);
            }
        }

        private void BroadcastDependencyInitialization(object dependency)
        {
            IAutoInitialize iAutoInitialize = dependency as IAutoInitialize;

            if (iAutoInitialize != null) {
                iAutoInitialize.Initialize();
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
            return AttemptRegisterBinding(type) || AttemptRegisterSingleton(type) || AttemptRegisterProvider(type);
        }

        private bool AttemptRegisterSingleton(Type type)
        {
            bool foundImplementedByInterface = type.GetInterfaces().Count(
                iType => iType.GetTypeAttribute<ImplementedBy>() != null) > 0;

            Singleton singleton = type.GetTypeAttribute<Singleton>();

            bool success = !foundImplementedByInterface && singleton != null;

            if (success) {
                RegisterProvider(type, new SingletonProvider(type));
            }

            return success;
        }

        private bool AttemptRegisterProvider(Type type)
        {
            ProvidedBy providedBy = type.GetTypeAttribute<ProvidedBy>();
            bool success = providedBy != null;

            if (success) {
                ImplementedBy implementedBy = providedBy.ProviderType.GetTypeAttribute<ImplementedBy>();

                Type implementedProviderType = (implementedBy == null)
                                                   ? providedBy.ProviderType
                                                   : implementedBy.ImplementedType;

                if (implementedProviderType.IsSubclassOf(typeof (AbstractProvider))) {
                    if (implementedProviderType.GetTypeAttribute<ImplementedBy>(true) == null &&
                        implementedProviderType.GetTypeAttribute<Singleton>(true) == null) {
                        SingletonProvider singletonProvider = new SingletonProvider(providedBy.ProviderType,
                                                                                    implementedProviderType);
                        RegisterProvider(providedBy.ProviderType, singletonProvider);
                    }

                    ProviderProxy providerProxy = new ProviderProxy(type, providedBy.ProviderType);

                    RegisterProvider(type, providerProxy);
                } else {
                    throw new InvalidProvidedByException(type.FullName);
                }
            }

            return success;
        }

        private bool AttemptRegisterBinding(Type type)
        {
            ImplementedBy implementedBy = type.GetTypeAttribute<ImplementedBy>();
            bool success = implementedBy != null;

            if (success) {
                Type bindedType = implementedBy.ImplementedType;
                bool isSingleton = bindedType.GetTypeAttribute<Singleton>() != null;
                AbstractProvider provider;

                if (!bindedType.GetInterfaces().Contains(type)) {
                    throw new InvalidImplementedByException(type.FullName, bindedType.FullName);
                }

                if (isSingleton) {
                    provider = new SingletonProvider(type, bindedType);
                } else {
                    provider = new NoScopeProvider(type, bindedType);
                }

                RegisterProvider(type, provider);
            }

            return success;
        }
    }
}