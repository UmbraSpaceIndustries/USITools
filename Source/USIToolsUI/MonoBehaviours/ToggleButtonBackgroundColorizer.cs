using UnityEngine;
using UnityEngine.UI;

namespace USIToolsUI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleButtonBackgroundColorizer : MonoBehaviour
    {
        private Color _normalColor;
        private Toggle _toggle;

        [SerializeField]
        private Color PressedColor;
        
        private void Start()
        {
            _toggle = gameObject.GetComponent<Toggle>();
            _normalColor = _toggle.colors.normalColor;
        }

        public void OnValueChanged(bool isOn)
        {
            var colors = _toggle.colors;
            colors.normalColor = isOn ?
                PressedColor :
                _normalColor;
            _toggle.colors = colors;
        }
    }
}
