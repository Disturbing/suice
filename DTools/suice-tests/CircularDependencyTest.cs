using System.Reflection;
using DTools.Suice;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace suice_tests
{
    [TestClass]
    public class CircularDependencyTest
    {
        private Injector injector;
        private Module module;

        [TestInitialize]
        public void Init()
        {
            injector = new Injector();
            injector.RegisterModule(module = new Module());
            injector.Initialize(Assembly.GetExecutingAssembly());
        }

        [TestMethod]
        public void TestCircularDependency()
        {
            Assert.IsTrue(module.Container.BothCallsWereSuccessful());
        }

        private class Module : AbstractModule
        {
            public Container Container;

            [Provides(Scope.EAGER_SINGLETON)]
            public Container ProvidesContainer(DependencyA depA, DependencyB depB)
            {
                return Container = new Container(depA, depB);
            }
        }

        public class Container
        {
            private DependencyA depA;
            private DependencyB depB;
            
            public Container(DependencyA depA, DependencyB depB)
            {
                this.depA = depA;
                this.depB = depB;
            }

            public bool BothCallsWereSuccessful()
            {
                depA.Test();
                depB.Test();
                return depA.ProxyCallSuccessful && depB.ProxyCallSuccessful;
            }
        }

        [ImplementedBy(typeof(DepedencyAImpl))]
        public interface DependencyA
        {
            bool ProxyCallSuccessful { get; set; }
            void Test();
        }

        [ImplementedBy(typeof(DependencyBImpl))]
        public interface DependencyB
        {
            bool ProxyCallSuccessful { get; set; }
            void Test();
        }

        [Singleton]
        public class DepedencyAImpl : DependencyA
        {
            public bool ProxyCallSuccessful { get; set; }
            private DependencyB depB;

            [Inject]
            public DepedencyAImpl(DependencyB depB)
            {
                this.depB = depB;
            }

            public void Test()
            {
                this.depB.ProxyCallSuccessful = true;
            }
        }

        [Singleton]
        public class DependencyBImpl : DependencyB
        {
            public bool ProxyCallSuccessful { get; set; }
            private DependencyA depA;

            [Inject]
            public DependencyBImpl(DependencyA depA)
            {
                this.depA = depA;
            }

            public void Test()
            {
                this.depA.ProxyCallSuccessful = true;
            }
        }
    }
}
