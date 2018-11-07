using UnityEngine;

namespace USITools
{
    /// <summary>
    /// Classes that want to participate in automatic dependency injection, but cannot
    ///   accept constructor parameters, should implement this interface instead.
    /// </summary>
    /// <remarks>
    /// The primary use case for this interface is for <see cref="MonoBehaviour"/>s,
    ///   since they can't accept dependencies via constructor.
    /// </remarks>
    public interface IDependencyService
    {
        void SetServiceManager(IServiceManager serviceManager);
    }
}
