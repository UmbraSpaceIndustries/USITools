using System;
using System.Collections.Generic;
using UnityEngine;

namespace USITools
{
    /// <summary>
    /// Use this to register services that <see cref="IServiceManager"/> can provide.
    /// </summary>
    public class ServiceCollection : IServiceCollection
    {
        public Dictionary<Type, ServiceDefinition> Services { get; private set; }
            = new Dictionary<Type, ServiceDefinition>();

        private void DoAddService<U, T>(ServiceDefinitionLifetime lifetime, bool isMonoBehaviour = false)
        {
            if (Services.ContainsKey(typeof(U)))
                throw new ServiceAlreadyRegisteredException(typeof(U));

            var definition = new ServiceDefinition(typeof(T), lifetime, isMonoBehaviour);

            Services.Add(typeof(U), definition);
        }

        public IServiceCollection AddService<U, T>()
            where T : class, U
        {
            DoAddService<U, T>(ServiceDefinitionLifetime.Transient);
            return this;
        }

        public IServiceCollection AddService<T>()
            where T : class
        {
            return AddService<T, T>();
        }

        public IServiceCollection AddMonoBehaviour<U, T>()
            where T : MonoBehaviour, U
        {
            DoAddService<U, T>(ServiceDefinitionLifetime.Transient, true);
            return this;
        }

        public IServiceCollection AddMonoBehaviour<T>()
            where T : MonoBehaviour
        {
            return AddMonoBehaviour<T, T>();
        }

        public IServiceCollection AddSingletonService<U, T>()
            where T : class, U
        {
            DoAddService<U, T>(ServiceDefinitionLifetime.Singleton);
            return this;
        }

        public IServiceCollection AddSingletonService<T>()
            where T : class
        {
            return AddSingletonService<T, T>();
        }

        public IServiceCollection AddSingletonMonoBehaviour<U, T>()
            where T : MonoBehaviour, U
        {
            DoAddService<U, T>(ServiceDefinitionLifetime.Singleton, true);
            return this;
        }

        public IServiceCollection AddSingletonMonoBehaviour<T>()
            where T : MonoBehaviour
        {
            return AddSingletonMonoBehaviour<T, T>();
        }
    }
}
