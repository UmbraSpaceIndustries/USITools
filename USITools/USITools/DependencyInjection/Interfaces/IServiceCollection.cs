using System;
using System.Collections.Generic;
using UnityEngine;

namespace USITools
{
    public interface IServiceCollection
    {
        Dictionary<Type, ServiceDefinition> Services { get; }
        IServiceCollection AddService<U, T>() where T : class, U;
        IServiceCollection AddService<T>() where T : class;
        IServiceCollection AddMonoBehaviour<U, T>() where T : MonoBehaviour, U;
        IServiceCollection AddMonoBehaviour<T>() where T : MonoBehaviour;
        IServiceCollection AddSingletonService<U, T>() where T : class, U;
        IServiceCollection AddSingletonService<T>() where T : class;
        IServiceCollection AddSingletonMonoBehaviour<T>() where T : MonoBehaviour;
        IServiceCollection AddSingletonMonoBehaviour<U, T>() where T : MonoBehaviour, U;
    }
}
