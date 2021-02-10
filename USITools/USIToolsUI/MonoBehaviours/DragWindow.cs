using UnityEngine;
using UnityEngine.EventSystems;
using USIToolsUI.Interfaces;

namespace USIToolsUI
{
    /// <summary>
    /// Add this to a <see cref="GameObject"/> in your window prefab to enable window dragging
    /// and to make it the drag handle for the window.
    /// </summary>
    /// <remarks>
    /// Normally this would be added to something like an app bar at the top of the window.
    /// </remarks>
    public class DragWindow
        : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        #region Unity editor fields
#pragma warning disable IDE0044
#pragma warning disable 0649

        [SerializeField]
        private AbstractWindow window;

#pragma warning restore 0649
#pragma warning restore IDE0044
        #endregion

        private void Awake()
        {
            if (window == null)
            {
                Debug.LogError(
                    $"[USIToolsUI] {gameObject.name} has a {nameof(DragWindow)} component with missing {nameof(IWindow)} reference.");
                enabled = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            // TODO - Add some code to prevent the window from being dragged outside the bounds of the canvas
            window.RectTransform.anchoredPosition
                += eventData.delta / window.Canvas.scaleFactor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Bring the selected window to the foreground
            window.RectTransform.SetAsLastSibling();
        }
    }
}
