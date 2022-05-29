using UnityEngine;
using USIToolsUI.Interfaces;

namespace USIToolsUI
{
    public abstract class AbstractWindow
        : MonoBehaviour, IWindow
    {
        public virtual Canvas Canvas { get; }

        private RectTransform _rectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }

        public abstract void Reset();
    }
}
