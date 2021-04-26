using System;
using System.Collections.Generic;
using UnityEngine;

namespace USITools
{
    public enum PrefabType { Prefab, Window }

    public class PrefabDefinition
    {
        public string Name { get; private set; }
        public Type MonobehaviourType { get; private set; }
        public PrefabType PrefabType { get; private set; }

        public PrefabDefinition(
            string prefabName,
            Type monobehaviourType,
            PrefabType prefabType)
        {
            Name = prefabName;
            MonobehaviourType = monobehaviourType;
            PrefabType = prefabType;
        }

        public PrefabDefinition(Type monobehaviourType, PrefabType prefabType)
            : this(monobehaviourType.Name, monobehaviourType, prefabType)
        {
        }
    }

    public class PrefabDefinition<T> : PrefabDefinition
        where T: MonoBehaviour
    {
        public PrefabDefinition(PrefabType prefabType)
            : base(typeof(T), prefabType)
        {
        }
    }

    public class PrefabManager
    {
        private readonly WindowManager _windowManager;
        private readonly List<string> _loadedAssetBundles
            = new List<string>();

        public PrefabManager(WindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public void LoadAssetBundle(
            string filepath,
            List<PrefabDefinition> prefabDefinitions)
        {
            if (!_loadedAssetBundles.Contains(filepath) &&
                _windowManager != null &&
                !string.IsNullOrEmpty(filepath) &&
                prefabDefinitions != null &&
                prefabDefinitions.Count > 0)
            {
                try
                {
                    var assetBundle = AssetBundle.LoadFromFile(filepath);
                    foreach (var definition in prefabDefinitions)
                    {
                        var prefab = assetBundle
                            .LoadAsset<GameObject>(definition.Name);
                        switch (definition.PrefabType)
                        {
                            case PrefabType.Window:
                                _windowManager.RegisterWindow(prefab, definition.MonobehaviourType);
                                break;
                            case PrefabType.Prefab:
                            default:
                                _windowManager.RegisterPrefab(prefab, definition.MonobehaviourType);
                                break;
                        }
                    }
                    assetBundle.Unload(false);
                    _loadedAssetBundles.Add(filepath);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[USITools] {nameof(PrefabManager)}: {ex.Message}");
                }
            }
        }
    }
}
