﻿using System.Collections.Generic;
using System.Reflection;
using Suice;
using Xunit;

namespace suice_tests
{
    
    public class DynamicProviderTest
    {
        private Injector injector;
        private Module module;

        public DynamicProviderTest()
        {
            injector = new Injector();
            injector.RegisterModule(module = new Module());
            injector.Initialize(Assembly.GetExecutingAssembly());
        }

        [Fact]
        public void TestDynamicProvider()
        {
            module.Container.Create10TestObjects();
            
            Assert.True(module.Container.testObjects.Count == 10);
            Assert.True(module.Container.testObjects.TrueForAll(t => t.DepsExist()));
        }

        private class Module : AbstractModule
        {
            public TestContainer Container;

            [Provides(Scope.EAGER_SINGLETON)]
            public TestContainer ProvidesContainer(IProvider<TestObject> testObjectProvider)
            {
                return Container = new TestContainer(testObjectProvider);
            }
        }

        [Singleton]
        public class DepA {}
        [Singleton]
        public class DepB {}

        public class TestObject
        {
            private readonly DepA depA;
            private readonly DepB depB;

            [Inject]
            public TestObject(DepA depA, DepB depB)
            {
                this.depA = depA;
                this.depB = depB;
            }

            public bool DepsExist()
            {
                return depA != null && depB != null;
            }
        }

        public class TestContainer
        {
            private readonly IProvider<TestObject> testObjectProvider;

            public readonly List<TestObject> testObjects = new List<TestObject>();
            
            [Inject]
            public TestContainer(IProvider<TestObject> testObjectProvider)
            {
                this.testObjectProvider = testObjectProvider;
            }

            public void Create10TestObjects()
            {
                for (int i = 0; i < 10; i++) {
                    testObjects.Add(testObjectProvider.Provide());
                }
            }
        }
    }
}
