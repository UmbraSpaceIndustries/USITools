using Xunit;

namespace USITools.Tests.Unit
{
    public class ServiceCollection_Tests
    {
        [Fact]
        public void Can_register_transient_service_by_class_type()
        {
            // Assign
            var collection = new ServiceCollection();
            var expectedLifetime = ServiceDefinitionLifetime.Transient;

            // Act
            collection.AddService<TestServiceWithoutInterface>();

            // Assert
            Assert.Contains(typeof(TestServiceWithoutInterface), collection.Services.Keys);
            var service = collection.Services[typeof(TestServiceWithoutInterface)];
            Assert.Equal(expectedLifetime, service.Lifetime);
            Assert.Equal(typeof(TestServiceWithoutInterface), service.Type);
            Assert.Empty(service.ConstructorParams);
        }

        [Fact]
        public void Can_register_transient_MonoBehaviour_by_class_type()
        {
            // Assign
            var collection = new ServiceCollection();
            var expectedLifetime = ServiceDefinitionLifetime.Transient;

            // Act
            collection.AddMonoBehaviour<TestMonoBehaviourWithoutInterface>();

            // Assert
            Assert.Contains(typeof(TestMonoBehaviourWithoutInterface), collection.Services.Keys);
            var service = collection.Services[typeof(TestMonoBehaviourWithoutInterface)];
            Assert.Equal(typeof(TestMonoBehaviourWithoutInterface), service.Type);
            Assert.Equal(expectedLifetime, service.Lifetime);
            Assert.Empty(service.ConstructorParams);
        }

        [Fact]
        public void Can_register_transient_service_by_interface_type()
        {
            // Assign
            var collection = new ServiceCollection();
            var expectedLifetime = ServiceDefinitionLifetime.Transient;

            // Act
            collection.AddService<ITestInterface, TestServiceImplmentingInterface>();

            // Assert
            Assert.Contains(typeof(ITestInterface), collection.Services.Keys);
            var service = collection.Services[typeof(ITestInterface)];
            Assert.Equal(expectedLifetime, service.Lifetime);
            Assert.Equal(typeof(TestServiceImplmentingInterface), service.Type);
            Assert.Empty(service.ConstructorParams);
        }

        [Fact]
        public void Can_register_transient_MonoBehaviour_by_interface_type()
        {
            // Assign
            var collection = new ServiceCollection();
            var expectedLifetime = ServiceDefinitionLifetime.Transient;

            // Act
            collection.AddMonoBehaviour<ITestInterface, TestMonoBehaviourImplementingInterface>();

            // Assert
            Assert.Contains(typeof(ITestInterface), collection.Services.Keys);
            var service = collection.Services[typeof(ITestInterface)];
            Assert.Equal(expectedLifetime, service.Lifetime);
            Assert.Equal(typeof(TestMonoBehaviourImplementingInterface), service.Type);
            Assert.Empty(service.ConstructorParams);
        }

        [Fact]
        public void Can_register_singleton_service_by_class_type()
        {
            // Assign
            var collection = new ServiceCollection();
            var expectedLifetime = ServiceDefinitionLifetime.Singleton;

            // Act
            collection.AddSingletonService<TestServiceWithoutInterface>();

            // Assert
            Assert.Contains(typeof(TestServiceWithoutInterface), collection.Services.Keys);
            var service = collection.Services[typeof(TestServiceWithoutInterface)];
            Assert.Equal(expectedLifetime, service.Lifetime);
            Assert.Equal(typeof(TestServiceWithoutInterface), service.Type);
            Assert.Empty(service.ConstructorParams);
        }

        [Fact]
        public void Can_register_singleton_MonoBehaviour_by_class_type()
        {
            // Assign
            var collection = new ServiceCollection();
            var expectedLifetime = ServiceDefinitionLifetime.Singleton;

            // Act
            collection.AddSingletonMonoBehaviour<TestMonoBehaviourWithoutInterface>();

            // Assert
            Assert.Contains(typeof(TestMonoBehaviourWithoutInterface), collection.Services.Keys);
            var service = collection.Services[typeof(TestMonoBehaviourWithoutInterface)];
            Assert.Equal(expectedLifetime, service.Lifetime);
            Assert.Equal(typeof(TestMonoBehaviourWithoutInterface), service.Type);
            Assert.Empty(service.ConstructorParams);
        }

        [Fact]
        public void Can_register_singleton_service_by_interface_type()
        {
            // Assign
            var collection = new ServiceCollection();
            var expectedLifetime = ServiceDefinitionLifetime.Singleton;

            // Act
            collection.AddSingletonService<ITestInterface, TestServiceImplmentingInterface>();

            // Assert
            Assert.Contains(typeof(ITestInterface), collection.Services.Keys);
            var service = collection.Services[typeof(ITestInterface)];
            Assert.Equal(expectedLifetime, service.Lifetime);
            Assert.Equal(typeof(TestServiceImplmentingInterface), service.Type);
            Assert.Empty(service.ConstructorParams);
        }

        [Fact]
        public void Can_register_singleton_MonoBehaviour_by_interface_type()
        {
            // Assign
            var collection = new ServiceCollection();
            var expectedLifetime = ServiceDefinitionLifetime.Singleton;

            // Act
            collection.AddSingletonMonoBehaviour<ITestInterface, TestMonoBehaviourImplementingInterface>();

            // Assert
            Assert.Contains(typeof(ITestInterface), collection.Services.Keys);
            var service = collection.Services[typeof(ITestInterface)];
            Assert.Equal(expectedLifetime, service.Lifetime);
            Assert.Equal(typeof(TestMonoBehaviourImplementingInterface), service.Type);
            Assert.Empty(service.ConstructorParams);
        }

        [Fact]
        public void Can_register_service_with_a_class_dependency()
        {
            // Assign
            var collection = new ServiceCollection();
            var expectedDependencyType = typeof(TestServiceWithoutInterface);

            // Act
            collection.AddService<TestServiceWithClassDependency>();

            // Assert
            var service = collection.Services[typeof(TestServiceWithClassDependency)];
            Assert.NotNull(service);
            Assert.NotEmpty(service.ConstructorParams);
            Assert.Contains(expectedDependencyType, service.ConstructorParams);
        }

        [Fact]
        public void Can_register_service_with_an_interface_dependency()
        {
            // Assign
            var collection = new ServiceCollection();
            var expectedDependencyType = typeof(ITestInterface);

            // Act
            collection.AddService<TestServiceWithInterfaceDependency>();

            // Assert
            var service = collection.Services[typeof(TestServiceWithInterfaceDependency)];
            Assert.NotNull(service);
            Assert.NotEmpty(service.ConstructorParams);
            Assert.Contains(expectedDependencyType, service.ConstructorParams);
        }

        [Fact]
        public void Can_register_service_with_multiple_dependencies()
        {
            // Assign
            var collection = new ServiceCollection();
            var expectedDependencyType1 = typeof(ITestInterface);
            var expectedDependencyType2 = typeof(TestServiceWithoutInterface);

            // Act
            collection.AddService<TestServiceWithInterfaceAndClassDependencies>();

            // Assert
            var service = collection.Services[typeof(TestServiceWithInterfaceAndClassDependencies)];
            Assert.NotNull(service);
            Assert.NotEmpty(service.ConstructorParams);
            Assert.Contains(expectedDependencyType1, service.ConstructorParams);
            Assert.Contains(expectedDependencyType2, service.ConstructorParams);
        }

        [Fact]
        public void Should_not_allow_registering_multiple_MonoBehaviours_by_the_same_class_type()
        {
            // Assign
            var collection = new ServiceCollection();

            // Act
            collection.AddMonoBehaviour<TestMonoBehaviourWithoutInterface>();

            // Assert
            Assert.Throws<ServiceAlreadyRegisteredException>(() => collection.AddMonoBehaviour<TestMonoBehaviourWithoutInterface>());
        }

        [Fact]
        public void Should_not_allow_registering_multiple_services_by_the_same_class_type()
        {
            // Assign
            var collection = new ServiceCollection();

            // Act
            collection.AddService<TestServiceWithoutInterface>();

            // Assert
            Assert.Throws<ServiceAlreadyRegisteredException>(() => collection.AddService<TestServiceWithoutInterface>());
        }

        [Fact]
        public void Should_not_allow_registering_multiple_MonoBehaviours_by_the_same_interface_type()
        {
            // Assign
            var collection = new ServiceCollection();

            // Act
            collection.AddMonoBehaviour<ITestInterface, TestMonoBehaviourImplementingInterface>();

            // Assert
            Assert.Throws<ServiceAlreadyRegisteredException>(() => collection.AddMonoBehaviour<ITestInterface, AnotherTestMonoBehaviourImplementingInterface>());
        }

        [Fact]
        public void Should_not_allow_registering_multiple_services_by_the_same_interface_type()
        {
            // Assign
            var collection = new ServiceCollection();

            // Act
            collection.AddService<ITestInterface, TestServiceImplmentingInterface>();

            // Assert
            Assert.Throws<ServiceAlreadyRegisteredException>(() => collection.AddService<ITestInterface, AnotherTestServiceImplementingInterface>());
        }

        [Fact]
        public void Should_not_allow_registering_a_service_with_multiple_constructors()
        {
            // Assign
            var collection = new ServiceCollection();

            // Assert
            Assert.Throws<ServiceHasTooManyConstructorsException>(() => collection.AddService<TestServiceWithMultipleConstructors>());
        }
    }
}
