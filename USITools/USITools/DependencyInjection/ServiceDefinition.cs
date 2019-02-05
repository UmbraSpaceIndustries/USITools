using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace USITools
{
    /// <summary>
    /// Used by <see cref="IServiceManager"/> to determine the intended lifetime of a requested service.
    /// </summary>
    /// <remarks>
    /// Scoped services aren't a thing yet. We'll make them a thing if the need ever arises.
    /// </remarks>
    public enum ServiceDefinitionLifetime { Scoped, Singleton, Transient }

    /// <summary>
    /// Used by <see cref="IServiceManager"/> to determine how to instantiate a requested service.
    /// </summary>
    public class ServiceDefinition
    {
        public Type Type { get; private set; }
        public ServiceDefinitionLifetime Lifetime { get; private set; }
        public ConstructorInfo ConstructorInfo { get; private set; }
        public List<Type> ConstructorParams { get; private set; }

        public ServiceDefinition(Type type, ServiceDefinitionLifetime lifetime, bool isMonoBehaviour = false)
        {
            Type = type;
            Lifetime = lifetime;

            GetConstructorInfo(isMonoBehaviour);
        }

        private void GetConstructorInfo(bool isMonoBehaviour)
        {
            var constructors = Type.GetConstructors();
            if (constructors.Length > 1)
                throw new ServiceHasTooManyConstructorsException(Type);

            ConstructorInfo = constructors[0];
            var constructorParams = ConstructorInfo.GetParameters();

            // MonoBehaviours cannot have parameterized constructors
            if (isMonoBehaviour && constructorParams.Count() > 0)
                throw new MonoBehaviourServiceCannotHaveParameterizedConstructorException(Type);

            // We cache the constructor params to avoid unnecessary Linq and foreach queries
            //   every time a service instance is requested.
            ConstructorParams = constructorParams
                .Select(p => p.ParameterType)
                .ToList();
        }
    }
}
