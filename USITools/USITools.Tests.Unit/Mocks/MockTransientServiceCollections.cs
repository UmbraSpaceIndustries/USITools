using Moq;
using System;
using System.Collections.Generic;

namespace USITools.Tests.Unit
{
    class MockTransientServiceCollection
    {
        public IServiceCollection ServiceCollection { get; private set; }

        public MockTransientServiceCollection()
        {
            var services = new Dictionary<Type, ServiceDefinition>
            {
                {
                    typeof(ITestInterface),
                    new ServiceDefinition(typeof(TestServiceImplmentingInterface), ServiceDefinitionLifetime.Transient)
                },
                {
                    typeof(TestServiceWithoutInterface),
                    new ServiceDefinition(typeof(TestServiceWithoutInterface), ServiceDefinitionLifetime.Transient)
                },
                {
                    typeof(TestServiceWithInterfaceDependency),
                    new ServiceDefinition(typeof(TestServiceWithInterfaceDependency), ServiceDefinitionLifetime.Transient)
                },
                {
                    typeof(TestServiceWithClassDependency),
                    new ServiceDefinition(typeof(TestServiceWithClassDependency), ServiceDefinitionLifetime.Transient)
                },
                {
                    typeof(TestServiceWithInterfaceAndClassDependencies),
                    new ServiceDefinition(typeof(TestServiceWithInterfaceAndClassDependencies), ServiceDefinitionLifetime.Transient)
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
    class BrokenMockTransientServiceCollection
    {
        public IServiceCollection ServiceCollection { get; private set; }

        public BrokenMockTransientServiceCollection()
        {
            var services = new Dictionary<Type, ServiceDefinition>
            {
                {
                    typeof(TestServiceImplmentingInterface),  // Should be registered by the interface, not the implementing class
                    new ServiceDefinition(typeof(TestServiceImplmentingInterface), ServiceDefinitionLifetime.Transient)
                },
                // These services are missing their dependencies
                {
                    typeof(TestServiceWithInterfaceDependency),
                    new ServiceDefinition(typeof(TestServiceWithInterfaceDependency), ServiceDefinitionLifetime.Transient)
                }
            };

            var mock = new Mock<IServiceCollection>();
            mock.Setup(x => x.Services).Returns(services);

            ServiceCollection = mock.Object;
        }
    }
}
