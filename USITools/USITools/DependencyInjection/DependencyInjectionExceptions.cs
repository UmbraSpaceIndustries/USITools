using System;

namespace USITools
{
    /// <summary>
    /// Thrown when a service class has already been registered in an <see cref="IServiceCollection"/>.
    /// </summary>
    public class ServiceAlreadyRegisteredException : Exception
    {
        private static string messageTemplate = "{0} is already registered.";

        public ServiceAlreadyRegisteredException(Type type)
            : base(string.Format(messageTemplate, type.Name)) { }
    }

    /// <summary>
    /// Thrown when an attempt is made to register a service class with multiple constructors in an <see cref="IServiceCollection"/>.
    /// </summary>
    public class ServiceHasTooManyConstructorsException : Exception
    {
        private static string messageTemplate
            = "Cannot register {0}. Only classes with a single constructor can be used as a service.";

        public ServiceHasTooManyConstructorsException(Type type)
            : base(string.Format(messageTemplate, type.Name)) { }
    }

    /// <summary>
    /// Thrown when an attempt is made to register a MonoBehaviour service class with a parameterized constructor in an <see cref="IServiceCollection"/>.
    /// </summary>
    public class MonoBehaviourServiceCannotHaveParameterizedConstructorException : Exception
    {
        private static string messageTemplate
            = "Cannot register {0}. MonoBehaviours cannot accept dependencies via constructor. Setup dependencies in Awake or Start methods.";

        public MonoBehaviourServiceCannotHaveParameterizedConstructorException(Type type)
            : base(string.Format(messageTemplate, type.Name)) { }
    }

    /// <summary>
    /// Thrown by <see cref="IServiceManager"/> when a request is made for a service
    ///   that was not registered in its <see cref="IServiceCollection"/>.
    /// </summary>
    public class ServiceNotRegisteredException : Exception
    {
        private static string messageTemplate = "{0} is not registered as a service.";

        public ServiceNotRegisteredException(Type type)
            : base(string.Format(messageTemplate, type.Name)) { }
    }

    /// <summary>
    /// Thrown by <see cref="IServiceManager"/> when a request is made for a service
    ///   that has a dependency which is not registered in its <see cref="IServiceCollection"/>.
    /// </summary>
    public class ServiceDependencyNotRegisteredException : Exception
    {
        private static string messageTemplate = "{0} is dependent on {1}, which is not registered as a service.";

        public ServiceDependencyNotRegisteredException(Type type, Type dependency)
            : base(string.Format(messageTemplate, type.Name, dependency.Name)) { }
    }

    /// <summary>
    /// Thrown by <see cref="ServiceManager"/> when a request is made for a <see cref="MonoBehaviour"/>
    ///   service that isn't a <see cref="MonoBehaviour"/>.
    /// </summary>
    public class ServiceNotAssignableFromMonoBehaviourException : Exception
    {
        private static string messageTemplate = "{0} is not a MonoBehaviour.";

        public ServiceNotAssignableFromMonoBehaviourException(Type type)
            : base(string.Format(messageTemplate, type.Name)) { }
    }
}
