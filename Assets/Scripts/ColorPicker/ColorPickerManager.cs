using System.Collections.Generic;
using TDSTK;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker
{
    public class ColorPickerManager : MonoBehaviour
    {
        [SerializeField] private HSVPicker.ColorPicker picker;
        [SerializeField] private Dropdown dropdown;
        [SerializeField] private Button closeButton;
        private UnitPlayer _unitPlayer;
        private Renderer[] _renderers;

        private void OnValidate()
        {
            if (!picker)
            {
                picker = GetComponent<HSVPicker.ColorPicker>();
            }
        }

        private void Awake()
        {
            _unitPlayer = FindObjectOfType<UnitPlayer>();
            _renderers = _unitPlayer.GetComponentsInChildren<Renderer>();
        }

        private void Start()
        {
            dropdown.options = new List<Dropdown.OptionData>()
            {
                new("Color 1"),
                new("Color 2"),
            };
            dropdown.onValueChanged.AddListener(value =>
            {
                picker.CurrentColor = _renderers[0].material.GetColor($"_Color{value + 1}");
            });
            picker.CurrentColor = _renderers[0].material.GetColor($"_Color{dropdown.value + 1}");
            picker.onValueChanged.AddListener(color =>
            {
                foreach (var renderer1 in _renderers)
                {
                    renderer1.material.SetColor($"_Color{dropdown.value + 1}", color);
                }
            });

            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }
    }
}
