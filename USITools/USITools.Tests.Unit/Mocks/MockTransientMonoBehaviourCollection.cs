using Moq;
using System;
using System.Collections.Generic;

namespace USITools.Tests.Unit
{
    class MockTransientMonoBehaviourCollection
    {
        public IServiceCollection ServiceCollection { get; private set; }

        public MockTransientMonoBehaviourCollection()
        {
            var services = new Dictionary<Type, ServiceDefinition>
            {
                {
                    typeof(ITestInterface),
                    new ServiceDefinition(typeof(TestMonoBehaviourImplementingInterface), ServiceDefinitionLifetime.Transient, true)
                },
                {
                    typeof(TestMonoBehaviourWithoutInterface),
                    new ServiceDefinition(typeof(TestMonoBehaviourWithoutInterface), ServiceDefinitionLifetime.Transient, true)
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
    class BrokenMockTransientMonoBehaviourCollection
    {
        public IServiceCollection ServiceCollection { get; private set; }

        public BrokenMockTransientMonoBehaviourCollection()
        {
            var services = new Dictionary<Type, ServiceDefinition>
            {
                {
                    typeof(TestMonoBehaviourImplementingInterface),  // Should be registered by the interface, not the implementing class
                    new ServiceDefinition(typeof(TestMonoBehaviourImplementingInterface), ServiceDefinitionLifetime.Transient, true)
                }
            };

            var mock = new Mock<IServiceCollection>();
            mock.Setup(x => x.Services).Returns(services);

            ServiceCollection = mock.Object;
        }
    }
}
