using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace CmnTools.Suice {
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
    public class Injector {
        private const BindingFlags DEPENDENCY_VARIABLE_FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly Dictionary<Type, AbstractProvider> providersMap = new Dictionary<Type, AbstractProvider>();

        private readonly Dictionary<Type, IBinding> bindingMap = new Dictionary<Type, IBinding>();

        public Action<object> OnInitializeDependency;

        private HashSet<Type> CircularDependencyLockedTypes = new HashSet<Type>();

        public void Init() {
            InjectDependencies();
        }

        private void InjectDependencies() {
            IEnumerable<Type> assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();

            RegisterJustInTimeDependencies(assemblyTypes);

            foreach (KeyValuePair<Type, AbstractProvider> kvp in providersMap
                .Where(x => !(x.Value is SingletonMethodProvider) && x.Value is SingletonProvider)) {
                GetDependency(kvp.Key);
            }
        }

        public void RegisterModule(AbstractModule module) {
            module.Configure();
            RegisterBindings(module.Bindings);
            CreateProvidersFromMethods(module);
        }

        private void CreateProvidersFromBindings(AbstractModule module) {
            foreach (IBinding binding in module.Bindings) {
                CreateProvider(binding);
            }
        }

        private void CreateProvidersFromMethods(AbstractModule module) {
            foreach (MethodInfo methodInfo in module.GetType().GetMethodsWithAttribute<Provides>()) {
                Provides provides = (Provides) Attribute.GetCustomAttribute(methodInfo, typeof (Provides));

                if (provides.Scope == Scope.NO_SCOPE) {
                    RegisterProvider(methodInfo.ReturnType, new MethodProvider(module, methodInfo));
                } else if (provides.Scope == Scope.SINGLETON) {
                    RegisterProvider(methodInfo.ReturnType, new SingletonMethodProvider(module, methodInfo));
                } else {
                    throw new Exception(string.Format(
                        "AbstractModule {0}#{1} has unhandled scope type {2}",
                        module.GetType().FullName,
                        methodInfo.Name,
                        provides.Scope));
                }
            }
        }

        private void RegisterProvider(Type bindedType, AbstractProvider provider) {
            try {
                providersMap.Add(bindedType, provider);
            } catch (ArgumentException e) {
                throw new Exception(
                    string.Format("Attempted to bind {0} to {1} twice! May only specify one implementation type!",
                                  bindedType,
                                  provider.ProvidedType));
            }
        }

        private void RegisterBindings(IEnumerable<IBinding> bindings) {
            foreach (IBinding binding in bindings) {
                CreateProvider(binding);
            }
        }

        private object[] GetMethodDependencies(Type dependencyType, MethodBase methodInfo) {
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            object[] parameters = new object[parameterInfos.Length];

            for (int i = 0; i < parameterInfos.Length; i++) {
                Type parameterType = parameterInfos[i].ParameterType;

                if (dependencyType == parameterType) {
                    throw new Exception("Dependency " + dependencyType.FullName +
                                        " is attempting to inject itself through method/constructor injection!");
                }

                parameters[i] = GetDependency(parameterType);
            }

            return parameters;
        }

        private FieldDependency[] GetFieldDependencies(Type type) {
            FieldInfo[] fieldInfos = type.GetFields(DEPENDENCY_VARIABLE_FLAGS)
                                         .Where(f => f.GetCustomAttributes(typeof (Inject), true).Length > 0).ToArray();
            FieldDependency[] fieldDependencies = new FieldDependency[fieldInfos.Length];

            for (int i = 0; i < fieldInfos.Length; i++) {
                Type fieldType = fieldInfos[i].FieldType;

                if (fieldType == type) {
                    throw new Exception("Dependency " + type.FullName +
                                        " is attempting to inject itself through field injection!");
                }

                fieldDependencies[i] = new FieldDependency(fieldInfos[i], GetDependency(fieldType));
            }

            return fieldDependencies;
        }

        private void CreateProvider(IBinding binding) {
            if (binding.Scope == Scope.NO_SCOPE) {
                RegisterProvider(binding.TypeToBind,
                                 new NoScopeProvider(binding.TypeToBind, binding.BindedType));
            } else if (binding.Scope == Scope.SINGLETON) {
                SingletonProvider singletonProvider = new SingletonProvider(binding.TypeToBind, binding.BindedType);

                if (binding.BindedInstance != null) {
                    singletonProvider.SetInstance(binding.BindedInstance);
                    singletonProvider.IsInitialized = true;
                }

                RegisterProvider(binding.TypeToBind, singletonProvider);
            } else {
                throw new Exception(string.Format(
                    "Binding {0} has unhandled scope type {1}",
                    binding.TypeToBind.FullName,
                    binding.Scope));
            }
        }

        private ConstructorInfo GetConstructor(Type bindedType) {
            ConstructorInfo[] constructorInfos = bindedType.GetConstructors();
            ConstructorInfo constructorInfo = null;

            if (constructorInfos.Length == 0) {
                constructorInfo = bindedType.GetConstructor(Type.EmptyTypes);
            } else if (constructorInfos.Length == 1 && IsValidConstructor(constructorInfos[0])) {
                constructorInfo = constructorInfos[0];
            }

            if (constructorInfo == null) {
                throw new Exception(string.Format("Could not find valid constructor for type {0}." +
                                                  "Must provide no constructor, an empty constructor, or a constructor with an [Inject] attribute!",
                                                  bindedType.FullName));
            }

            return constructorInfo;
        }

        private bool IsValidConstructor(ConstructorInfo constructorInfo) {
            return constructorInfo.GetParameters().Length == 0 ||
                   constructorInfo.GetMemberInfoAttribute<Inject>() != null;
        }

        private object GetDependency(Type type) {
            AbstractProvider provider;

            if (!providersMap.TryGetValue(type, out provider)) {
                throw new Exception("Attempted to get dependency: " + type.FullName + " that doesn't exist!");
            }

            if (CircularDependencyLockedTypes.Contains(type)) {
                throw new Exception("Detected Ciruclar Constructor Dependency for type: " + type +
                                    ". Check all constructor dependencies !");
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

        private void InitializeAfterInstantiation(AbstractProvider provider, object dependency) {
            InitializeDependencyFields(provider, dependency);

            BroadcastDependencyInitialization(dependency);
        }

        private void PrepareInstantation(Type type, AbstractProvider provider) {
            CircularDependencyLockedTypes.Add(type);

            GenerateConstructorDependencyMap(type, provider);

            SingletonProvider singletonProvider = provider as SingletonProvider;
            if (singletonProvider != null) {
                singletonProvider.CreateSingletonInstance();
            }

            CircularDependencyLockedTypes.Remove(type);
        }

        private void GenerateConstructorDependencyMap(Type type, AbstractProvider provider) {
            IMethodConstructor methodConstructor = provider as IMethodConstructor;
            ProviderProxy providerProxy = provider as ProviderProxy;

            if (providerProxy != null) {
                providerProxy.SetProviderInstance(
                    (AbstractProvider) GetDependency(providerProxy.ProviderType));
            } else if (methodConstructor != null) {
                provider.SetConstructorDependencies(GetMethodDependencies(type,
                                                                          methodConstructor.GetMethodConstructor()));
            } else {
                provider.SetConstructorDependencies(GetMethodDependencies(provider.ProvidedType,
                                                                          GetConstructor(provider.ImplementedType)));
            }
        }

        private void InitializeDependencyFields(AbstractProvider provider, object dependency) {
            provider.IsInitialized = true;

            FieldDependency[] fieldDependencies = GetFieldDependencies(dependency.GetType());

            foreach (FieldDependency fieldDependency in fieldDependencies) {
                fieldDependency.FieldInfo.SetValue(dependency, fieldDependency.DependencyInstance);
            }
        }

        private void BroadcastDependencyInitialization(object dependency) {
            InitializeDependency iDependency = dependency as InitializeDependency;

            if (iDependency != null) {
                iDependency.Initialize();
            }

            if (OnInitializeDependency != null) {
                OnInitializeDependency(dependency);
            }
        }

        private object[] GetDependencies(params Type[] types) {
            object[] dependencies = new object[types.Length];

            for (int i = 0; i < types.Length; i++) {
                dependencies[i] = GetDependency(types[i]);
            }

            return dependencies;
        }

        private void RegisterJustInTimeDependencies(IEnumerable<Type> types) {
            foreach (Type type in types) {
                AttemptRegisterDependency(type);
            }
        }

        private bool AttemptRegisterDependency(Type type) {
            return AttemptRegisterBinding(type) || AttemptRegisterSingleton(type) || AttemptRegisterProvider(type);
        }

        private bool AttemptRegisterSingleton(Type type) {
            bool foundImplementedByInterface = type.GetInterfaces().Count(
                iType => iType.GetTypeAttribute<ImplementedBy>() != null) > 0;

            Singleton singleton = type.GetTypeAttribute<Singleton>();

            bool success = !foundImplementedByInterface && singleton != null;

            if (success) {
                RegisterProvider(type, new SingletonProvider(type));
            }

            return success;
        }

        private bool AttemptRegisterProvider(Type type) {
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
                    throw new Exception(
                        string.Format(
                            "Provided invalid ProviderType to ProvidedBy attribute on Type: {0}.  Must provide IProvider class!",
                            type.FullName));
                }
            }

            return success;
        }

        private bool AttemptRegisterBinding(Type type) {
            ImplementedBy implementedBy = type.GetTypeAttribute<ImplementedBy>();
            bool success = implementedBy != null;

            if (success) {
                Type bindedType = implementedBy.ImplementedType;
                bool isSingleton = bindedType.GetTypeAttribute<Singleton>() != null;
                AbstractProvider provider;

                if (!bindedType.GetInterfaces().Contains(type))
                {
                    throw new Exception(string.Format("ImplementedBy attribute on interface {0} " +
                        "has invalid set implementation class: {1} which does not inherit {0}",
                        type.FullName,
                        bindedType.FullName));
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

        private bool IsProvidedByProvider(Type type) {
            bool isProvidedByProvider = false;

            if (type.IsGenericType) {
                isProvidedByProvider = type.GetGenericArguments().Count(t => t.GetTypeAttribute<ProvidedBy>() != null) >
                                       0;
            }

            return isProvidedByProvider;
        }

        private static void InitializeDependency(object dependency) {
            InitializeDependency initalizeDependency = dependency as InitializeDependency;

            if (initalizeDependency != null) {
                initalizeDependency.Initialize();
            }
        }
    }
}