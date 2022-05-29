using Moq;
using System;
using System.Collections.Generic;

namespace USITools.Tests.Unit
{
    class MockSingletonServiceCollection
    {
        public IServiceCollection ServiceCollection { get; private set; }

        public MockSingletonServiceCollection()
        {
            var services = new Dictionary<Type, ServiceDefinition>
            {
                {
                    typeof(ITestInterface),
                    new ServiceDefinition(typeof(TestServiceImplmentingInterface), ServiceDefinitionLifetime.Singleton)
                },
                {
                    typeof(TestServiceWithoutInterface),
                    new ServiceDefinition(typeof(TestServiceWithoutInterface), ServiceDefinitionLifetime.Singleton)
                },
                {
                    typeof(TestServiceWithInterfaceDependency),
                    new ServiceDefinition(typeof(TestServiceWithInterfaceDependency), ServiceDefinitionLifetime.Singleton)
                },
                {
                    typeof(TestServiceWithClassDependency),
                    new ServiceDefinition(typeof(TestServiceWithClassDependency), ServiceDefinitionLifetime.Singleton)
                },
                {
                    typeof(TestServiceWithInterfaceAndClassDependencies),
                    new ServiceDefinition(typeof(TestServiceWithInterfaceAndClassDependencies), ServiceDefinitionLifetime.Singleton)
                }
            };

            var mock = new Mock<IServiceCollection>();
            mock.Setup(x => x.Services).Returns(services);

            ServiceCollection = mock.Object;
        }
    }

    /// <summary>
    /// Intentionally setup incorrectly to test misconfiguration error handling.
    /// </summary>
    class BrokenMockSingletonServiceCollection
    {
        public IServiceCollection ServiceCollection { get; private set; }

        public BrokenMockSingletonServiceCollection()
        {
            var services = new Dictionary<Type, ServiceDefinition>
            {
                {
                    typeof(TestServiceImplmentingInterface),  // Should be registered by the interface, not the implementing class
                    new ServiceDefinition(typeof(TestServiceImplmentingInterface), ServiceDefinitionLifetime.Singleton)
                },
                // These services are missing their dependencies
                {
                    typeof(TestServiceWithInterfaceDependency),
                    new ServiceDefinition(typeof(TestServiceWithInterfaceDependency), ServiceDefinitionLifetime.Singleton)
                }
            };

            var mock = new Mock<IServiceCollection>();
            mock.Setup(x => x.Services).Returns(services);

            ServiceCollection = mock.Object;
        }
    }
}
