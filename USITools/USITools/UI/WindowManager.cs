using System;
using System.Collections.Generic;
using UnityEngine;
using USIToolsUI.Interfaces;

namespace USITools
{
    /// <summary>
    /// Manages windows built using Unity UI and imported into KSP via asset bundles.
    /// </summary>
    /// <remarks>
    /// Check out this excellent
    /// <a href="https://forum.kerbalspaceprogram.com/index.php?/topic/151354-unity-ui-creation-tutorial/">KSP forum post</a>
    /// on making and importing UI elements for KSP. Thanks DMagic! 
    /// </remarks>
    public class WindowManager : IPrefabInstantiator
    {
        private readonly Dictionary<Type, GameObject> _prefabs
            = new Dictionary<Type, GameObject>();
        private readonly EventData<GameScenes>.OnEvent _sceneChangeDelegate;
        private readonly Dictionary<Type, GameObject> _windows
            = new Dictionary<Type, GameObject>();

        public WindowManager()
        {
            // Close windows when game scene changes
            _sceneChangeDelegate = new EventData<GameScenes>.OnEvent(CloseWindows);
            GameEvents.onGameSceneLoadRequested.Add(_sceneChangeDelegate);
        }

        /// <summary>
        /// Handler for <see cref="GameEvents.onGameSceneLoadRequested"/> event.
        /// </summary>
        /// <param name="target"></param>
        public void CloseWindows(GameScenes target)
        {
            foreach (var window in _windows)
            {
                if (window.Value != null)
                {
                    if (window.Value.GetComponent(window.Key) is IWindow windowComponent)
                    {
                        windowComponent.Reset();
                    }
                    window.Value.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Get the window instance for the requested type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWindow<T>()
            where T : IWindow
        {
            var type = typeof(T);
            if (!_windows.ContainsKey(type))
            {
                return default;
            }
            var window = _windows[type];
            return window.GetComponent<T>();
        }

        /// <summary>
        /// Use this for things like lists, table rows, etc. in your UI.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent">The object the prefab should be parented to.</param>
        /// <returns></returns>
        public T InstantiatePrefab<T>(Transform parent)
        {
            var type = typeof(T);
            if (parent == null || !_prefabs.ContainsKey(type))
            {
                return default;
            }

            var prefab = _prefabs[type];
            var obj = GameObject.Instantiate(prefab, parent);
            var component = obj.GetComponent<T>();
            return component;
        }

        /// <summary>
        /// Register a prefab for later use by <see cref="InstantiatePrefab{T}(Transform)"/>.
        /// </summary>
        /// <remarks>
        /// This is meant for smaller, iterable UI prefabs placed inside the main window.
        /// Use <see cref="RegisterWindow{T}(GameObject)"/> to register your main window.
        /// Prefabs can only be loaded from asset bundles once, so they need to be cached if
        /// we want to be able to instantiate them later. That's the purpose of this method.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefab">The prefab to register.</param>
        public void RegisterPrefab<T>(GameObject prefab)
        {
            var type = typeof(T);
            if (type == null || prefab == null || _prefabs.ContainsKey(type))
            {
                return;
            }
            var component = prefab.GetComponent<T>();
            if (component == null)
            {
                throw new Exception(
                    $"WindowManager.RegisterPrefab: Prefab does not contain a {type.Name} component.");
            }
            _prefabs.Add(type, prefab);
        }

        /// <summary>
        /// Register a UI window prefab and its controller.
        /// </summary>
        /// <remarks>
        /// Window prefabs must contain a <see cref="MonoBehaviour"/> that implements <see cref="IWindow"/>
        /// in order to be used with <see cref="WindowManager"/>.
        /// </remarks>
        /// <typeparam name="T">The <see cref="IWindow"/> that controls this UI.</typeparam>
        /// <param name="prefab">The UI prefab.</param>
        public void RegisterWindow<T>(GameObject prefab)
            where T : IWindow
        {
            var type = typeof(T);
            if (type == null || prefab == null || _windows.ContainsKey(type))
            {
                return;
            }
            var obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(MainCanvasUtil.MainCanvas.transform);
            obj.SetActive(false);

            _windows.Add(type, obj);
        }
    }
}
