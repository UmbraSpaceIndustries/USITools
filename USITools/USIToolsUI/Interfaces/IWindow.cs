using UnityEngine;

namespace USIToolsUI.Interfaces
{
    /// <summary>
    /// Window controllers must implement this interface in order to
    /// be managed via the WindowManager in USITools.
    /// </summary>
    public interface IWindow
    {
        Canvas Canvas { get; }
        RectTransform RectTransform { get; }
        void Reset();
    }
}
