using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeInventoryTypeButton : MonoBehaviour
{
    private event Action<ItemType> OnChangedInventoryType;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private SoundDataSO buttonClickSound;

    private Color _originColor;
    private Button _button;
    private Image _image;
    private TextMeshProUGUI _text;

    private ItemType _itemType;


    public void Init(ItemType itemType, Action<ItemType> ChangedInventoryTypeCallBack)
    {
        OnChangedInventoryType = ChangedInventoryTypeCallBack;
        _text = transform.Find("Text_InventoryType").GetComponent<TextMeshProUGUI>();
        _itemType = itemType;
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
        _originColor = _image.color;
        _button.onClick.AddListener(HandleClickChangeInventoryTypeButton);
        _text.SetText(itemType.ToString());
    }

    private void HandleClickChangeInventoryTypeButton()
    {
        SoundManager.CreateSoundBuilder().Play(buttonClickSound);
        OnChangedInventoryType?.Invoke(_itemType);
    }

    public void SetSelected(bool isSelected)
    {
        Color color = isSelected ? selectedColor : _originColor;
        _image.color = color;
    }
}