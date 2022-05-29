using System;
using System.Collections.Generic;
using UnityEngine;

namespace USITools
{
    /// <summary>
    /// Provides instances of service classes, dependency injection-style.
    /// </summary>
    /// <remarks>
    /// This is most useful for managing singletons and making classes
    ///   easier to unit test. This eliminates the need to setup static
    ///   properties on every class that is intended to be used as a singleton.
    ///   It also makes it easier for transient classes to consume singletons.
    /// </remarks>
    public class ServiceManager : IServiceManager
    {
        private readonly IServiceCollection _collection;
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

        public ServiceManager(IServiceCollection collection)
        {
            _collection = collection;
        }

        public T GetService<T>()
            where T : class
        {
            return GetService(typeof(T)) as T;
        }

        public object GetService(Type type)
        {
            if (!HasRegisteredService(type))
                throw new ServiceNotRegisteredException(type);

            // Short circuit if there is already a singleton instance of this type
            if (_singletons.ContainsKey(type))
                return _singletons[type];

            var serviceDefinition = _collection.Services[type];

            object service;
            if (type.IsAssignableFrom(typeof(IDependencyService)))
            {
                service = serviceDefinition.ConstructorInfo.Invoke(new object[0]);
                (service as IDependencyService).SetServiceManager(this);
            }
            else
            {
                var serviceParams = new List<object>();
                var serviceParamTypes = serviceDefinition.ConstructorParams;
                for (int i = 0; i < serviceParamTypes.Count; i++)
                {
                    var dependencyType = serviceParamTypes[i];
                    try
                    {
                        serviceParams.Add(GetService(dependencyType));
                    }
                    catch (ServiceNotRegisteredException)
                    {
                        throw new ServiceDependencyNotRegisteredException(type, dependencyType);
                    }
                }

                service = serviceDefinition.ConstructorInfo.Invoke(serviceParams.ToArray());
            }

            if (serviceDefinition.Lifetime == ServiceDefinitionLifetime.Singleton)
            {
                _singletons.Add(type, service);
            }

            return service;
        }

        public T GetMonoBehaviour<T>()
            where T : class
        {
            return GetMonoBehaviour(typeof(T)) as T;
        }

        public MonoBehaviour GetMonoBehaviour(Type type)
        {
            if (!HasRegisteredService(type))
                throw new ServiceNotRegisteredException(type);

            // Short circuit if there is already a singleton instance of this type
            if (_singletons.ContainsKey(type))
                return _singletons[type] as MonoBehaviour;

            var service = new GameObject(type.Name + "_" + Guid.NewGuid().ToString()).AddComponent(type);

            if (type.IsAssignableFrom(typeof(IDependencyService)))
                (service as IDependencyService).SetServiceManager(this);

            var serviceDefinition = _collection.Services[type];
            if (serviceDefinition.Lifetime == ServiceDefinitionLifetime.Singleton)
            {
                _singletons.Add(type, service);
                UnityEngine.Object.DontDestroyOnLoad(service.gameObject);
            }

            return service as MonoBehaviour;
        }

        private bool HasRegisteredService(Type type)
        {
            return _collection.Services.ContainsKey(type);
        }
    }
}
