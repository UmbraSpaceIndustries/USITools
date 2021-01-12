using Xunit;

namespace USITools.Tests.Unit
{
    public class ServiceManager_Tests
    {
        private readonly IServiceCollection _transientServiceCollection;
        private readonly IServiceCollection _singletonServiceCollection;
        private readonly IServiceCollection _brokenServiceCollection;

        public ServiceManager_Tests()
        {
            var mockTransientServiceCollection = new MockTransientServiceCollection();
            _transientServiceCollection = mockTransientServiceCollection.ServiceCollection;

            var mockSingletonServiceCollection = new MockSingletonServiceCollection();
            _singletonServiceCollection = mockSingletonServiceCollection.ServiceCollection;

            var mockBrokenServiceCollection = new BrokenMockTransientServiceCollection();
            _brokenServiceCollection = mockBrokenServiceCollection.ServiceCollection;
        }

        [Fact]
        public void Can_get_a_singleton_instance_of_a_service()
        {
            // Assign
            IServiceManager serviceManager = new ServiceManager(_singletonServiceCollection);

            // Act
            var firstService = serviceManager.GetService<TestServiceWithoutInterface>();
            var secondService = serviceManager.GetService<TestServiceWithoutInterface>();

            // Assert
            Assert.NotNull(firstService);
            Assert.NotNull(secondService);
            Assert.Equal(firstService, secondService);
        }

        [Fact]
        public void Can_get_a_transient_instance_of_a_service()
        {
            // Assign
            IServiceManager serviceManager = new ServiceManager(_transientServiceCollection);

            // Act
            var firstService = serviceManager.GetService<TestServiceWithoutInterface>();
            var secondService = serviceManager.GetService<TestServiceWithoutInterface>();

            // Assert
            Assert.NotNull(firstService);
            Assert.NotNull(secondService);
            Assert.NotEqual(firstService, secondService);
            Assert.IsType<TestServiceWithoutInterface>(firstService);
        }

        [Fact]
        public void Can_get_a_service_instance_by_interface_type()
        {
            // Assign
            IServiceManager serviceManager = new ServiceManager(_transientServiceCollection);

            // Act
            var service = serviceManager.GetService<ITestInterface>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<TestServiceImplmentingInterface>(service);
        }

        [Fact]
        public void Can_get_a_service_instance_with_interface_dependencies()
        {
            // Assign
            IServiceManager serviceManager = new ServiceManager(_transientServiceCollection);

            // Act
            var service = serviceManager.GetService<TestServiceWithInterfaceDependency>();

            // Assert
            Assert.NotNull(service);
            var serviceWithDependencies = Assert.IsType<TestServiceWithInterfaceDependency>(service);
            var dependency = serviceWithDependencies.Dependency;
            Assert.NotNull(dependency);
            Assert.IsType<TestServiceImplmentingInterface>(dependency);
        }

        [Fact]
        public void Can_get_a_service_instance_with_class_dependencies()
        {
            // Assign
            IServiceManager serviceManager = new ServiceManager(_transientServiceCollection);

            // Act
            var service = serviceManager.GetService<TestServiceWithClassDependency>();

            // Assert
            Assert.NotNull(service);
            var serviceWithDependencies = Assert.IsType<TestServiceWithClassDependency>(service);
            var dependency = serviceWithDependencies.Dependency;
            Assert.NotNull(dependency);
        }

        [Fact]
        public void Can_get_a_service_instance_with_multiple_dependencies()
        {
            // Assign
            IServiceManager serviceManager = new ServiceManager(_transientServiceCollection);

            // Act
            var service = serviceManager.GetService<TestServiceWithInterfaceAndClassDependencies>();

            // Assert
            Assert.NotNull(service);
            var serviceWithDependencies = Assert.IsType<TestServiceWithInterfaceAndClassDependencies>(service);
            var classDependency = serviceWithDependencies.ClassDependency;
            var interfaceDependency = serviceWithDependencies.InterfaceDependency;
            Assert.NotNull(classDependency);
            Assert.NotNull(interfaceDependency);
            Assert.IsType<TestServiceWithoutInterface>(classDependency);
            Assert.IsAssignableFrom<ITestInterface>(interfaceDependency);
        }

        [Fact]
        public void Services_with_transient_dependencies_should_get_a_transient_instance()
        {
            // Assign
            IServiceManager serviceManager = new ServiceManager(_transientServiceCollection);

            // Act
            var dependency = serviceManager.GetService<ITestInterface>();
            var service = serviceManager.GetService<TestServiceWithInterfaceDependency>();

            // Assert
            Assert.NotNull(service);
            Assert.NotNull(dependency);
            Assert.NotEqual(dependency, service.Dependency);
        }

        [Fact]
        public void Services_with_singleton_dependencies_should_get_the_singleton_instance()
        {
            // Assign
            IServiceManager serviceManager = new ServiceManager(_singletonServiceCollection);

            // Act
            var dependency = serviceManager.GetService<ITestInterface>();
            var service = serviceManager.GetService<TestServiceWithInterfaceDependency>();

            // Assert
            Assert.NotNull(service);
            Assert.NotNull(dependency);
            Assert.Equal(dependency, service.Dependency);
        }

        [Fact]
        public void Should_throw_exception_when_unregistered_service_is_requested()
        {
            // Assign
            IServiceManager serviceManager = new ServiceManager(_transientServiceCollection);

            // Assert
            Assert.Throws<ServiceNotRegisteredException>(() => serviceManager.GetService<string>());
        }

        [Fact]
        public void Should_throw_exception_when_dependencies_arent_registered_for_a_service()
        {
            // Assign
            IServiceManager serviceManager = new ServiceManager(_brokenServiceCollection);

            // Assert
            Assert.Throws<ServiceDependencyNotRegisteredException>(() => serviceManager.GetService<TestServiceWithInterfaceDependency>());
        }
    }
}
