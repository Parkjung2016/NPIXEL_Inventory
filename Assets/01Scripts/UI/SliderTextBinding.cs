using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderTextBinding : MonoBehaviour
{
    [SerializeField] private Slider slider;
    private TMP_Text _text;

    private void OnValidate()
    {
        if (slider != null)
        {
            _text = GetComponent<TMP_Text>();
            SubscribeEvent();
        }
    }

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        SubscribeEvent();
    }

    private void SubscribeEvent()
    {
        slider.onValueChanged.RemoveListener(HandleValueChanged);
        slider.onValueChanged.AddListener(HandleValueChanged);
    }

    private void HandleValueChanged(float value)
    {
        if (_text != null)
        {
            _text.text = $"{value}/{slider.maxValue}";
        }
    }
}