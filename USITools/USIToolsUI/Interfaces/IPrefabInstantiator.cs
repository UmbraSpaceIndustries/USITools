using UnityEngine;

namespace USIToolsUI.Interfaces
{
    /// <summary>
    /// Used by window objects to instantiate child prefabs.
    /// </summary>
    /// <remarks>
    /// UI assemblies cannot directly reference KSP assemblies (because PartTools??),
    /// so they must be provided at runtime via interfaces.
    /// </remarks>
    public interface IPrefabInstantiator
    {
        T InstantiatePrefab<T>(Transform parent);
    }
}
